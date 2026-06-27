using Achieve.Tempo.Config;
using UnityEngine;

namespace Achieve.Tempo.Director
{
    /// <summary>
    /// 국면 전환기. 기준이 하나면 경계에서 국면이 왔다 갔다 깜빡이므로,
    /// 올리는 기준과 내리는 기준을 다르게(히스테리시스) 두고 최소 쉬는 시간을 보장한다.
    /// 순수 C# 이라 EditMode 테스트에서 그대로 쓴다.
    /// </summary>
    public sealed class PhaseFsm
    {
        Phase _phase = Phase.BuildUp;
        float _relaxTimer;

        PhaseData _cfgOverride;
        PhaseData Cfg => _cfgOverride ?? DataService<PhaseData>.GetDataCenter();

        public Phase Current => _phase;

        public PhaseFsm(PhaseData cfgOverride = null)
        {
            _cfgOverride = cfgOverride;
        }

        public Phase Evaluate(float tension, float dt)
        {
            PhaseData cfg = Cfg;
            _relaxTimer += dt;

            switch (_phase)
            {
                case Phase.BuildUp:
                    if (tension >= cfg.PeakEnter)
                        _phase = Phase.Peak;
                    break;

                case Phase.Peak:
                    if (tension <= cfg.RelaxEnter)
                    {
                        _phase = Phase.Relax;
                        _relaxTimer = 0f;
                    }
                    break;

                case Phase.Relax:
                    if (tension <= cfg.BuildUpEnter && _relaxTimer >= cfg.MinRelax)
                        _phase = Phase.BuildUp;
                    break;
            }

            return _phase;
        }

        public void Reset()
        {
            _phase = Phase.BuildUp;
            _relaxTimer = 0f;
        }
    }
}
