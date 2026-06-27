namespace Achieve.Tempo.Director
{
    /// <summary>
    /// 디렉터의 국면. 쌓기 → 터짐 → 쉬기 를 돈다.
    /// </summary>
    public enum Phase
    {
        /// <summary>잘 탈 조건만 차곡차곡 모은다(아직 불은 안 붙임).</summary>
        BuildUp,
        /// <summary>쌓인 데다 불씨를 던져 와르르 번지게 한다.</summary>
        Peak,
        /// <summary>비로 끄고 한숨 돌리며 예산을 회복한다.</summary>
        Relax,
    }
}
