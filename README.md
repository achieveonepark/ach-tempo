# Tempo

> 게임이 알아서 위기 상황을 만들어내는 시스템. **연출 담당(디렉터)** 이 불·비·바람 같은 조건만 슬쩍 깔아두면, **월드 반응** 쪽이 그 조건을 받아 불이 번지듯 상황을 키웁니다. 한 장면 한 장면 미리 짜두지 않아도 매번 다른 상황이 알아서 벌어집니다.

두 게임에서 아이디어를 가져왔습니다 — **Left 4 Dead**(상황의 강약을 자동 조절) + **Zelda: BotW**(불·물·전기가 정해진 규칙대로 서로 반응).

**이름이 Tempo인 이유:** 결국 게임의 긴장 '템포(강약)'를 조절하는 일을 하기 때문입니다.

## 설치

Unity Package Manager → **Add package from git URL**:

```
https://github.com/achieveonepark/ach-tempo.git
```

또는 `manifest.json`:

```json
{
  "dependencies": {
    "com.achieve.tempo": "https://github.com/achieveonepark/ach-tempo.git"
  }
}
```

## 핵심 구조

```
[디렉터]  ──조건 깔기(불·비·바람·번개)──▶  [월드 반응]
   ▲                                          │
   │  긴장도 되돌려받기                        │ 결과 계산 (번짐·꺼짐·전기 통함)
   │  (플레이어 위협 + 주변에서 벌어지는 일)   ▼
   └──────────────  [긴장도 계산기]  ◀── 플레이어 위협 / 지금 벌어지는 일
```

- **`WorldReaction`** — 격자에서 규칙대로 번지고, '지금 벌어지는 일' 목록을 갱신합니다. 디렉터의 존재를 모릅니다.
- **`DirectorBrain`** — 긴장도 하나를 보고 국면(쌓기→터짐→쉬기)을 정해, 예산 안에서 동작을 골라 월드에 조건만 깝니다.
- **`TensionModel`** — 플레이어 위협 + 주변에서 벌어지는 일을 합쳐 긴장도(0~1)를 냅니다.

세 개의 `Tick` 은 항상 **긴장도 → 디렉터 → 월드** 순서로 부릅니다.

## 빠른 시작

```csharp
using Achieve.Tempo.Director;
using Achieve.Tempo.Tension;
using Achieve.Tempo.World;
using UnityEngine;

public class TempoExample : MonoBehaviour
{
    WorldReaction _world;
    DirectorBrain _director;
    TensionModel  _tension;

    void Start()
    {
        _world = new WorldReaction(width: 64, height: 64, cellSize: 1f);
        _world.Fill("Grass");                       // 풀밭을 깐다
        _world.SetWind(new Vector2(1f, 0f), 1.2f);  // 바람을 동쪽으로

        _director = new DefaultDirectorBrain();
        _tension  = new TensionModel();
    }

    void Update()
    {
        float dt = Time.deltaTime;
        Vector3 player = transform.position;

        _tension.Tick(player, _world, dt);     // ① 긴장도
        _director.Tick(_world, _tension, player, dt); // ② 디렉터
        _world.Tick(dt);                        // ③ 월드
    }
}
```

기본 데이터(규칙표·동작·기준값)가 들어 있어 설정 없이 바로 돕니다.
값을 갈아끼우려면 `DataService<T>.Register(...)` 로 자기 데이터센터를 등록하세요.

## 바깥에서 쓰는 부분

| 클래스 | 바깥에 보이는 것 | 하는 일 |
|---|---|---|
| `WorldReaction` | `ApplyElement`, `SetWind`, `ActiveEvents`, `Tick` | 번짐·벌어지는 일 |
| `DirectorBrain` | `Tick`, `ActionPool`(virtual), `PickActions`(virtual) | 국면·동작 고르기 |
| `SeedAction` | `Cost`, `Intent`, `CanSeed`, `Seed` | 조건 깔기 한 종류 |
| `TensionModel` | `Current`, `OnPlayerDamaged`, `Tick` | 긴장도 계산 |
| `PhaseFsm` | `Evaluate` | 국면 전환 |

## 문서

전체 문서: `docs~/` (fumadocs). 한국어/English/日本語/中文.

## 라이선스

[MIT](LICENSE.md)
