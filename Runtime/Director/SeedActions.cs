using Achieve.Tempo.World;
using UnityEngine;

namespace Achieve.Tempo.Director
{
    /// <summary>
    /// 쌓기 동작: 바람을 세우고 말려, 곧 불이 잘 번질 조건만 만든다(아직 불은 안 붙임).
    /// 전조: 바람이 말라가는 소리/흔들림으로 보여주면 된다(문서 7장).
    /// </summary>
    public sealed class DryUpwind : SeedAction
    {
        readonly Vector2 _windDir;
        readonly float _windStrength;
        readonly float _radius;

        public DryUpwind(Vector2 windDir, float windStrength = 1.2f, float radius = 6f)
        {
            _windDir = windDir;
            _windStrength = windStrength;
            _radius = radius;
        }

        public override float Cost => 1f;
        public override SeedIntent Intent => SeedIntent.Prepare;

        public override void Seed(WorldReaction world, Vector3 playerPos)
        {
            // 바람을 세워 번질 방향을 정하고, 바람 위쪽을 살짝 데워 둔다(점화 전).
            world.SetWind(_windDir, _windStrength);
            Vector3 upwind = playerPos - new Vector3(_windDir.x, 0f, _windDir.y).normalized * _radius;
            world.ApplyElement(upwind, _radius, ElementTag.Wind, 0.5f);
        }
    }

    /// <summary>
    /// 터짐 동작: 바람 위쪽 마른 풀에 불씨를 던진다. 바람을 타고 플레이어 쪽으로 번진다.
    /// </summary>
    public sealed class IgniteUpwind : SeedAction
    {
        readonly float _distance;
        readonly float _radius;
        readonly float _power;

        public IgniteUpwind(float distance = 8f, float radius = 2f, float power = 1f)
        {
            _distance = distance;
            _radius = radius;
            _power = power;
        }

        public override float Cost => 3f;
        public override SeedIntent Intent => SeedIntent.Trigger;

        public override bool CanSeed(WorldReaction world, Vector3 playerPos)
            => world.WindStrength > 0f; // 바람이 서 있어야 번진다.

        public override void Seed(WorldReaction world, Vector3 playerPos)
        {
            Vector3 wind = new Vector3(world.Wind.x, 0f, world.Wind.y).normalized;
            Vector3 spot = playerPos - wind * _distance; // 바람 위쪽
            world.ApplyElement(spot, _radius, ElementTag.Fire, _power);
        }
    }

    /// <summary>
    /// 터짐 동작: 금속 무장한 적/물웅덩이 위에 번개구름을 띄워 감전을 일으킨다.
    /// 전조: 먹구름이 몰려오는 연출이 따라붙어야 한다(문서 7장).
    /// </summary>
    public sealed class ChargeStorm : SeedAction
    {
        readonly float _radius;
        readonly float _power;

        public ChargeStorm(float radius = 5f, float power = 1f)
        {
            _radius = radius;
            _power = power;
        }

        public override float Cost => 4f;
        public override SeedIntent Intent => SeedIntent.Trigger;

        public override void Seed(WorldReaction world, Vector3 playerPos)
        {
            world.ApplyElement(playerPos, _radius, ElementTag.Electric, _power);
        }
    }

    /// <summary>
    /// 쉬기 동작: 비로 불을 끄고 젖게 해 한숨 돌리게 한다.
    /// </summary>
    public sealed class SummonRain : SeedAction
    {
        readonly float _radius;
        readonly float _power;

        public SummonRain(float radius = 14f, float power = 1f)
        {
            _radius = radius;
            _power = power;
        }

        public override float Cost => 2f;
        public override SeedIntent Intent => SeedIntent.Calm;

        public override void Seed(WorldReaction world, Vector3 playerPos)
        {
            world.SetWind(world.Wind, 0f); // 바람을 재운다.
            world.ApplyElement(playerPos, _radius, ElementTag.Water, _power);
        }
    }
}
