# SaveParts

Новая save/load-система построена вокруг:

- `*Service` как владельца runtime-state и бизнес-логики;
- `*Repository` как persistence-адаптера одной части прогресса;
- `SaveSystem` как orchestration-слоя;
- `AutoSaveService` как общего lifecycle/timer-trigger;
- `ISerializer` и `ISaveBackend` как backend-слоя.

## Базовые правила

- Сервис владеет состоянием и доменной логикой.
- Репозиторий подписывается на `Changed`, держит `IsDirty` и умеет `LoadAsync()` / `SaveAsync()`.
- `PartId` одновременно является идентификатором репозитория и физическим ключом сохранения.
- При отсутствии сохранения репозиторий поднимает default-state через `CreateDefaultState()` и помечает себя dirty.
- `CreateDefaultState()` всегда должна возвращать новый независимый объект.
- В новой save-системе не используется скрытый deep copy. Если конкретному репозиторию нужна копия, он делает ее явно сам.
- `SaveSystem` работает только с зарегистрированными репозиториями и сам вызывает `ISaveBackend.FlushAsync()`.

## Как добавить новую сохраняемую часть

1. Определить `*State`.
2. Сделать `*Service`, который владеет этим состоянием и поднимает `Changed`.
3. Сделать `*Repository : ProgressRepository<TState>` и реализовать ровно один маркер группы загрузки:
   - `IProjectLoadProgressRepository`
   - `IMapLoadProgressRepository`
   - `IRobberyLoadProgressRepository`
   - `IFightLoadProgressRepository`
4. Реализовать в репозитории:
   - `PartId`
   - `Version` — `virtual`, дефолт `1`; переопределяйте только при смене формата данных
   - `CreateDefaultState()`
   - `CaptureState()`
   - `RestoreState()`
   - `SubscribeToChanges()`
   - `UnsubscribeFromChanges()`
5. Забиндить сервис и репозиторий в правильном scope через `BindInterfacesAndSelfTo<*Repository>().AsSingle()`, чтобы маркер группы попал в Zenject list binding.
   Учтите `AutoSaveEnabled`: по умолчанию `true` в базовом классе; если переопределить в `false`, репозиторий исключается из `SaveDirtyAsync()`.
6. Явно загрузить группу из нужной точки входа:
   - project-level части: через `ProjectProgressLoadingState`
   - scene/mechanic-level части: через scene bootstrap / `ProgressLoadingState`

## Текущее состояние

- Все боевые сохранения идут через `SaveSystem` и `ProgressRepository<TState>`.
- `ProjectProgressLoadingState` получает `List<IProjectLoadProgressRepository>` и грузит project-level части через `LoadAsync(...)`.
- `ProgressLoadingState` сцены/механики получает свой список группы и грузит только его через `LoadAsync(...)`.
- Формат любой части: envelope с `Version` и `State`.
- `Fight` использует прямое обновление `FightProgressService`, без `FightResultForProgress`.
- `Robbery` сохраняет lineup отдельно в `robbery/lineup`.
- `ScenariosProgressRepository` (`"scenarios"`, project-level) — хранит состояния сценарных объектов.
- `AudioSettingsRepository` (`"project/audio-settings"`, project-level) — хранит настройки звука.
- `TileUpgradePreferencesRepository` (`"project/tile-upgrade-preferences"`, `IProjectLoadProgressRepository`) — хранит пользовательские настройки апгрейда тайлов (например, режим максимального апгрейда).
- `TileProgressData` расширен fire-полями (`FireState`, `FireLevelLoss`, `FireDamageProgress`, `FireExtinguishProgress`, `FireSectors` и др.) — актуально при версионировании tile-частей.

## Практические правила

- `CaptureState()` и `RestoreState()` должны работать через явные snapshot-копии, если состояние mutable.
- Каждый `ProgressRepository<TState>` должен входить ровно в одну группу загрузки. Именно resolve этого списка создает репозитории до загрузки и регистрирует их в `SaveSystem`.
- `IsDirty` управляется базовым `ProgressRepository<TState>`: конкретный репозиторий только вызывает `MarkDirty()` (обычно из обработчика `Changed`).
- `AutoSaveService` триггерит сохранение по таймеру, при потере фокуса, паузе, закрытии и при смене активной сцены (`SceneManager.activeSceneChanged`).
- Репозиторий не должен зависеть от старого `SaveLoadService` или key-provider слоя.
