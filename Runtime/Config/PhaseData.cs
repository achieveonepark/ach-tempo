using System;
using Achieve.Tempo.Director;
using UnityEngine;

namespace Achieve.Tempo.Config
{
    /// <summary>
    /// 국면 전환 기준과 국면별 '쓸 수 있는 양(예산)'을 담은 데이터센터.
    /// 경계에서 국면이 깜빡이지 않도록 올리는 기준과 내리는 기준을 다르게 둔다.
    /// </summary>
    [Serializable]
    public class PhaseData
    {
        [Header("국면 전환 기준 (히스테리시스)")]
        [Tooltip("쌓기→터짐 으로 올라가는 기준.")]
        [Range(0f, 1f)] public float PeakEnter = 0.80f;
        [Tooltip("터짐→쉬기 로 내려가는 기준.")]
        [Range(0f, 1f)] public float RelaxEnter = 0.45f;
        [Tooltip("쉬기→쌓기 로 돌아가는 기준.")]
        [Range(0f, 1f)] public float BuildUpEnter = 0.35f;
        [Tooltip("쉬기를 최소한 보장하는 시간(초).")]
        public float MinRelax = 4f;

        [Header("국면별 예산")]
        public float BuildUpBudget = 2f;
        public float PeakBudget = 6f;
        public float RelaxBudget = 3f;

        /// <summary>국면에 맞는 예산을 돌려준다.</summary>
        public float BudgetFor(Phase phase)
        {
            switch (phase)
            {
                case Phase.BuildUp: return BuildUpBudget;
                case Phase.Peak: return PeakBudget;
                case Phase.Relax: return RelaxBudget;
                default: return 0f;
            }
        }
    }
}
