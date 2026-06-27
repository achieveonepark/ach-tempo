using Achieve.Tempo.Config;
using Achieve.Tempo.World;
using NUnit.Framework;
using UnityEngine;

namespace Achieve.Tempo.Tests
{
    public class WorldReactionTests
    {
        static WorldReaction MakeGrassField()
        {
            var world = new WorldReaction(width: 21, height: 21, cellSize: 1f,
                origin: Vector3.zero, ruleOverride: new RuleData(), seed: 777);
            world.Fill("Grass");
            return world;
        }

        static Vector3 CellCenter(int x, int y) => new Vector3(x + 0.5f, 0f, y + 0.5f);

        [Test]
        public void Fire_SpreadsDownwind()
        {
            var world = MakeGrassField();
            world.SetWind(new Vector2(1f, 0f), 2f); // 동쪽으로 강하게
            world.ApplyElement(CellCenter(10, 10), 0.5f, ElementTag.Fire, 2f);

            for (int i = 0; i < 40; i++) world.Tick(0.2f);

            int east = 0, west = 0;
            for (int y = 0; y < world.Height; y++)
            for (int x = 0; x < world.Width; x++)
            {
                if (!world.GetCell(x, y).OnFire) continue;
                if (x > 10) east++;
                else if (x < 10) west++;
            }

            Assert.Greater(east, west,
                $"바람(동쪽)을 타고 동쪽으로 더 번져야 한다. east={east}, west={west}");
        }

        [Test]
        public void Fire_ProducesActiveEvents()
        {
            var world = MakeGrassField();
            world.SetWind(new Vector2(1f, 0f), 1f);
            world.ApplyElement(CellCenter(10, 10), 0.5f, ElementTag.Fire, 2f);

            world.Tick(0.2f);
            Assert.Greater(world.ActiveEvents.Count, 0, "불이 붙으면 벌어지는 일이 생겨야 한다.");
            Assert.Greater(world.BurningCells, 0);
        }

        [Test]
        public void Water_Extinguishes_AndEventsClearAfterTtl()
        {
            var world = MakeGrassField();
            world.SetWind(new Vector2(1f, 0f), 1f);
            world.ApplyElement(CellCenter(10, 10), 1.5f, ElementTag.Fire, 2f);
            for (int i = 0; i < 5; i++) world.Tick(0.2f);
            Assert.Greater(world.BurningCells, 0, "먼저 불이 붙어 있어야 한다.");

            // 비로 넓게 끈다. (BurningCells 는 다음 Tick 에서 갱신된다.)
            world.ApplyElement(CellCenter(10, 10), 20f, ElementTag.Water, 1f);

            // TTL(기본 1.5초)이 지나면 벌어지는 일 목록이 비어야 한다.
            for (int i = 0; i < 20; i++) world.Tick(0.2f);
            Assert.AreEqual(0, world.BurningCells, "비를 맞은 뒤 다시 붙지 않아야 한다.");
            Assert.AreEqual(0, world.ActiveEvents.Count, "끝난 일은 치워져야 한다.");
        }

        [Test]
        public void ApplyElement_OutsideGrid_DoesNotThrow()
        {
            var world = MakeGrassField();
            Assert.DoesNotThrow(() =>
                world.ApplyElement(new Vector3(9999f, 0f, 9999f), 3f, ElementTag.Fire, 1f));
        }
    }
}
