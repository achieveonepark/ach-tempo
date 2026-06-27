using System;
using System.Collections.Generic;
using Achieve.Tempo.Config;
using UnityEngine;

namespace Achieve.Tempo.World
{
    /// <summary>
    /// 월드 반응(불·물·전기가 규칙대로 서로 바뀌는 쪽). 디렉터의 존재를 전혀 모른다.
    /// 평평한 격자(NativeArray 대신 관리되는 평면 배열)에서 규칙을 적용해 매 프레임 번진다.
    /// SortedSet 은 쓰지 않는다(약속).
    ///
    /// 디렉터는 <see cref="ApplyElement"/>·<see cref="SetWind"/> 로 조건만 깔고,
    /// <see cref="ActiveEvents"/> 로 결과만 읽는다.
    /// </summary>
    public class WorldReaction
    {
        readonly int _width;
        readonly int _height;
        readonly float _cellSize;
        readonly Vector3 _origin;
        readonly ElementalState[] _cells;

        readonly List<ReactionHandle> _events = new List<ReactionHandle>(64);

        RuleData _ruleOverride;
        Vector2 _wind = Vector2.zero;
        float _windStrength;

        System.Random _rng;

        /// <summary>디렉터·긴장도 계산기가 읽는 '지금 벌어지는 일' 목록.</summary>
        public IReadOnlyList<ReactionHandle> ActiveEvents => _events;

        public int Width => _width;
        public int Height => _height;
        public float CellSize => _cellSize;
        public Vector2 Wind => _wind;
        public float WindStrength => _windStrength;

        /// <summary>지금 불타고 있는 칸 개수. 안전장치(상한)에서 본다.</summary>
        public int BurningCells { get; private set; }

        RuleData Rules => _ruleOverride ?? DataService<RuleData>.GetDataCenter();

        /// <param name="origin">격자 (0,0) 칸의 월드 좌표(XZ 평면).</param>
        /// <param name="seed">번짐 확률용 난수 시드. 테스트에서 같은 결과를 보장.</param>
        public WorldReaction(int width, int height, float cellSize = 1f,
            Vector3 origin = default, RuleData ruleOverride = null, int seed = 12345)
        {
            _width = Mathf.Max(1, width);
            _height = Mathf.Max(1, height);
            _cellSize = Mathf.Max(0.01f, cellSize);
            _origin = origin;
            _ruleOverride = ruleOverride;
            _rng = new System.Random(seed);
            _cells = new ElementalState[_width * _height];
            for (int i = 0; i < _cells.Length; i++)
                _cells[i] = ElementalState.Empty;
        }

        // ── 격자 채우기 (게임/예제가 초기 지형을 깐다) ───────────────────────────

        /// <summary>모든 칸을 같은 재질로 채운다. (예: 풀밭)</summary>
        public void Fill(string materialName)
        {
            int id = Rules.ResolveMaterialId(materialName);
            for (int i = 0; i < _cells.Length; i++)
            {
                _cells[i] = ElementalState.Empty;
                _cells[i].MaterialId = id;
            }
        }

        public void SetCell(int x, int y, string materialName)
        {
            if (!InBounds(x, y)) return;
            int idx = Index(x, y);
            _cells[idx] = ElementalState.Empty;
            _cells[idx].MaterialId = Rules.ResolveMaterialId(materialName);
        }

        public ElementalState GetCell(int x, int y)
            => InBounds(x, y) ? _cells[Index(x, y)] : ElementalState.Empty;

        // ── 디렉터가 조건을 까는 입구 ─────────────────────────────────────────────

        /// <summary>월드의 한 영역에 요소를 끼얹는다. (불씨 던지기·비 뿌리기·번개 등)</summary>
        public void ApplyElement(Vector3 pos, float radius, ElementTag element, float power)
        {
            ToCell(pos, out int cx, out int cy);
            int r = Mathf.Max(0, Mathf.CeilToInt(radius / _cellSize));
            float r2 = (radius / _cellSize) * (radius / _cellSize);

            for (int dy = -r; dy <= r; dy++)
            for (int dx = -r; dx <= r; dx++)
            {
                if (dx * dx + dy * dy > r2) continue;
                int x = cx + dx, y = cy + dy;
                if (!InBounds(x, y)) continue;
                ApplyElementToCell(Index(x, y), element, power);
            }
        }

        void ApplyElementToCell(int idx, ElementTag element, float power)
        {
            ref ElementalState c = ref _cells[idx];
            Material mat = Rules.GetMaterial(c.MaterialId);

            switch (element)
            {
                case ElementTag.Fire:
                    c.Temperature += power * 200f;
                    break;
                case ElementTag.Water:
                    c.Wetness = Mathf.Clamp01(c.Wetness + power * mat.WaterAbsorption);
                    c.Temperature -= power * 40f;
                    break;
                case ElementTag.Ice:
                    c.Temperature -= power * 120f;
                    break;
                case ElementTag.Electric:
                    c.Charge = Mathf.Clamp01(c.Charge + power * mat.Conductivity);
                    break;
                case ElementTag.Wind:
                    // 바람은 칸 상태가 아니라 전역 바람으로 다룬다.
                    break;
            }

            ApplyRules(idx, element);
        }

        /// <summary>바람 방향·세기. 불이 번지는 쪽을 정한다.</summary>
        public void SetWind(Vector2 dir, float strength)
        {
            _wind = dir.sqrMagnitude > 1e-6f ? dir.normalized : Vector2.zero;
            _windStrength = Mathf.Max(0f, strength);
        }

        // ── 매 프레임 한 바퀴 ─────────────────────────────────────────────────────

        public void Tick(float dt)
        {
            BurningCells = 0;

            // 1) 칸 상태 갱신(가열/냉각/마름) + 발화/소화 + 옆 칸으로 번짐
            //    번짐을 같은 프레임 안에서 연쇄로 타지 않게 한 칸씩 본다.
            for (int y = 0; y < _height; y++)
            for (int x = 0; x < _width; x++)
            {
                int idx = Index(x, y);
                StepCell(idx, x, y, dt);
                if (_cells[idx].OnFire) BurningCells++;
            }

            // 2) 벌어지는 일 목록: 시간 깎고 끝난 건 치운다.
            for (int i = _events.Count - 1; i >= 0; i--)
            {
                ReactionHandle h = _events[i];
                h.Ttl -= dt;
                if (h.IsAlive) _events[i] = h;
                else _events.RemoveAt(i);
            }
        }

        void StepCell(int idx, int x, int y, float dt)
        {
            ref ElementalState c = ref _cells[idx];
            Material mat = Rules.GetMaterial(c.MaterialId);

            // 젖은 칸은 서서히 마른다. 불타는 칸은 더 빨리 마르고 뜨거워진다.
            c.Wetness = Mathf.Clamp01(c.Wetness - Rules.DryRate * dt * (c.OnFire ? 4f : 1f));

            if (c.OnFire)
            {
                c.Temperature = Mathf.Max(c.Temperature, mat.IgnitionPoint + 100f);
                EmitEvent(x, y, ElementTag.Fire, 1f);
                TrySpreadFire(x, y, dt);

                // 연료가 너무 젖으면 꺼진다.
                if (c.Wetness > 0.8f) Extinguish(idx);
            }
            else
            {
                // 자연 냉각.
                c.Temperature = Mathf.MoveTowards(c.Temperature, 20f, Rules.CoolRate * dt);

                // 점화 조건: 탈 수 있고, 젖지 않았고, 점화 온도를 넘었다.
                if (mat.CanBurn && c.Wetness < 0.5f && c.Temperature >= mat.IgnitionPoint)
                    Ignite(idx, x, y);
            }

            // 충전된 전도성 칸은 방전(감전) 이벤트를 낸다.
            if (c.Charge > 0.5f && mat.CanConduct)
            {
                EmitEvent(x, y, ElementTag.Electric, c.Charge);
                c.Charge = Mathf.Clamp01(c.Charge - Rules.DischargeRate * dt);
            }
        }

        void TrySpreadFire(int srcX, int srcY, float dt)
        {
            for (int dy = -1; dy <= 1; dy++)
            for (int dx = -1; dx <= 1; dx++)
            {
                if (dx == 0 && dy == 0) continue;
                int nx = srcX + dx, ny = srcY + dy;
                if (!InBounds(nx, ny)) continue;

                int nIdx = Index(nx, ny);
                ref ElementalState n = ref _cells[nIdx];
                if (n.OnFire) continue;

                Material nMat = Rules.GetMaterial(n.MaterialId);
                if (!nMat.CanBurn || n.Wetness >= 0.5f) continue;

                // 바람이 부는 쪽일수록 더 잘 번진다.
                float windBias = 1f;
                if (_windStrength > 0f)
                {
                    float align = Vector2.Dot(_wind, new Vector2(dx, dy).normalized);
                    windBias = 1f + align * _windStrength;
                }

                float p = nMat.Flammability * Rules.SpreadRate * windBias * dt;
                if (p > 0f && _rng.NextDouble() < p)
                    n.Temperature += nMat.IgnitionPoint; // 다음 StepCell 에서 발화
            }
        }

        void Ignite(int idx, int x, int y)
        {
            _cells[idx].OnFire = true;
            _cells[idx].Frozen = false;
            EmitEvent(x, y, ElementTag.Fire, 1f);
        }

        void Extinguish(int idx)
        {
            _cells[idx].OnFire = false;
            _cells[idx].Temperature = Mathf.Min(_cells[idx].Temperature, 80f);
        }

        // 규칙표(요소→요소, 요소→상태)를 한 칸에 적용. 직접 끼얹는 순간 발동한다.
        void ApplyRules(int idx, ElementTag trigger)
        {
            ref ElementalState c = ref _cells[idx];
            Material mat = Rules.GetMaterial(c.MaterialId);
            IReadOnlyList<ReactionRule> rules = Rules.Rules;

            for (int i = 0; i < rules.Count; i++)
            {
                ReactionRule rule = rules[i];
                if (rule.Trigger != trigger) continue;
                if (!rule.MatchesMaterial(mat.Name)) continue;
                if (_rng.NextDouble() > rule.Probability) continue;

                switch (rule.Effect)
                {
                    case StateEffect.Ignite:
                        if (mat.CanBurn && c.Wetness < 0.5f) c.OnFire = true;
                        break;
                    case StateEffect.Extinguish:
                        c.OnFire = false;
                        break;
                    case StateEffect.Wet:
                        c.Wetness = Mathf.Clamp01(c.Wetness + 0.5f);
                        c.OnFire = false;
                        break;
                    case StateEffect.Dry:
                        c.Wetness = Mathf.Clamp01(c.Wetness - 0.5f);
                        break;
                    case StateEffect.Freeze:
                        c.Frozen = true;
                        c.OnFire = false;
                        break;
                    case StateEffect.Thaw:
                        c.Frozen = false;
                        break;
                    case StateEffect.Charge:
                        c.Charge = Mathf.Clamp01(c.Charge + 0.5f);
                        break;
                    case StateEffect.Discharge:
                        c.Charge = 0f;
                        break;
                }
            }
        }

        // 같은 자리에 이미 비슷한 일이 있으면 시간만 늘려 합친다(가까운 연쇄를 한 건으로).
        void EmitEvent(int x, int y, ElementTag kind, float magnitude)
        {
            Vector3 pos = ToWorld(x, y);
            float mergeDist = _cellSize * 1.5f;

            for (int i = 0; i < _events.Count; i++)
            {
                if (_events[i].Kind != kind) continue;
                if ((_events[i].Position - pos).sqrMagnitude > mergeDist * mergeDist) continue;

                ReactionHandle merged = _events[i];
                merged.Magnitude = Mathf.Max(merged.Magnitude, magnitude);
                merged.Ttl = Mathf.Max(merged.Ttl, Rules.EventTtl);
                _events[i] = merged;
                return;
            }

            _events.Add(new ReactionHandle
            {
                Position = pos,
                Magnitude = magnitude,
                Ttl = Rules.EventTtl,
                Kind = kind,
            });
        }

        // ── 좌표 변환 ──────────────────────────────────────────────────────────────

        int Index(int x, int y) => x + y * _width;
        bool InBounds(int x, int y) => x >= 0 && x < _width && y >= 0 && y < _height;

        void ToCell(Vector3 world, out int x, out int y)
        {
            x = Mathf.FloorToInt((world.x - _origin.x) / _cellSize);
            y = Mathf.FloorToInt((world.z - _origin.z) / _cellSize);
        }

        Vector3 ToWorld(int x, int y)
            => new Vector3(_origin.x + (x + 0.5f) * _cellSize, _origin.y, _origin.z + (y + 0.5f) * _cellSize);
    }
}
