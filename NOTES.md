# Random Events

`RandEventSystem.UpdateRandomEvent`:

```
// if (Game.m_eventRate > 0f && m_eventTimer > m_eventIntervalMin * 60f * Game.m_eventRate)
IL_001d: ldsfld float32 Game::m_eventRate
IL_0022: ldc.r4 0.0
IL_0027: ble.un.s IL_0071
IL_0029: ldarg.0
IL_002a: ldfld float32 RandEventSystem::m_eventTimer
IL_002f: ldarg.0
IL_0030: ldfld float32 RandEventSystem::m_eventIntervalMin
IL_0035: ldc.r4 60
IL_003a: mul
IL_003b: ldsfld float32 Game::m_eventRate
IL_0040: mul
IL_0041: ble.un.s IL_0071
```

Raid Rate Settings:

- Day Length: 1800s

|                 | none | much less | less | normal | more | much more |
| --------------- | ---- | --------- | ---- | ------ | ---- | --------- |
| m_eventRate     | 0    | 2         | 1.5  | 1      | 0.6  | 0.3       |
| interval (secs) | 0    | 5520      | 4140 | 2760   | 1656 | 828       |
| interval (days) | 0    | 3.06      | 2.3  | 1.53   | 0.92 | 0.46      |
