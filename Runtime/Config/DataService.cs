namespace Achieve.Tempo.Config
{
    /// <summary>
    /// 바깥에서 조정하는 값(규칙표·기준값·가중치)을 코드와 분리해 한 곳에서 들고 있는 작은 서비스.
    /// 기획자는 데이터센터(<typeparamref name="T"/>)만 만지고, 코드는 <see cref="GetDataCenter"/> 로 읽기만 한다.
    ///
    /// 기본값은 각 데이터센터의 기본 생성자에 들어 있어 별도 설정 없이도 동작한다.
    /// 게임이 자기 값을 쓰려면 시작할 때 <see cref="Register"/> 로 갈아끼운다.
    /// (Addressables 로 불러온 ScriptableObject 를 등록하는 자리이기도 하다 — UnityWebRequest 는 안 쓴다.)
    /// </summary>
    public static class DataService<T> where T : class, new()
    {
        static T _center;

        public static T GetDataCenter() => _center ??= new T();

        public static void Register(T center) => _center = center;

        /// <summary>테스트에서 기본값으로 되돌릴 때.</summary>
        public static void Reset() => _center = null;
    }
}
