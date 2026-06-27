namespace Achieve.Tempo.World
{
    /// <summary>
    /// 칸(또는 물체) 하나의 '지금 상태'. 격자에서 많이 처리하니 가벼운 struct 로 둔다.
    /// 변하지 않는 성질(잘 타는지 등)은 여기 적지 않고 <see cref="MaterialDef"/> 에서 찾는다.
    /// </summary>
    public struct ElementalState
    {
        /// <summary>재질 레지스트리의 인덱스. -1 이면 빈 칸.</summary>
        public int MaterialId;

        public float Temperature; // 온도
        public float Wetness;     // 젖은 정도 0~1
        public float Charge;      // 전기 충전 0~1
        public bool OnFire;
        public bool Frozen;

        public static ElementalState Empty => new ElementalState
        {
            MaterialId = -1,
            Temperature = 20f,
            Wetness = 0f,
            Charge = 0f,
            OnFire = false,
            Frozen = false,
        };

        public bool IsEmpty => MaterialId < 0;
    }
}
