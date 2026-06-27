# Basic Grass Fire

가장 단순한 한 바퀴 흐름 예제입니다.

## 쓰는 법

1. 빈 GameObject 를 만들고 `BasicGrassFireDirector` 컴포넌트를 붙입니다.
2. Play 합니다.
3. 좌상단 GUI 로 긴장도·국면·벌어지는 일을, Scene 뷰의 Gizmo 로 불타는 칸(빨강)과
   벌어지는 일(노랑)을 봅니다.
4. 인스펙터의 **Debug Threat** 슬라이더를 올리면 긴장도가 올라가
   디렉터가 쌓기 → 터짐 으로 넘어가 불씨를 던지는 흐름을 볼 수 있습니다.

## 한 바퀴

```csharp
_tension.Tick(player, _world, dt);          // ① 긴장도
_director.Tick(_world, _tension, player, dt); // ② 디렉터(조건만 깖)
_world.Tick(dt);                              // ③ 월드(번짐)
```

세 `Tick` 은 항상 **긴장도 → 디렉터 → 월드** 순서입니다.
