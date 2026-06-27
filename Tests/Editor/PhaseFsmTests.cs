using Achieve.Tempo.Config;
using Achieve.Tempo.Director;
using NUnit.Framework;

namespace Achieve.Tempo.Tests.Editor
{
    public class PhaseFsmTests
    {
        static PhaseData Cfg() => new PhaseData
        {
            PeakEnter = 0.80f,
            RelaxEnter = 0.45f,
            BuildUpEnter = 0.35f,
            MinRelax = 4f,
        };

        [Test]
        public void StartsInBuildUp()
        {
            var fsm = new PhaseFsm(Cfg());
            Assert.AreEqual(Phase.BuildUp, fsm.Current);
        }

        [Test]
        public void BuildUp_To_Peak_OnHighTension()
        {
            var fsm = new PhaseFsm(Cfg());
            Assert.AreEqual(Phase.BuildUp, fsm.Evaluate(0.79f, 0.1f));
            Assert.AreEqual(Phase.Peak, fsm.Evaluate(0.80f, 0.1f));
        }

        [Test]
        public void Hysteresis_NoFlickerNearEnterThreshold()
        {
            var fsm = new PhaseFsm(Cfg());
            fsm.Evaluate(0.85f, 0.1f); // → Peak
            // 0.45~0.80 사이를 오가도 깜빡이지 않고 Peak 유지.
            Assert.AreEqual(Phase.Peak, fsm.Evaluate(0.50f, 0.1f));
            Assert.AreEqual(Phase.Peak, fsm.Evaluate(0.79f, 0.1f));
            Assert.AreEqual(Phase.Peak, fsm.Evaluate(0.46f, 0.1f));
        }

        [Test]
        public void Peak_To_Relax_ThenMinRelaxEnforced()
        {
            var fsm = new PhaseFsm(Cfg());
            fsm.Evaluate(0.85f, 0.1f);          // → Peak
            fsm.Evaluate(0.40f, 0.1f);          // → Relax (relaxTimer = 0)

            Assert.AreEqual(Phase.Relax, fsm.Current);

            // 최소 쉬는 시간(4초) 전에는 기준을 만족해도 BuildUp 으로 못 돌아간다.
            Assert.AreEqual(Phase.Relax, fsm.Evaluate(0.10f, 1f));
            Assert.AreEqual(Phase.Relax, fsm.Evaluate(0.10f, 2f));
            // 누적 4초 이상 + 기준(0.35) 이하 → BuildUp.
            Assert.AreEqual(Phase.BuildUp, fsm.Evaluate(0.10f, 2f));
        }
    }
}
