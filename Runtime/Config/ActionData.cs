using System;
using System.Collections.Generic;
using Achieve.Tempo.Director;
using UnityEngine;

namespace Achieve.Tempo.Config
{
    /// <summary>
    /// 디렉터가 고를 수 있는 동작 목록을 담은 데이터센터.
    /// 기본 생성자에 쌓기·터짐·쉬기 한 종류씩 들어 있어 바로 한 바퀴 돈다.
    /// 게임은 <see cref="DirectorBrain.ActionPool"/> 를 오버라이드하거나 이 목록을 갈아끼워 바꾼다.
    /// </summary>
    [Serializable]
    public class ActionData
    {
        readonly List<SeedAction> _actions;

        public IReadOnlyList<SeedAction> Actions => _actions;

        public ActionData()
        {
            _actions = new List<SeedAction>
            {
                new DryUpwind(new Vector2(1f, 0f)),  // 쌓기
                new IgniteUpwind(),                  // 터짐
                new ChargeStorm(),                   // 터짐
                new SummonRain(),                    // 쉬기
            };
        }

        public ActionData(IEnumerable<SeedAction> actions)
        {
            _actions = new List<SeedAction>(actions);
        }

        public void Add(SeedAction action) => _actions.Add(action);
    }
}
