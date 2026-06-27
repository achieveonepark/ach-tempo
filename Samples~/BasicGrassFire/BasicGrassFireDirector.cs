using Achieve.Tempo.Director;
using Achieve.Tempo.Tension;
using Achieve.Tempo.World;
using UnityEngine;

namespace Achieve.Tempo.Samples
{
    /// <summary>
    /// 가장 단순한 한 바퀴 흐름 예제. 풀밭 격자를 깔고, 디렉터가 긴장도를 보며
    /// 조건을 깔면 월드가 받아 불이 번진다. 빈 GameObject 에 붙여 Play 하면 된다.
    ///
    /// 위협 신호가 없으면 긴장도가 낮게 유지되므로, 인스펙터에서 _debugThreat 를
    /// 올려 디렉터가 쌓기→터짐으로 넘어가는 흐름을 눈으로 확인할 수 있다.
    /// </summary>
    public sealed class BasicGrassFireDirector : MonoBehaviour
    {
        [Header("격자")]
        [SerializeField] int _width = 64;
        [SerializeField] int _height = 64;
        [SerializeField] float _cellSize = 1f;

        [Header("바람")]
        [SerializeField] Vector2 _wind = new Vector2(1f, 0f);
        [SerializeField] float _windStrength = 1.2f;

        [Header("디버그: 가짜 위협 신호(0~1)")]
        [Range(0f, 1f)]
        [SerializeField] float _debugThreat = 0.6f;

        WorldReaction _world;
        DirectorBrain _director;
        DebugTensionModel _tension;

        // 물리 씬 없이 인스펙터 값으로 위협을 흉내내는 긴장도 모델.
        sealed class DebugTensionModel : TensionModel
        {
            public float Threat;
            protected override float SampleThreat(Vector3 p) => Threat;
        }

        void Start()
        {
            _world = new WorldReaction(_width, _height, _cellSize, transform.position);
            _world.Fill("Grass");
            _world.SetWind(_wind, _windStrength);

            _director = new DefaultDirectorBrain();
            _tension = new DebugTensionModel();
        }

        void Update()
        {
            float dt = Time.deltaTime;
            Vector3 player = transform.position;
            _tension.Threat = _debugThreat;

            // 항상 같은 순서: 긴장도 → 디렉터 → 월드.
            _tension.Tick(player, _world, dt);
            _director.Tick(_world, _tension, player, dt);
            _world.Tick(dt);
        }

        void OnGUI()
        {
            if (_world == null) return;
            GUI.Label(new Rect(10, 10, 400, 20), $"Tension: {_tension.Current:F2}");
            GUI.Label(new Rect(10, 30, 400, 20), $"Phase: {_director.CurrentPhase}");
            GUI.Label(new Rect(10, 50, 400, 20), $"Active events: {_world.ActiveEvents.Count}");
            GUI.Label(new Rect(10, 70, 400, 20), $"Burning cells: {_world.BurningCells}");
        }

        void OnDrawGizmos()
        {
            if (_world == null) return;
            // 불타는 칸을 빨간 점으로, 벌어지는 일을 노란 구로 그린다.
            Gizmos.color = Color.red;
            for (int y = 0; y < _world.Height; y++)
            for (int x = 0; x < _world.Width; x++)
            {
                if (!_world.GetCell(x, y).OnFire) continue;
                Vector3 p = transform.position + new Vector3((x + 0.5f) * _cellSize, 0f, (y + 0.5f) * _cellSize);
                Gizmos.DrawCube(p, Vector3.one * _cellSize * 0.6f);
            }

            Gizmos.color = new Color(1f, 0.9f, 0.2f, 0.4f);
            foreach (ReactionHandle h in _world.ActiveEvents)
                Gizmos.DrawWireSphere(h.Position, _cellSize * h.Magnitude);
        }
    }
}
