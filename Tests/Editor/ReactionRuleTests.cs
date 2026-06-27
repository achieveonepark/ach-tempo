using Achieve.Tempo.World;
using NUnit.Framework;

namespace Achieve.Tempo.Tests.Editor
{
    public class ReactionRuleTests
    {
        [Test]
        public void Split_TrimsAndDropsEmpty()
        {
            string[] parts = ReactionRule.Split(" Wood; Grass ;;Cloth; ");
            CollectionAssert.AreEqual(new[] { "Wood", "Grass", "Cloth" }, parts);
        }

        [Test]
        public void Split_EmptyOrWhitespace_ReturnsEmpty()
        {
            Assert.AreEqual(0, ReactionRule.Split("").Length);
            Assert.AreEqual(0, ReactionRule.Split("   ").Length);
            Assert.AreEqual(0, ReactionRule.Split(null).Length);
        }

        [Test]
        public void TargetMaterialList_IsCachedUntilSourceChanges()
        {
            var rule = new ReactionRule { TargetMaterials = "Wood;Grass" };
            string[] first = rule.TargetMaterialList;
            string[] second = rule.TargetMaterialList;
            Assert.AreSame(first, second, "같은 입력이면 캐시를 재사용해야 한다.");

            rule.TargetMaterials = "Cloth";
            string[] third = rule.TargetMaterialList;
            Assert.AreNotSame(first, third, "입력이 바뀌면 다시 쪼개야 한다.");
            CollectionAssert.AreEqual(new[] { "Cloth" }, third);
        }

        [Test]
        public void MatchesMaterial_IsCaseInsensitive()
        {
            var rule = new ReactionRule { TargetMaterials = "Wood;Grass" };
            Assert.IsTrue(rule.MatchesMaterial("grass"));
            Assert.IsFalse(rule.MatchesMaterial("Metal"));
        }

        [Test]
        public void EmptyTarget_MatchesAnyMaterial()
        {
            var rule = new ReactionRule { TargetMaterials = "" };
            Assert.IsTrue(rule.MatchesAnyMaterial);
            Assert.IsTrue(rule.MatchesMaterial("Anything"));
        }
    }
}
