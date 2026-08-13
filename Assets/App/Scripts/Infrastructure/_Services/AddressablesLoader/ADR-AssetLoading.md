# ADR: AssetLoading

## Решение

Игровой код грузит ассеты по `AssetReference` (префабы, иконки, материалы)
только через `IAddressableAssetProvider`.

Низкоуровневый `IAddressablesLoader` — для сцен и handle-ов; используется только
из инфраструктуры.

Конфиги-`ScriptableObject` имеют свой отдельный API — см.
[ADR: ConfigLoading](ADR-ConfigLoading.md).

### Шпаргалка: что инжектить

| Что нужно | Куда смотреть |
|---|---|
| Иконка, материал, общий префаб | `IAddressableAssetProvider.LoadAsync` |
| Component из общего префаба | `IAddressableAssetProvider.LoadComponentAsync` |
| Префаб с явным временем жизни | `IAddressableAssetProvider.AcquireAsync` + `Release` |
| Component с явным временем жизни | `IAddressableAssetProvider.AcquireComponentAsync` + `Release` |
| Уже загруженное окно/префаб (синхронно) | `IAddressableAssetProvider.GetLoaded` / `GetLoadedComponent` |
| Проверить, загружен ли ассет (без исключения) | `IAddressableAssetProvider.IsLoaded` |
| UI-картинка по `AssetReference` | `AddressableImage` |
| UI с переключением ассета на лету | `AssetReferenceLoader<T>` |
| Загрузка сцены или handle | `IAddressablesLoader` (только в инфраструктуре) |

### Правила

- Не звать `UnityEngine.AddressableAssets.Addressables.*` напрямую из игрового
  кода или UI.
- Не использовать `WaitForCompletion()`.
- `GetLoaded` и `GetLoadedComponent` бросают исключение, если ассет ещё не
  загружен. Это намеренно: неправильный порядок инициализации видно сразу.

### Разница `LoadAsync` и `AcquireAsync`

- `LoadAsync` — ассет общий, нужен надолго, освобождать вручную не надо.
  Примеры: иконки, материалы, окна, общие префабы.
- `AcquireAsync` + `Release` — у ассета есть владелец и момент, когда его пора
  освободить. Каждый `Acquire` обязан быть сбалансирован `Release`.
  Примеры: окружение ограбления, окружение боя, префаб текущего сектора,
  runtime-префаб капо.

### `AddressableImage`

Компонент `Image`, расширенный для addressable-ассетов. Умеет грузить как `Sprite`
(через `SetReference`), так и `Material` (через `SetReferenceMaterial`). Оба лоадера
работают независимо — можно задать оба одновременно.

### `AssetReferenceLoader<T>`

Помощник для UI, где важен только последний загруженный ассет — например, иконка
карточки, которая перепривязывается к другой ссылке при смене модели. Сам ассетами
не владеет, `Release` не вызывает.

## Последствия

- Игровой код не зависит от `UnityEngine.AddressableAssets`.
- Неправильный порядок инициализации виден сразу — `GetLoaded` бросает исключение.
- Ассеты с явным временем жизни освобождаются детерминированно через `Release`.

## См. также

- [ADR: ConfigLoading](ADR-ConfigLoading.md) — конфиги-`ScriptableObject` и их жизненный цикл.
- [ADR: SceneBootstrap](../../Bootstrappers/ADR-SceneBootstrap.md) — когда что предзагружается.
