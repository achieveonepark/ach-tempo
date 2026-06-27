namespace Achieve.Tempo.World
{
    /// <summary>
    /// 규칙이 발동했을 때 칸 상태를 어떻게 바꾸는지.
    /// 야숨식 세 가지 기본 규칙(요소→재질 / 요소→요소 / 요소→힘)을 표현하는 데 쓴다.
    /// </summary>
    public enum StateEffect
    {
        None = 0,
        Ignite,      // 불붙음
        Extinguish,  // 불 끔
        Wet,         // 젖음 늘림
        Dry,         // 젖음 줄임
        Freeze,      // 얼림
        Thaw,        // 녹임
        Charge,      // 전기 채움
        Discharge,   // 전기 방전 (감전 이벤트)
    }
}
