using System.Collections.Generic;
using Achieve.Tempo.Config;
using Achieve.Tempo.Tension;
using Achieve.Tempo.World;
using UnityEngine;

namespace Achieve.Tempo.Director
{
    /// <summary>
    /// 디렉터(위에서 조건을 까는 쪽). 긴장도 숫자 하나를 보고 국면을 정하고,
    /// 쓸 수 있는 양(예산) 안에서 동작을 골라 월드에 '조건만' 깐다. 적을 직접 만들지 않는다.
    ///
    /// 게임은 이 클래스를 상속해 <see cref="ActionPool"/> 만 바꾸거나,
    /// <see cref="PickActions"/> 를 오버라이드해 세밀한 정책을 넣는다. (인터페이스 대신 virtual)
    /// </summary>
    public abstract class DirectorBrain
    {
        readonly PhaseFsm _fsm = new PhaseFsm();

        /// <summary>지금 국면(디버그 창·전조 연출에서 읽는다).</summary>
        public Phase CurrentPhase => _fsm.Current;

        /// <summary>안전장치: 동시에 벌어지는 일이 이 수를 넘으면 새 불 붙이기를 잠시 막는다.</summary>
        protected virtual int MaxActiveEvents => 48;

        // 게임마다 동작 목록만 바꿔치기 (인터페이스 말고 virtual).
        protected virtual IReadOnlyList<SeedAction> ActionPool
            => DataService<ActionData>.GetDataCenter().Actions;

        public void Tick(WorldReaction world, TensionModel tension, Vector3 playerPos, float dt)
        {
            Phase phase = _fsm.Evaluate(tension.Current, dt);
            float budget = DataService<PhaseData>.GetDataCenter().BudgetFor(phase);

            foreach (SeedAction action in PickActions(phase, budget, world, playerPos))
                action.Seed(world, playerPos); // ★ 적을 직접 만드는 게 아니라 조건만 깖
        }

        /// <summary>
        /// 기본 정책: 국면에 맞는 의도의 동작을, 쓸 수 있는 양 안에서, 깔아도 되는 것만 고른다.
        /// - 쌓기: Prepare / 터짐: Trigger / 쉬기: Calm
        /// - 쉬기 국면에선 끄기를 우선한다(안전장치, 문서 7장).
        /// - 너무 커지지 않게 동시 이벤트 상한을 넘으면 새 불(Trigger)은 건너뛴다.
        /// </summary>
        protected virtual IEnumerable<SeedAction> PickActions(
            Phase phase, float budget, WorldReaction world, Vector3 playerPos)
        {
            SeedIntent want = IntentFor(phase);
            bool overloaded = world.ActiveEvents.Count >= MaxActiveEvents;

            float remaining = budget;
            IReadOnlyList<SeedAction> pool = ActionPool;

            for (int i = 0; i < pool.Count; i++)
            {
                SeedAction action = pool[i];
                if (action.Intent != want) continue;

                // 상한을 넘었으면 새 불을 붙이는 Trigger 는 막는다.
                if (overloaded && action.Intent == SeedIntent.Trigger) continue;

                if (action.Cost > remaining) continue;
                if (!action.CanSeed(world, playerPos)) continue;

                remaining -= action.Cost;
                yield return action;
            }
        }

        protected static SeedIntent IntentFor(Phase phase)
        {
            switch (phase)
            {
                case Phase.Peak: return SeedIntent.Trigger;
                case Phase.Relax: return SeedIntent.Calm;
                default: return SeedIntent.Prepare;
            }
        }
    }

    /// <summary>오버라이드 없이 기본 정책만 쓰는 디렉터. 예제·빠른 시작용.</summary>
    public sealed class DefaultDirectorBrain : DirectorBrain
    {
    }
}
