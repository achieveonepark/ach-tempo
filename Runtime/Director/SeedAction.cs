using Achieve.Tempo.World;
using UnityEngine;

namespace Achieve.Tempo.Director
{
    /// <summary>
    /// 디렉터가 동작을 고를 때 보는 '의도'. 국면과 연결된다.
    /// </summary>
    public enum SeedIntent
    {
        /// <summary>쌓기: 잘 탈 조건만 깐다(아직 위험하지 않음).</summary>
        Prepare,
        /// <summary>터짐: 불씨를 던져 와르르 번지게 한다.</summary>
        Trigger,
        /// <summary>쉬기: 끄고 진정시킨다.</summary>
        Calm,
    }

    /// <summary>
    /// 조건 깔기 한 종류. 디렉터는 적을 직접 만들지 않고 이 동작으로 '조건만' 깐다.
    /// 갈아끼우는 자리이므로 인터페이스 대신 기본 동작을 깔아둔 클래스 + 가상 메서드로 둔다.
    /// </summary>
    public abstract class SeedAction
    {
        /// <summary>쓸 수 있는 양(예산)을 얼마나 먹는지.</summary>
        public abstract float Cost { get; }

        /// <summary>이 동작이 어느 국면에 어울리는지.</summary>
        public abstract SeedIntent Intent { get; }

        /// <summary>지금 깔아도 되나(전조를 보여줄 여지가 있나) 먼저 확인.</summary>
        public virtual bool CanSeed(WorldReaction world, Vector3 playerPos) => true;

        /// <summary>월드의 입구(ApplyElement/SetWind)만 부른다.</summary>
        public abstract void Seed(WorldReaction world, Vector3 playerPos);
    }
}
