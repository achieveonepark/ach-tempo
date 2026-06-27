namespace Achieve.Tempo.World
{
    /// <summary>
    /// 월드에 닿을 수 있는 요소 종류. 야숨식 '불·물·얼음·전기·바람'.
    /// 규칙표(<see cref="ReactionRule"/>)의 Trigger 와 결과(<see cref="ReactionEvent"/>)의 Kind 로 쓰인다.
    /// </summary>
    public enum ElementTag
    {
        None = 0,
        Fire,
        Water,
        Ice,
        Electric,
        Wind,
    }
}
