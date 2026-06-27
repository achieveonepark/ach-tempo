using System;
using UnityEngine;

namespace Achieve.Tempo.Config
{
    /// <summary>
    /// 긴장도 계산기가 보는 데이터센터(세기·반경·가중치·속도·기준).
    /// 기본값은 baseline 이며, 실제 수치는 게임에서 맞춘다(문서 11장 '아직 안 정한 것').
    /// </summary>
    [Serializable]
    public class TensionConfig
    {
        [Header("플레이어 위협 → 스트레스")]
        [Tooltip("피격 시 받은 피해 비율에 곱하는 즉발 상승량.")]
        public float DamageGain = 1.5f;
        [Tooltip("주변 위협을 매초 누적시키는 속도.")]
        public float ThreatGain = 0.6f;
        [Tooltip("스트레스가 시간이 지나며 빠지는 속도(/초).")]
        public float StressDecay = 0.25f;

        [Header("위협 샘플링")]
        public float ThreatRadius = 12f;
        [Tooltip("이 합 이상이면 위협 신호를 1로 본다.")]
        public float ThreatSaturation = 4f;
        [Tooltip("위협으로 칠 레이어 마스크.")]
        public LayerMask ThreatMask = ~0;

        [Header("주변에서 벌어지는 일 샘플링")]
        public float AroundRadius = 14f;
        [Tooltip("이 합 이상이면 주변 신호를 1로 본다.")]
        public float AroundSaturation = 5f;

        [Header("두 신호 합치기 가중")]
        [Range(0f, 2f)] public float StressWeight = 1f;
        [Range(0f, 2f)] public float AroundWeight = 1f;

        [Header("부드럽게 바꾸기(시간 상수, 초)")]
        [Tooltip("올라갈 때: 작게 둘수록 빠르게 반응(위험은 바로 느낀다).")]
        public float RiseSpeed = 0.25f;
        [Tooltip("내려갈 때: 크게 둘수록 천천히 안심(서서히 빠진다).")]
        public float FallSpeed = 2.5f;
    }
}
