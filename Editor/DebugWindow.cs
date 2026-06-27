using System.Collections.Generic;
using Achieve.Tempo.Director;
using Achieve.Tempo.Tension;
using Achieve.Tempo.World;
using UnityEditor;
using UnityEngine;

namespace Achieve.Tempo.Editor
{
    /// <summary>
    /// 긴장도·국면을 실시간으로 보는 디버그 창. 플레이 중인 디렉터/긴장도를 등록하면
    /// 긴장도 그래프와 지금 국면, 벌어지는 일 개수를 보여준다.
    ///
    /// 게임 코드에서 <see cref="Bind"/> 로 자기 인스턴스를 연결한다.
    /// </summary>
    public sealed class DebugWindow : EditorWindow
    {
        static TensionModel _tension;
        static DirectorBrain _director;
        static WorldReaction _world;

        readonly Queue<float> _history = new Queue<float>();
        const int HistoryLen = 240;

        /// <summary>게임 코드가 플레이 시작 시 호출해 디버그 창에 연결한다.</summary>
        public static void Bind(TensionModel tension, DirectorBrain director, WorldReaction world)
        {
            _tension = tension;
            _director = director;
            _world = world;
        }

        [MenuItem("Tools/Tempo/Debug Window")]
        public static void Open() => GetWindow<DebugWindow>("Tempo Debug");

        void OnEnable()
        {
            EditorApplication.update += Repaint;
        }

        void OnDisable()
        {
            EditorApplication.update -= Repaint;
        }

        void Update()
        {
            if (!Application.isPlaying || _tension == null) return;
            _history.Enqueue(_tension.Current);
            while (_history.Count > HistoryLen) _history.Dequeue();
        }

        void OnGUI()
        {
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("플레이 모드에서, 게임이 DebugWindow.Bind(...) 를 호출하면 보입니다.", MessageType.Info);
                return;
            }
            if (_tension == null)
            {
                EditorGUILayout.HelpBox("아직 연결되지 않았습니다. DebugWindow.Bind(tension, director, world) 를 호출하세요.", MessageType.Warning);
                return;
            }

            EditorGUILayout.LabelField("긴장도 (Tension)", $"{_tension.Current:F3}");
            if (_director != null)
                EditorGUILayout.LabelField("국면 (Phase)", _director.CurrentPhase.ToString());
            if (_world != null)
            {
                EditorGUILayout.LabelField("벌어지는 일 (Active Events)", _world.ActiveEvents.Count.ToString());
                EditorGUILayout.LabelField("불타는 칸 (Burning Cells)", _world.BurningCells.ToString());
            }

            DrawGraph();
        }

        void DrawGraph()
        {
            Rect rect = GUILayoutUtility.GetRect(100, 120, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, new Color(0.1f, 0.1f, 0.12f));

            if (_history.Count < 2) return;

            float[] vals = _history.ToArray();
            Handles.color = new Color(0.3f, 0.8f, 1f);
            Vector3 prev = default;
            for (int i = 0; i < vals.Length; i++)
            {
                float x = rect.x + rect.width * i / (vals.Length - 1);
                float y = rect.yMax - rect.height * Mathf.Clamp01(vals[i]);
                Vector3 p = new Vector3(x, y, 0f);
                if (i > 0) Handles.DrawLine(prev, p);
                prev = p;
            }

            // 0.8(터짐) / 0.45(쉬기) 기준선.
            DrawGuide(rect, 0.80f, new Color(1f, 0.4f, 0.3f, 0.6f));
            DrawGuide(rect, 0.45f, new Color(0.5f, 0.9f, 0.5f, 0.6f));
        }

        static void DrawGuide(Rect rect, float level, Color color)
        {
            float y = rect.yMax - rect.height * level;
            EditorGUI.DrawRect(new Rect(rect.x, y, rect.width, 1f), color);
        }
    }
}
