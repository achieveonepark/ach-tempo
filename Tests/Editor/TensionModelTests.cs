using Achieve.Tempo.Config;
using Achieve.Tempo.Tension;
using Achieve.Tempo.World;
using NUnit.Framework;
using UnityEngine;

namespace Achieve.Tempo.Tests.Editor
{
    /// <summary>
    /// 물리 씬 없이 신호를 주입하려고 두 Sample 메서드를 갈아끼운 테스트용 모델.
    /// </summary>
    sealed class StubTensionModel : TensionModel
    {
        public float ThreatSignal;
        public float AroundSignal;

        public StubTensionModel(TensionConfig cfg) : base(cfg) { }

        protected override float SampleThreat(Vector3 p) => ThreatSignal;
        protected override float SampleAround(Vector3 p, WorldReaction world) => AroundSignal;
    }

    public class TensionModelTests
    {
        static TensionConfig Cfg() => new TensionConfig
        {
            DamageGain = 1f,
            ThreatGain = 1f,
            StressDecay = 0.1f,
            StressWeight = 1f,
            AroundWeight = 1f,
            RiseSpeed = 0.2f,
            FallSpeed = 2.5f,
        };

        [Test]
        public void Deterministic_SameInputsSameGraph()
        {
            var a = new StubTensionModel(Cfg()) { ThreatSignal = 0.5f, AroundSignal = 0.3f };
            var b = new StubTensionModel(Cfg()) { ThreatSignal = 0.5f, AroundSignal = 0.3f };

            for (int i = 0; i < 100; i++)
            {
                a.Tick(Vector3.zero, null, 0.016f);
                b.Tick(Vector3.zero, null, 0.016f);
                Assert.AreEqual(a.Current, b.Current, 1e-6f);
            }
        }

        [Test]
        public void StaysWithinUnitRange()
        {
            var m = new StubTensionModel(Cfg()) { ThreatSignal = 1f, AroundSignal = 1f };
            for (int i = 0; i < 500; i++)
            {
                m.Tick(Vector3.zero, null, 0.05f);
                Assert.GreaterOrEqual(m.Current, 0f);
                Assert.LessOrEqual(m.Current, 1f);
            }
        }

        [Test]
        public void RisesFasterThanItFalls()
        {
            // 같은 크기의 변화라도 올라갈 때가 내려갈 때보다 빠르게 움직여야 한다.
            var rising = new StubTensionModel(Cfg()) { ThreatSignal = 1f, AroundSignal = 1f };
            rising.Tick(Vector3.zero, null, 0.1f);
            float afterOneRise = rising.Current;

            var falling = new StubTensionModel(Cfg()) { ThreatSignal = 1f, AroundSignal = 1f };
            for (int i = 0; i < 200; i++) falling.Tick(Vector3.zero, null, 0.1f); // 거의 1까지 채움
            float high = falling.Current;
            falling.ThreatSignal = 0f;
            falling.AroundSignal = 0f;
            falling.Tick(Vector3.zero, null, 0.1f);
            float fallDelta = high - falling.Current;

            Assert.Greater(afterOneRise, fallDelta,
                "올라갈 땐 빠르게, 내려갈 땐 천천히여야 한다.");
        }

        [Test]
        public void Combine_EitherSignalRaisesTension()
        {
            var onlyThreat = new StubTensionModel(Cfg()) { ThreatSignal = 0.8f, AroundSignal = 0f };
            var onlyAround = new StubTensionModel(Cfg()) { ThreatSignal = 0f, AroundSignal = 0.8f };
            for (int i = 0; i < 50; i++)
            {
                onlyThreat.Tick(Vector3.zero, null, 0.05f);
                onlyAround.Tick(Vector3.zero, null, 0.05f);
            }
            Assert.Greater(onlyThreat.Current, 0.1f);
            Assert.Greater(onlyAround.Current, 0.1f);
        }

        [Test]
        public void OnPlayerDamaged_SpikesImmediately()
        {
            var m = new StubTensionModel(Cfg()) { ThreatSignal = 0f, AroundSignal = 0f };
            float before = m.PlayerStress;
            m.OnPlayerDamaged(50f, 100f); // 절반 피해
            Assert.Greater(m.PlayerStress, before);
        }
    }
}
