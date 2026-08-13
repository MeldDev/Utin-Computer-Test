# ADR: SceneBootstrap

## Решение

Каждая сцена проходит через единообразную цепочку стейтов. Fight — исключение: у него нет отдельного префаб-стейта.

Префабы окон сцены грузятся в её начале отдельным префаб-стейтом. Сценовые ассеты
с явным временем жизни грузят сами сервисы по мере необходимости.

Конфиги предзагружаются ещё до старта стейт-машины — см.
[ADR: ConfigLoading](../_Services/AddressablesLoader/ADR-ConfigLoading.md).

### Цепочки стейтов

```
Project:  AppSettings → ProgressLoading → PrefabsPreload → Initialization → SceneLoading(Map)
Map:      ProgressLoading → PrefabsPreload → Initialization → AwaitGameplay → Gameplay
Robbery:  ProgressLoading → PrefabsPreload → Initialization → AwaitGameplay → Gameplay
Fight:    ProgressLoading → Initialization → AwaitGameplay → Gameplay
```

### `PrefabsPreloadState` — загрузка окон в начале сцены

Префаб-стейт цепочки в Project, Map и Robbery. Грузит окна через
`_assetProvider.LoadAsync<GameObject>(prefabAddresses.X)` внутри `UniTask.WhenAll`
и переходит в следующий стейт.

| Сцена | Что грузит |
|---|---|
| Project | `SettingsWindow`, `LanguageSelectionWindow` |
| Map | окна карты — карточки капо, шоп, туториал, диалоги, дебаг и т.д. (полный список — в `MapPrefabsPreloadState.cs`) |
| Robbery | `RobberyCapoChooseWindow`, `RobberyResultsWindow`, `DialogueWindow`, `Tutorial*` |

Fight своего `PrefabsPreloadState` не имеет — нужные ему префабы и эффекты грузит
сервис боя при подготовке команды противника.

### Загрузка под затемнением, показ после

Когда инициализация сервиса включает и загрузку ассетов, и видимое игроку
действие (вступительная анимация, открытие окна), её делят на две фазы:

- `LoadAsync` — вызывается в `InitializationState`, пока экран затемнён.
  Грузит окружение, заполняет регистры. Ничего видимого не делает.
- `PresentAsync` — вызывается в `AwaitGameplayState`, параллельно с ожиданием
  снятия затемнения. Играет вступительные анимации, открывает окна.

Без такого разделения анимация проигралась бы под чёрным экраном — и игрок её
не увидел бы.

Пример: `RobberyPreparationService`.

### Как добавить окно в предзагрузку

| Шаг | Файл |
|---|---|
| 1. Поле `AssetReferenceGameObject MyWindow` | `PrefabAddresses.cs` |
| 2. `LoadAsync<GameObject>(prefabAddresses.MyWindow)` | `<Scope>PrefabsPreloadState.cs` |

### Scene-scoped регистрация модулей

Помимо стейтов, scene-installer регистрирует модули с временем жизни сцены.
Это компенсирует отсутствие DI внутри Instantiate-flow контента; модули дерегистрируются при выгрузке сцены:

| Сцена | Регистрирует в scene-scope |
|---|---|
| Map, Robbery | `DialoguePresenter`, `SetScenarioObjectStateActionHandler`, `SceneScenarioHandlersRegistrar` |
| Map | `TutorialRestrictionsCoordinator` (+ `ITutorialRestriction` через `TutorialRestrictionsFromConfigRestriction`), `ObjectVisibilityController` (из иерархии), подсистема TileFire: `TileFireService`/`TileFireBoosterService`/`TileFireHoseController`/`TileFireAssetOwner`/`TileFireBoosterController` и сопутствующие runtime-классы |
| Fight | `DialoguePresenter` |

Подробнее — [ADR: DialogueSystem](../../Gameplay/Dialogues/ADR-DialogueSystem.md) и
[ADR: ScenariosSystem](../../Gameplay/Scenarios/ADR-ScenariosSystem.md).

## Последствия

- Единая форма цепочки: `Progress → Prefabs → Init → Await → Gameplay`
  (Fight — без Prefabs).
- Чёткая граница «что готово к какому стейту».
- Видимые анимации не теряются за затемнением благодаря разделению `Load` и
  `Present`.

## См. также
- [ADR: AssetLoading](../_Services/AddressablesLoader/ADR-AssetLoading.md) — какой API чем грузить.
- [ADR: ConfigLoading](../_Services/AddressablesLoader/ADR-ConfigLoading.md) — `ConfigBootDecorator` и предзагрузка конфигов до стейт-машины.
