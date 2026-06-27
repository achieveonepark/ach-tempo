using Achieve.Tempo.Config;
using Achieve.Tempo.World;
using UnityEngine;

namespace Achieve.Tempo.Tension
{
    /// <summary>
    /// 긴장도 숫자(0~1) 하나를 계산한다. 디렉터는 이 숫자만 보고 다음 행동을 정한다.
    /// 세 단계: ① 두 신호를 0~1로 맞추기 → ② 합치기 → ③ 부드럽게 바꾸기.
    ///
    /// 물리 씬 없이 테스트하려고 <see cref="SampleThreat"/>·<see cref="SampleAround"/> 를 virtual 로 둔다
    /// (인터페이스 대신 갈아끼움).
    /// </summary>
    public class TensionModel
    {
        readonly TensionConfig _cfgOverride;
        protected TensionConfig Cfg => _cfgOverride ?? DataService<TensionConfig>.GetDataCenter();

        float _playerStress; // 0~1
        float _smoothed;     // 디렉터가 읽는 값

        public float Current => _smoothed;
        public float PlayerStress => _playerStress;

        public TensionModel(TensionConfig cfgOverride = null)
        {
            _cfgOverride = cfgOverride;
        }

        /// <summary>피격 시 바로 확 올림(튀어오름).</summary>
        public void OnPlayerDamaged(float dmg, float maxHp)
        {
            if (maxHp <= 0f) return;
            _playerStress = Mathf.Clamp01(_playerStress + dmg / maxHp * Cfg.DamageGain);
        }

        public void Tick(Vector3 playerPos, WorldReaction world, float dt)
        {
            TensionConfig cfg = Cfg;

            // 1) 가까운 위협일수록 크게 누적 + 2) 시간 지나면 서서히 내림
            _playerStress += SampleThreat(playerPos) * cfg.ThreatGain * dt;
            _playerStress = Mathf.Clamp01(_playerStress - cfg.StressDecay * dt);

            // 3) 주변에서 벌어지는 일 — 가까운 것만 크게 쳐서 더함
            float around = SampleAround(playerPos, world);

            // 4) 합치기: 하나만 높아도 올라가고, 둘 다면 최대 근처(1을 안 넘음)
            float a = Mathf.Clamp01(_playerStress * cfg.StressWeight);
            float b = Mathf.Clamp01(around * cfg.AroundWeight);
            float raw = 1f - (1f - a) * (1f - b);

            // 5) 부드럽게: 올라갈 땐 빠르게, 내려갈 땐 천천히
            float speed = raw > _smoothed ? cfg.RiseSpeed : cfg.FallSpeed;
            float k = 1f - Mathf.Exp(-dt / Mathf.Max(1e-4f, speed));
            _smoothed = Mathf.Lerp(_smoothed, raw, k);
        }

        /// <summary>플레이어 주변 위협을 0~1로. 기본은 물리 오버랩으로 적을 센다.</summary>
        protected virtual float SampleThreat(Vector3 p)
        {
            TensionConfig cfg = Cfg;
            float sum = 0f;
            Collider[] hits = Physics.OverlapSphere(p, cfg.ThreatRadius, cfg.ThreatMask);
            for (int i = 0; i < hits.Length; i++)
            {
                float d = Vector3.Distance(p, hits[i].transform.position);
                sum += 1f - Mathf.Clamp01(d / cfg.ThreatRadius); // 가까울수록 1
            }
            return Mathf.Clamp01(sum / cfg.ThreatSaturation);
        }

        /// <summary>주변에서 벌어지는 일을 0~1로. 가까운 큰 일일수록 크게 친다.</summary>
        protected virtual float SampleAround(Vector3 p, WorldReaction world)
        {
            TensionConfig cfg = Cfg;
            float load = 0f;
            var events = world.ActiveEvents;
            for (int i = 0; i < events.Count; i++)
            {
                ReactionEvent r = events[i];
                float d = Vector3.Distance(p, r.Position);
                if (d > cfg.AroundRadius) continue;
                load += r.Magnitude * (1f - d / cfg.AroundRadius); // 크기 × 가까운 정도
            }
            return Mathf.Clamp01(load / cfg.AroundSaturation);
        }
    }
}
