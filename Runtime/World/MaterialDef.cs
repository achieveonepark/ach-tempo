using System;
using UnityEngine;

namespace Achieve.Tempo.World
{
    /// <summary>
    /// 재질 정의(잘 타는지·전기 통하는지·언제 녹는지 같은 '변하지 않는 성질').
    /// 한 칸의 그때그때 상태는 <see cref="ElementalState"/> 에 두고, 고정 성질은 여기서 찾아본다.
    /// 같은 이름의 재질은 <see cref="RuleData"/> 의 레지스트리에 한 번만 등록해두고 공유한다.
    /// 이름은 UnityEngine.Material 과 헷갈리지 않도록 MaterialDef 로 둔다.
    /// </summary>
    [Serializable]
    public sealed class MaterialDef
    {
        [SerializeField] string _name = "Unknown";

        [Tooltip("잘 타는 정도 0~1. 0 이면 안 탄다.")]
        [Range(0f, 1f)] public float Flammability = 0f;

        [Tooltip("전기가 통하는 정도 0~1. 금속이면 1 에 가깝다.")]
        [Range(0f, 1f)] public float Conductivity = 0f;

        [Tooltip("이 온도 이상이면 불이 붙을 수 있다.")]
        public float IgnitionPoint = 250f;

        [Tooltip("이 온도 이하로 식으면 얼 수 있다.")]
        public float FreezePoint = 0f;

        [Tooltip("물을 머금는 정도 0~1. 천·풀은 잘 젖고 금속은 안 젖는다.")]
        [Range(0f, 1f)] public float WaterAbsorption = 0.5f;

        public string Name => _name;

        public MaterialDef() { }

        public MaterialDef(string name, float flammability, float conductivity,
            float ignitionPoint = 250f, float freezePoint = 0f, float waterAbsorption = 0.5f)
        {
            _name = name;
            Flammability = flammability;
            Conductivity = conductivity;
            IgnitionPoint = ignitionPoint;
            FreezePoint = freezePoint;
            WaterAbsorption = waterAbsorption;
        }

        public bool CanBurn => Flammability > 0f;
        public bool CanConduct => Conductivity > 0f;

        /// <summary>아무 성질도 없는 '빈' 재질. 격자의 기본값으로 쓴다.</summary>
        public static readonly MaterialDef Empty = new MaterialDef("Empty", 0f, 0f, float.MaxValue, float.MinValue, 0f);
    }
}
