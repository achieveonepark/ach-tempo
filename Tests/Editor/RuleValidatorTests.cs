using Achieve.Tempo.Config;
using Achieve.Tempo.Editor;
using Achieve.Tempo.World;
using NUnit.Framework;

namespace Achieve.Tempo.Tests.Editor
{
    public class RuleValidatorTests
    {
        [Test]
        public void DefaultRules_PassValidation()
        {
            var issues = RuleValidator.Validate(new RuleData());
            foreach (var i in issues)
                Assert.IsFalse(i.IsError, "기본 규칙표에 에러: " + i.Message);
        }

        [Test]
        public void MissingMaterial_IsError()
        {
            var data = new RuleData();
            data.AddRule(new ReactionRule
            {
                Trigger = ElementTag.Fire,
                TargetMaterials = "Unobtainium", // 레지스트리에 없음
                Effect = StateEffect.Ignite,
                Probability = 1f,
            });

            var issues = RuleValidator.Validate(data);
            Assert.IsTrue(issues.Exists(i => i.IsError && i.Message.Contains("Unobtainium")));
        }

        [Test]
        public void ProbabilityOutOfRange_IsError()
        {
            var data = new RuleData();
            data.AddRule(new ReactionRule
            {
                Trigger = ElementTag.Water,
                TargetMaterials = "Grass",
                Effect = StateEffect.Wet,
                Probability = 1.5f,
            });

            var issues = RuleValidator.Validate(data);
            Assert.IsTrue(issues.Exists(i => i.IsError && i.Message.Contains("확률")));
        }
    }
}
