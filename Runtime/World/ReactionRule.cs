using System;
using UnityEngine;

namespace Achieve.Tempo.World
{
    /// <summary>
    /// 반응 규칙 한 줄(표의 한 행). "이런 요소가 / 이런 재질에 닿고 / 기준을 넘으면 → 이렇게 바뀐다".
    /// 코드가 아니라 데이터로 두어 기획자가 코드 없이 만진다. (<see cref="RuleData"/> 가 표 전체를 들고 있다.)
    /// </summary>
    [Serializable]
    public sealed class ReactionRule
    {
        [Tooltip("어떤 요소가 닿았을 때 (불/물/얼음/전기/바람)")]
        public ElementTag Trigger = ElementTag.Fire;

        [Tooltip("대상 재질들. 배열 UI 말고 한 칸에 구분자로: \"Wood;Grass;Cloth\". 비우면 모든 재질.")]
        public string TargetMaterials = "";

        [Tooltip("어떻게 바뀌는지")]
        public StateEffect Effect = StateEffect.Ignite;

        [Tooltip("매번 일어날 확률 0~1")]
        [Range(0f, 1f)] public float Probability = 1f;

        [Tooltip("발동 기준값(온도/젖음/전기 등 Effect 에 따라 해석). 예: Ignite 면 점화 온도 보정.")]
        public float Threshold = 0f;

        // ; 로 적은 대상 재질을 한 번만 쪼개 캐시해 둔다.
        [NonSerialized] string[] _targetCache;
        [NonSerialized] string _targetCacheSource;

        public const char Separator = ';';

        /// <summary>
        /// "Wood;Grass;Cloth" 같은 한 칸 문자열을 재질 이름 배열로 쪼갠다.
        /// 빈 칸/공백은 버리고, 같은 입력이면 캐시를 그대로 돌려준다.
        /// </summary>
        public string[] TargetMaterialList
        {
            get
            {
                if (_targetCache != null && _targetCacheSource == TargetMaterials)
                    return _targetCache;

                _targetCacheSource = TargetMaterials;
                _targetCache = Split(TargetMaterials);
                return _targetCache;
            }
        }

        /// <summary>대상 재질이 비어 있으면(모든 재질 대상) true.</summary>
        public bool MatchesAnyMaterial => TargetMaterialList.Length == 0;

        public bool MatchesMaterial(string materialName)
        {
            if (MatchesAnyMaterial) return true;
            string[] list = TargetMaterialList;
            for (int i = 0; i < list.Length; i++)
            {
                if (string.Equals(list[i], materialName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        /// <summary>구분자로 적은 한 칸을 배열로. 공백·빈 항목은 버린다. (테스트가 직접 부른다.)</summary>
        public static string[] Split(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return Array.Empty<string>();

            string[] parts = raw.Split(Separator);
            int count = 0;
            for (int i = 0; i < parts.Length; i++)
            {
                string trimmed = parts[i].Trim();
                if (trimmed.Length > 0) parts[count++] = trimmed;
            }

            if (count == parts.Length) return parts;
            var result = new string[count];
            Array.Copy(parts, result, count);
            return result;
        }
    }
}
