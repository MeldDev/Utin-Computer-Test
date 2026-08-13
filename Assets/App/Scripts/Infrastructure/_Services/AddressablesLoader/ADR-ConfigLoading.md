# ADR: ConfigLoading

## Решение

Конфиги — `ScriptableObject`-ы, зарегистрированные в `AddressableConfigsCatalog`.
На старте сцены preloader своего скоупа (Project, Map) предзагружает свой набор
конфигов; project-скоуп переживает смену сцены.

Игровой код получает конфиг через обёртку `IConfig<T>`; прямой `TConfig` в сервисы
не инжектится.

### Шпаргалка: как получить конфиг

| Где | Что инжектить |
|---|---|
| Сервис, UI, любой игровой код | `IConfig<TConfig>` |
| Внутри preloader-а или инфраструктуры | `IAddressableConfigProvider` |

В конструкторе сохраняем только ссылку. Читать `IConfig<T>.Value` можно начиная с
`IInitializable.Initialize()`.

### Зачем обёртка `IConfig<T>`

Конфиги грузятся асинхронно до старта стейт-машины. Если бы сервис инжектил сам
`TConfig`, контейнер не смог бы его выдать, пока загрузка не закончится, — а
ссылка нужна уже в момент сборки графа зависимостей.

`IConfig<T>` снимает эту проблему: контейнер выдаёт ссылку сразу, а `.Value` под
капотом ходит в `IAddressableConfigProvider` за уже загруженным конфигом. Чтение
`.Value` до завершения предзагрузки — исключение.

`ConfigInstaller` регистрирует `ConfigRef<T>` одним правилом для всех типов
конфигов, поэтому при добавлении нового конфига installer трогать не нужно.

### `IConfigsPreloader` — что грузится в каждой сцене

Один preloader на сцену:

- `ProjectConfigsPreloader` — конфиги проекта:
  - `PrefabAddresses`, `ProgressServiceConfig`, `AdvertisementConfig`
  - секция `[Header("Scenarios")]`: `ScenariosStorageConfig`
  - секция `[Header("Dialogues")]`: `DialoguesStorageConfig`,
    `DialoguePresentationConfig`, `CharactersStorageConfig`
  - и т.д.
- `MapConfigsPreloader` — конфиги карты + `CaposDatabase.PreloadAsync()` для
  вложенных `CapoConfig`. Примеры: `MapNavigationConfig`, `TileFireConfig`
  (крупный конфиг с данными тайлов), `TutorialRestrictionsConfig` и ещё ~25
  типов.

Внутри каждого — список `_configProvider.LoadAsync<TConfig>()`, обёрнутый в
`UniTask.WhenAll`.

Robbery и Fight своего preloader-а не имеют: нужные им конфиги уже загружены на
уровне Project и Map, а `IAddressableConfigProvider` живёт в project-контейнере
Zenject и переживает смену сцены.

### `ConfigBootDecorator` — кто запускает предзагрузку

Привязан в Bootstrap-сцене и в Map-сцене. На старте сцены выполняет последовательно:

1. Инициализирует Addressables.
2. Вызывает `IConfigsPreloader.PreloadAsync()` своего скоупа.
3. Запускает `InitializableManager` — `Initialize()` у всех `IInitializable`.
4. Снимает блокировку — `Update`, `LateUpdate`, `FixedUpdate`, `Dispose`,
   `LateDispose` начинают работать.

До конца этого процесса читать `IConfig<T>.Value` нельзя.

### Как добавить новый конфиг

| Шаг | Файл |
|---|---|
| 1. Поле `AssetReferenceT<MyConfig>` в нужном `[Header(...)]`-разделе | `AddressableConfigsCatalog.cs` |
| 2. Case в `GetReferenceFor<T>` | `AddressableConfigsCatalog.cs` |
| 3. `LoadAsync<MyConfig>` в нужном preloader | `ProjectConfigsPreloader` или `MapConfigsPreloader` |
| 4. Инжектить `IConfig<MyConfig>` | твой сервис |

`ConfigInstaller` трогать не нужно.

## Последствия

- Игровой код не зависит от порядка загрузки: ссылка `IConfig<T>` есть всегда,
  а `.Value` либо работает, либо громко падает.
- Добавление нового типа конфига не требует менять installer.
- Каждая сцена владеет своим списком конфигов.

## См. также
- [ADR: AssetLoading](ADR-AssetLoading.md) — ассеты по `AssetReference` (не-конфиги).
- [ADR: SceneBootstrap](../../Bootstrappers/ADR-SceneBootstrap.md) — место `ConfigBootDecorator` в общем потоке загрузки сцены.
