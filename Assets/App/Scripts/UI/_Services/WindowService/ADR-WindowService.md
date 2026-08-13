# ADR: WindowService

Централизованный сервис для создания, открытия и закрытия окон. Точка входа — [`IWindowService`](IWindowService.cs) с методами `OpenWindow<T>()`, `CloseWindow<T>()`, `GetWindow<T>()`. При первом обращении окно создаётся через `IWindowFactory` и регистрируется в `Windows`.

Каждое окно наследует [`BaseWindow`](../../../UI/Windows/BaseWindow.cs); для механизмов ниже важны два его поля в инспекторе:
- `Layer` (`WindowLayer`) — к какому UI-слою относится окно.
- `BlocksInput` — блокирует ли ввод карты пока открыто.

## Enum-доступ к окнам: WindowType + WindowTypeResolver

Альтернатива generic-API — открытие окна по значению enum [`WindowType`](WindowType.cs):

```csharp
var windowClass = WindowTypeResolver.ResolveType(WindowType.DialogueWindow);
_windowService.OpenWindow(windowClass); // через перегрузку, принимающую Type
```

[`WindowTypeResolver`](WindowTypeResolver.cs) — статический словарь `WindowType → System.Type`. Используется там, где тип окна известен как данные (сценарные экшены, конфиги), а не как compile-time generic.

**При добавлении нового окна**, которое должно быть доступно по `WindowType`: добавить значение в `WindowType`, **и** зарегистрировать пару `{ WindowType.X, typeof(XWindow) }` в `WindowTypeMap` внутри `WindowTypeResolver`. Если запись отсутствует — `ResolveType` бросит `ArgumentException` (fail-fast).

## WindowLayer и WindowBlockPolicy

`WindowLayer` (enum в [`WindowService.cs`](WindowService.cs)) описывает шесть слоёв UI:

```
Layer1_FullscreenFade, Layer2_Normal, Layer3_Navbar,
Layer4_TopPanel, Layer5_TopAndNavbar, Layer10_Tutorial
```

[`WindowBlockPolicy`](WindowBlockPolicy.cs) — статический предикат, не привязанный к сервису:

```csharp
WindowBlockPolicy.IsBlocked(
    windows.Select(w => (w.Layer, w.IsOpened)),
    blockingLayers);
```

Возвращает `true`, если хотя бы одно открытое окно принадлежит одному из `blockingLayers`. Используется внешними потребителями, которым нужно знать, «занят» ли нужный слой.

## Два механизма gating — не путать

| | Кто | Как | Когда срабатывает |
|---|---|---|---|
| **Блокировка ввода** | `WindowInputBlockerService` | подписывается на `OnWindowOpened/Closed`, проверяет `BlocksInput` | **немедленно** при открытии/закрытии |
| **Откладывание действия** | `DialoguePresenter`, `SetScenarioObjectStateActionHandler` | `WindowBlockPolicy.IsBlocked` + `IWindowService.Windows` | проверяют перед/после события, ждут закрытия нужного слоя |

`IWindowService.Windows` (`Dictionary<Type, BaseWindow>`) — через него внешние потребители перечисляют текущие окна при вызове `IsBlocked`.

## Последствия

1. Все окна должны регистрироваться через `WindowService` — иначе события и оба механизма gating не сработают.
2. Каждое окно обязано выставить **оба** поля: `BlocksInput` (используется `WindowInputBlockerService`) и `Layer` (используется `WindowBlockPolicy`).
