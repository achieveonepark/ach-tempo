using System;
using System.Collections.Generic;
using Achieve.Tempo.World;
using UnityEngine;

namespace Achieve.Tempo.Config
{
    /// <summary>
    /// 월드 반응이 보는 데이터센터: 재질 레지스트리 + 규칙표 + 번짐 관련 기준값.
    /// 기본 생성자에 '풀밭에서 불이 번지고 비가 끄는' 최소 규칙이 들어 있어 바로 동작한다.
    /// </summary>
    [Serializable]
    public class RuleData
    {
        [Header("재질 레지스트리 (이름으로 찾고 인덱스로 보관)")]
        [SerializeField] List<MaterialDef> _materials = new List<MaterialDef>();

        [Header("반응 규칙표")]
        [SerializeField] List<ReactionRule> _rules = new List<ReactionRule>();

        [Header("번짐 기준값")]
        [Tooltip("옆 칸으로 번지는 기본 속도(확률 배수).")]
        public float SpreadRate = 0.6f;
        [Tooltip("불 안 붙은 칸이 식는 속도(도/초).")]
        public float CoolRate = 60f;
        [Tooltip("젖은 칸이 마르는 속도(/초).")]
        public float DryRate = 0.05f;
        [Tooltip("충전된 칸이 방전되는 속도(/초).")]
        public float DischargeRate = 0.5f;
        [Tooltip("벌어지는 일 한 건의 기본 지속 시간(초).")]
        public float EventTtl = 1.5f;

        [NonSerialized] Dictionary<string, int> _index;

        public IReadOnlyList<ReactionRule> Rules => _rules;
        public IReadOnlyList<MaterialDef> Materials => _materials;

        public RuleData()
        {
            BuildDefaults();
        }

        /// <summary>이름으로 재질 인덱스를 찾고, 없으면 새로 등록한다.</summary>
        public int ResolveMaterialId(string name)
        {
            EnsureIndex();
            if (string.IsNullOrEmpty(name)) return -1;
            if (_index.TryGetValue(name, out int id)) return id;

            id = _materials.Count;
            _materials.Add(new MaterialDef(name, 0f, 0f));
            _index[name] = id;
            return id;
        }

        public MaterialDef GetMaterial(int id)
            => (id >= 0 && id < _materials.Count) ? _materials[id] : MaterialDef.Empty;

        public void AddMaterial(MaterialDef material)
        {
            EnsureIndex();
            if (_index.ContainsKey(material.Name)) return;
            _index[material.Name] = _materials.Count;
            _materials.Add(material);
        }

        public void AddRule(ReactionRule rule) => _rules.Add(rule);

        void EnsureIndex()
        {
            if (_index != null && _index.Count == _materials.Count) return;
            _index = new Dictionary<string, int>(_materials.Count);
            for (int i = 0; i < _materials.Count; i++)
                _index[_materials[i].Name] = i;
        }

        // 야숨식 세 가지 기본 규칙을 출발점으로 한 최소 데이터.
        void BuildDefaults()
        {
            _materials.Clear();
            _rules.Clear();

            //              이름      탈성  전도  점화온도 어는점 흡수
            AddMaterial(new MaterialDef("Grass", 0.9f, 0.1f, 220f, 0f, 0.7f));
            AddMaterial(new MaterialDef("Wood", 0.6f, 0.0f, 280f, 0f, 0.4f));
            AddMaterial(new MaterialDef("Cloth", 0.8f, 0.1f, 200f, 0f, 0.6f));
            AddMaterial(new MaterialDef("Metal", 0.0f, 1.0f, 1200f, -50f, 0.0f));
            AddMaterial(new MaterialDef("Stone", 0.0f, 0.0f, 1600f, -80f, 0.1f));
            AddMaterial(new MaterialDef("Water", 0.0f, 0.3f, 9999f, 0f, 1.0f));

            // ① 요소가 재질을 바꾼다: 불이 마른 풀·나무·천에 붙는다.
            AddRule(new ReactionRule
            {
                Trigger = ElementTag.Fire,
                TargetMaterials = "Grass;Wood;Cloth",
                Effect = StateEffect.Ignite,
                Probability = 0.9f,
            });

            // ② 요소가 다른 요소를 바꾼다: 물이 불을 끈다.
            AddRule(new ReactionRule
            {
                Trigger = ElementTag.Water,
                TargetMaterials = "", // 모든 재질
                Effect = StateEffect.Extinguish,
                Probability = 1f,
            });
            AddRule(new ReactionRule
            {
                Trigger = ElementTag.Water,
                TargetMaterials = "Grass;Wood;Cloth",
                Effect = StateEffect.Wet,
                Probability = 1f,
            });

            // ③ 상황에 따라 요소가 힘을 만든다: 전기가 금속에서 감전을 일으킨다.
            AddRule(new ReactionRule
            {
                Trigger = ElementTag.Electric,
                TargetMaterials = "Metal;Water",
                Effect = StateEffect.Discharge,
                Probability = 0.8f,
            });

            // 얼음이 물·젖은 것을 얼린다.
            AddRule(new ReactionRule
            {
                Trigger = ElementTag.Ice,
                TargetMaterials = "Water",
                Effect = StateEffect.Freeze,
                Probability = 1f,
            });
        }
    }
}
