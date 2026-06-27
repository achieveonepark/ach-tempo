using System.Collections.Generic;
using System.Text;
using Achieve.Tempo.Config;
using Achieve.Tempo.World;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Achieve.Tempo.Editor
{
    /// <summary>
    /// 규칙표 자동 검사. 빌드 전에(그리고 메뉴에서 수동으로) 규칙표의
    /// 없는 재질 / 서로 어긋나는 규칙을 잡아준다. (문서 10장 자동화)
    /// </summary>
    public static class RuleValidator
    {
        public struct Issue
        {
            public bool IsError;
            public string Message;
        }

        [MenuItem("Tools/Tempo/Validate Rules")]
        public static void ValidateMenu()
        {
            var issues = Validate(DataService<RuleData>.GetDataCenter());
            if (issues.Count == 0)
            {
                Debug.Log("[Tempo] 규칙표 검사 통과 — 문제 없음.");
                return;
            }

            var sb = new StringBuilder();
            int errors = 0;
            foreach (Issue issue in issues)
            {
                if (issue.IsError) errors++;
                sb.AppendLine((issue.IsError ? "ERROR: " : "WARN:  ") + issue.Message);
            }

            if (errors > 0) Debug.LogError("[Tempo] 규칙표 검사 실패\n" + sb);
            else Debug.LogWarning("[Tempo] 규칙표 경고\n" + sb);
        }

        /// <summary>규칙표를 검사해 문제 목록을 돌려준다. (테스트에서도 그대로 부른다.)</summary>
        public static List<Issue> Validate(RuleData data)
        {
            var issues = new List<Issue>();
            if (data == null)
            {
                issues.Add(new Issue { IsError = true, Message = "RuleData 가 null 이다." });
                return issues;
            }

            // 알려진 재질 이름 모음.
            var known = new HashSet<string>();
            foreach (Material m in data.Materials) known.Add(m.Name);

            // 같은 (Trigger, 재질) 에 서로 어긋나는 효과가 동시에 있는지 본다.
            var seen = new Dictionary<string, StateEffect>();

            IReadOnlyList<ReactionRule> rules = data.Rules;
            for (int i = 0; i < rules.Count; i++)
            {
                ReactionRule rule = rules[i];

                if (rule.Probability < 0f || rule.Probability > 1f)
                {
                    issues.Add(new Issue
                    {
                        IsError = true,
                        Message = $"규칙 #{i} ({rule.Trigger}): 확률이 0~1 밖이다 ({rule.Probability}).",
                    });
                }

                // 없는 재질을 가리키는지.
                foreach (string target in rule.TargetMaterialList)
                {
                    if (!known.Contains(target))
                    {
                        issues.Add(new Issue
                        {
                            IsError = true,
                            Message = $"규칙 #{i} ({rule.Trigger}): 레지스트리에 없는 재질 '{target}'.",
                        });
                    }
                }

                // 서로 어긋나는 규칙(예: 같은 트리거+재질이 Ignite 와 Extinguish 를 동시에).
                string[] targets = rule.MatchesAnyMaterial ? new[] { "*" } : rule.TargetMaterialList;
                foreach (string target in targets)
                {
                    string key = rule.Trigger + ":" + target;
                    if (seen.TryGetValue(key, out StateEffect prev) && Conflicts(prev, rule.Effect))
                    {
                        issues.Add(new Issue
                        {
                            IsError = false,
                            Message = $"규칙 #{i}: ({rule.Trigger}, {target}) 에 어긋나는 효과 {prev} ↔ {rule.Effect}.",
                        });
                    }
                    else
                    {
                        seen[key] = rule.Effect;
                    }
                }
            }

            return issues;
        }

        static bool Conflicts(StateEffect a, StateEffect b)
        {
            return (a == StateEffect.Ignite && b == StateEffect.Extinguish)
                || (a == StateEffect.Extinguish && b == StateEffect.Ignite)
                || (a == StateEffect.Wet && b == StateEffect.Dry)
                || (a == StateEffect.Dry && b == StateEffect.Wet)
                || (a == StateEffect.Freeze && b == StateEffect.Thaw)
                || (a == StateEffect.Thaw && b == StateEffect.Freeze);
        }

        /// <summary>빌드 전에 자동으로 검사해 에러가 있으면 빌드를 멈춘다.</summary>
        public sealed class BuildCheck : IPreprocessBuildWithReport
        {
            public int callbackOrder => 0;

            public void OnPreprocessBuild(BuildReport report)
            {
                var issues = Validate(DataService<RuleData>.GetDataCenter());
                foreach (Issue issue in issues)
                {
                    if (issue.IsError)
                        throw new BuildFailedException("[Tempo] 규칙표 검사 실패: " + issue.Message);
                }
            }
        }
    }
}
