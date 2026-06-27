using UnityEngine;

namespace Achieve.Tempo.World
{
    /// <summary>
    /// '지금 벌어지고 있는 일' 한 건. (불 한 덩이, 감전 한 번 등)
    /// 디렉터와 긴장도 계산기가 <see cref="WorldReaction.ActiveEvents"/> 로 이 목록을 읽는다.
    /// </summary>
    public struct ReactionEvent
    {
        public Vector3 Position;
        public float Magnitude; // 크기 (긴장도로 환산할 때 씀)
        public float Ttl;       // 남은 시간(초). 0 이하가 되면 치운다.
        public ElementTag Kind;

        public bool IsAlive => Ttl > 0f;
    }
}
