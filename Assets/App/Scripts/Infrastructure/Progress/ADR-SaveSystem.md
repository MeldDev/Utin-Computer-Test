# ADR: SaveSystem

## Как добавить новые сохраняемые данные

1. Создайте класс данных: `*Progress` или `*State`.
2. Создайте `*Service`. Он хранит эти данные во время игры и вызывает `Changed` при изменении данных.
3. Создайте `*Repository : ProgressRepository<TState>`. Репозиторий только загружает, сохраняет и следит за изменениями.
4. Выберите одну группу загрузки:
   - `IProjectLoadProgressRepository`: данные нужны сразу после старта игры;
   - `IMapLoadProgressRepository`: данные нужны только на карте;
   - `IRobberyLoadProgressRepository`: данные нужны только в robbery;
   - `IFightLoadProgressRepository`: данные нужны только в fight.
5. В репозитории реализуйте:
   - `PartId`;
   - `Version` — `virtual`, дефолт `1`; переопределяйте только при смене формата данных;
   - `CreateDefaultState()`;
   - `CaptureState()`;
   - `RestoreState()`;
   - `SubscribeToChanges()`;
   - `UnsubscribeFromChanges()`.
6. Зарегистрируйте сервис и репозиторий в нужном инсталлере через `BindInterfacesAndSelfTo<*Repository>().AsSingle()`. Репозиторий регистрируется в `SaveSystem` прямо из конструктора `ProgressRepository<TState>` — именно поэтому `BindInterfacesAndSelfTo` (а не просто `Bind`) обязателен: иначе маркер группы не попадёт в Zenject list binding.
7. В игровом коде работайте с сервисом. Репозиторий не должен содержать игровую логику.

## Как это работает

Прогресс хранится не одним большим объектом, а отдельными частями. Каждая часть сохраняется под своим `PartId`.

`SaveSystem` живет в `ProjectContext`. Он знает все созданные репозитории и умеет:

- загрузить одну часть;
- загрузить группу частей;
- сохранить одну часть (`SaveAsync<TRepository>()`);
- сохранить все измененные части (`SaveDirtyAsync()`);
- сохранить все части (`SaveAllAsync()`).

После любой операции сохранения `SaveSystem` сам вызывает `ISaveBackend.FlushAsync()` (важно для `PlayerPrefsSaveBackend`, который фактически пишет на диск именно в `Flush`).

Сами данные сейчас пишутся в `PlayerPrefs` через `PlayerPrefsSaveBackend`. Формат каждой части: JSON с `Version` и `State`.

Если ключа части еще нет, репозиторий создает новое состояние через `CreateDefaultState()` и помечает часть как измененную.

Старые монолитные сохранения не читаются и не мигрируются. Если новой части еще нет, игра берет состояние из `CreateDefaultState()`.

## Где загружать

Репозиторий должен жить там же, где нужны его данные.

- Данные всего проекта: `ProjectInstaller`, загрузка в `ProjectProgressLoadingState`.
- Данные карты: `MapSceneInstaller`, загрузка в `MapProgressLoadingState`.
- Данные robbery: `RobberySceneInstaller`, загрузка в `RobberyProgressLoadingState`.
- Данные fight: `FightSceneInstaller`, загрузка в `FightProgressLoadingState`.

Важно: один репозиторий должен быть только в одной группе загрузки.

## Автосохранение

Когда сервис вызывает `Changed`, репозиторий помечает свою часть как измененную. В коде это состояние называется dirty: данные изменились и ждут записи.

`AutoSaveService` вызывает `SaveSystem.SaveDirtyAsync()`:

- по таймеру;
- при потере фокуса;
- при паузе приложения;
- при закрытии приложения;
- при смене активной сцены.

`SaveDirtyAsync()` сохраняет только измененные части, у которых `AutoSaveEnabled == true`.

## Правила

- `PartId` должен быть уникальным и не должен меняться после релиза.
- `LoadAsync()` только восстанавливает состояние. Он не должен начислять награды, запускать таймеры или выполнять игровую логику.
- `CreateDefaultState()` всегда возвращает новый объект.
- Нет неявного deep copy: `CaptureState()` и `RestoreState()` должны явно делать снимок mutable-состояния. Если состояние можно менять по ссылке — делайте копию.
- Не отдавайте наружу внутренние изменяемые объекты сервиса без копии.
- Если меняете формат данных, сначала продумайте поддержку новой `Version`.
