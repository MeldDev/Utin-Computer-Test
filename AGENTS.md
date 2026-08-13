\# Project Instructions



\## Project Context



This is a Unity project. Follow the existing project architecture and coding conventions before adding new patterns.



The project uses:



\* Unity

\* C#

\* Zenject for dependency injection

\* UniRx for reactive events and state subscriptions

\* MVP architecture for UI and gameplay presentation logic

\* Addressables for loading configs and assets



Prefer small, explicit, maintainable changes over broad rewrites.



\---



\## Architecture



Use MVP as the default architecture pattern.



\### MVP Responsibilities



\* `Model` contains data and domain state.

\* `View` contains only Unity references, visual updates, and user input observables.

\* `Presenter` connects model/service state to the view and handles user interaction.

\* Views must not contain business logic.

\* Presenters must not directly search the scene or instantiate dependencies manually.

\* Services contain reusable gameplay/application logic.

\* Installers are composition roots and must only bind dependencies.



\### View Rules



Views should expose user input as `IObservable<T>`.



Correct example:



```csharp

\[SerializeField] private Button \_playButton;

\[SerializeField] private Button \_settingsButton;

\[SerializeField] private Button \_exitButton;



public IObservable<Unit> OnPlayButtonClicked() => \_playButton.OnClickAsObservable();



public IObservable<Unit> OnSettingsButtonClicked() => \_settingsButton.OnClickAsObservable();



public IObservable<Unit> OnExitButtonClicked() => \_exitButton.OnClickAsObservable();

```



Do not expose `UnityEvent`, `Action`, or public mutable fields for UI events when UniRx can be used.



Views may expose methods like:



```csharp

public void SetVisible(bool isVisible)

{

&#x20;   gameObject.SetActive(isVisible);

}



public void SetLevelText(int level)

{

&#x20;   \_levelText.text = level.ToString();

}

```



Views must not:



\* Resolve dependencies from Zenject directly.

\* Read configs directly unless explicitly designed as a pure view config.

\* Contain gameplay calculations.

\* Call services directly.

\* Use `FindObjectOfType`, `GameObject.Find`, or service locators.



\### Presenter Rules



Presenters should:



\* Receive dependencies through constructors.

\* Subscribe to view observables.

\* Subscribe to model/service observables.

\* Own a `CompositeDisposable`.

\* Dispose subscriptions when the presenter is disposed.





Do not put subscription logic into the view unless it is purely visual and local to the view.



\---

Unity Component Reference Rules



Avoid GetComponent, GetComponents, GetComponentInChildren, and GetComponentInParent in gameplay, UI, presenter, service, and runtime initialization code.



Required Unity component references must be assigned explicitly through the Inspector using \[SerializeField] private fields.

\---



\## UniRx Rules



Use UniRx for events, state updates, and reactive flows.



Preferred patterns:



\* `IObservable<T>` for public event streams.

\* `Subject<T>` or `ReactiveCommand<T>` for internal event sources.

\* `ReactiveProperty<T>` / `IReadOnlyReactiveProperty<T>` for observable state.

\* `CompositeDisposable` for subscription lifetime.

\* `AddTo(\_disposables)` for presenter/service-owned subscriptions.

\* `AddTo(this)` only inside MonoBehaviours when the lifetime must match the GameObject.



Avoid:



\* Raw C# events for gameplay/UI events.

\* Public `Subject<T>`.

\* Exposing mutable `ReactiveProperty<T>` from models/services.

\* Long-lived subscriptions without disposal.

\* Mixing UnityEvent and UniRx for the same interaction.



Correct encapsulation:



```csharp

private readonly Subject<Unit> \_completed = new();



public IObservable<Unit> Completed => \_completed;

```



For state:



```csharp

private readonly ReactiveProperty<int> \_level = new();



public IReadOnlyReactiveProperty<int> Level => \_level;

```



\---



\## Zenject Rules



Use Zenject for dependency injection.



Dependencies must be injected through constructors whenever possible.





Avoid:



\* Manual `new` for services that have dependencies.

\* Static service access.

\* Service Locator usage.

\* `FindObjectOfType`.

\* Scene installers containing feature-specific gameplay construction unless the feature must live in the scene.



\### Installer Rules



Project-level services belong in `ProjectContext` installers.



Scene installers should only bind:



\* Scene-specific views.

\* Scene-specific entry points.

\* Scene-owned MonoBehaviours.

\* Scene objects that must be connected to global services.

\* Runtime entities that are intentionally spawned or registered from the scene.



Scene installers must not become gameplay feature containers.



Do not put feature settings directly into scene installers unless they are scene-only references.



Prefer:



```csharp

Container.BindInterfacesTo<MainMenuPresenter>().AsSingle();

Container.Bind<MainMenuView>().FromInstance(\_mainMenuView).AsSingle();

```



For services:



```csharp

Container.BindInterfacesTo<PlayerHealthService>().AsSingle();

```



For factories and runtime entities, prefer Zenject factories/pools instead of manual instantiation.



\---



\## Configs and Addressables



All gameplay mechanic settings must be moved into configs.



Use `AddressableConfigsCatalog` as the central catalog for addressable configs.



Large feature configs should be separate addressable config assets.



Small nested settings should be placed inside larger global configs using serializable classes.





Rules:



\* Do not hardcode mechanic values in presenters, services, views, installers, or MonoBehaviours.

\* Do not store gameplay balance values in scenes.

\* Do not use `Resources.Load`.

\* Do not create duplicate config loading paths.

\* Configs should be loaded through the existing addressable config provider/catalog flow.

\* Release Addressables handles according to the existing project pattern.



\---



\## Scene Rules



Scenes are composition layers, not gameplay feature containers.



A scene may contain:



\* Visual layout.

\* Scene views.

\* Scene-specific MonoBehaviours.

\* Scene installer.

\* Entry points.

\* Spawn points.

\* Cameras, lights, UI roots, and scene references.



A scene should not contain:



\* Gameplay balance settings.

\* Feature logic.

\* Service state.

\* Hidden singleton objects.

\* Hardcoded config references outside the established config flow.



If a gameplay feature must be scene-bound, keep only the Unity-facing part in the scene and inject all services/configs through Zenject.



\---



\## Build, Test, and Development Commands



Run commands from repository root unless noted.



Run EditMode tests:



```bash

Unity.exe -batchmode -quit -projectPath SurvivalGame -runTests -testPlatform editmode -testResults test-results/editmode.xml

```



Run PlayMode tests:



```bash

Unity.exe -batchmode -quit -projectPath SurvivalGame -runTests -testPlatform playmode -testResults test-results/playmode.xml

```



If tests cannot be run, explain exactly why and what was checked manually instead.



\---



\## Coding Style \& Naming Conventions



Style is defined in `SurvivalGame/.editorconfig`.



\* C#: 4-space indent, block-scoped namespaces, explicit access modifiers, Allman braces.

\* Use 2 spaces for `.meta`, `.asmdef`, and `.json`.

\* Prefer `var` when the type is obvious.

\* Never inline methods into a single line, even for trivial bodies.

\* Expression-bodied properties are allowed for simple getters.

\* Short `if` branches and early exits may stay without braces, but the statement body must be on the next line.

\* Use braces for any `for`, `foreach`, `while`, and `do` loop body.

\* If an `if/else` condition does not fit one line, or either branch spans multiple statements, use braces.

\* If one branch of an `if/else` needs braces, both branches must use braces.

\* Private and serialized fields use underscore camelCase: `\_playerLevel`.

\* Keep inspector fields as `\[SerializeField] private ...`.

\* Avoid public fields except for required Unity data models or serialization DTOs.

\* Use explicit names. Do not use vague names like `data`, `manager`, `handler`, or `temp` unless the context is very narrow and obvious.



\---



\## Null Handling and Fail-Fast Policy



Do not add defensive null-checks for required `\[SerializeField]` fields.



Do not add defensive null-checks for DI-injected dependencies.



If a serialized field is not assigned or a DI binding is missing, treat it as a configuration bug and allow an immediate exception.



Correct:



```csharp

public PlayerPresenter(PlayerView view, IPlayerService playerService)

{

&#x20;   \_view = view;

&#x20;   \_playerService = playerService;

}

```



Avoid:



```csharp

if (\_view == null)

&#x20;   return;

```



Avoid silently hiding configuration errors.



Use validation only when it gives a clear editor-time or startup-time error.



\---



\## Async Rules



Use UniTask when asynchronous Unity code is required.



Avoid `async void` except for Unity event entry points where no alternative exists.



For fire-and-forget operations, use `.Forget()` only when errors are intentionally handled or acceptable according to the existing project pattern.



Do not mix coroutines, UniTask, and UniRx in the same flow unless there is a clear reason.



Prefer cancellation tokens for long-running async operations.



\---



\## Testing Rules



When adding or changing pure logic, prefer EditMode tests.



Keep domain services testable without requiring scene objects.



Avoid putting logic into MonoBehaviours if it can be tested as a plain C# service.



Tests should cover:



\* Config-driven calculations.

\* Model state changes.

\* Service behavior.

\* Presenter reactions to observable events where practical.



\---



\## Agent Workflow



Before making changes:



\* Inspect the existing architecture and naming patterns.

\* Reuse existing abstractions.

\* Prefer minimal changes.

\* Do not introduce a new architecture style without explicit instruction.

\* Do not create duplicate services/config providers/event systems.

\* Do not rewrite unrelated code.



After making changes:



\* Check for compile errors.

\* Run relevant tests when possible.

\* Summarize what changed.

\* Mention any tests that were not run.

\* Mention any assumptions.



\---



\## Prohibited Patterns



Do not introduce:



\* Static global services.

\* New service locators.

\* `FindObjectOfType` / `GameObject.Find` for dependencies.

\* Public mutable state where read-only state is enough.

\* Public `Subject<T>`.

\* Hidden scene singletons.

\* Gameplay configs stored directly in installers.

\* Hardcoded mechanic values.

\* `Resources.Load`.

\* Large god services or managers.

\* Business logic inside views.

\* UI event logic based on `UnityEvent` when UniRx observables are appropriate.

