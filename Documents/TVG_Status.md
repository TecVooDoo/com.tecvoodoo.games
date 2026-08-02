# TecVooDoo Games - Status

**Package:** `com.tecvoodoo.games` v1.3.0
**Type:** UPM local package (shared library)
**Source:** `E:\Unity\DefaultUnityPackages\com.tecvoodoo.games\`
**Namespace:** `TecVooDoo.Games`
**Installed in:** TecVooDoo game projects (not tools/sandbox projects)
**Depends on:** `com.tecvoodoo.utilities`

**Reference doc:** `TVG_Reference.md` -- full API

---

## Current Contents

| Module | Files | Status |
|--------|-------|--------|
| Simulation | SimpleBoids | Stable -- moved from TVU |
| Pooling | BulletHoleSpawner | Stable |
| StateMachine | CharacterStateMachine, Transition | Stable -- CRTP state machine from adammyhre |
| Processing | ProcessorChain | Stable -- generic processor chains from adammyhre |
| Reactive | Observable\<T\> | New -- reactive property with ValueChanged event |
| Functional | Either\<TLeft,TRight\>, Optional\<T\>, Preconditions | New -- monads + guard clauses |
| Effects | IEffect\<T\>, DamageEffect, DamageOverTimeEffect | New -- effect/DOT system using TVU IntervalTimer |
| Serialization | SerializableType, TypeFilterAttribute | New -- serialize System.Type in Inspector |
| Collections | PriorityQueue\<TElement,TPriority\> | New -- generic priority queue |

---

## Tests

**70 tests, 70 passing** -- first suite, added 2026-08-02; run in TVD on Unity 6000.5.5f1 (0 failed, 0 skipped). Before this, TVG had **zero** coverage: `TecVooDoo.Games.Tests.asmdef` existed and was correctly configured, but the folder held 0 `.cs` files, so Unity logged *"will not be compiled, because it has no scripts associated with it"* and the assembly never appeared in `CompilationPipeline` at all.

| Fixture | Tests | Covers |
|---------|-------|--------|
| `EitherTests` + `OptionalTests` | 26 | Left/Right access + wrong-side throws, `Match`, `Select` mapping Right while passing Left through, `SelectMany` short-circuit (binder must not run on the empty side), Optional equality / bool conversion / `ToString` |
| `ObservableTests` | 13 | Initial value, the equality short-circuit (setting an equal value must NOT fire), add/remove listener, multiple listeners, `Invoke` without mutation, implicit conversion, `Dispose` clearing listeners AND value |
| `PriorityQueueTests` | 12 | Priority ordering, FIFO within a priority, count tracking, empty-queue throws, `Clear`, re-enqueue after clear, negative/zero priorities, and the drain-then-advance path where an emptied priority level is removed from the `SortedList` |
| `PreconditionsTests` | 11 | `CheckNotNull` / `CheckState` incl. message + template overloads, and the destroyed-`UnityEngine.Object` case that is the whole reason `CheckNotNull` special-cases Unity objects |
| `ProcessorChainTests` | 8 | `CombinedProcessor` ordering, chaining across type changes, 3-stage chains, `Compile` equivalence, and that building/compiling a chain does not execute it |

**Not yet covered:** `SimpleBoids` and `BulletHoleSpawner` (the only two MonoBehaviours), `CharacterStateMachine` / `Transition`, `DamageEffect` / `DamageOverTimeEffect` (needs a TVU `IntervalTimer` harness), and `SerializableType`.

**Running them -- three gates that each look like "no tests exist":**

1. **The consuming project must list `com.tecvoodoo.games` in `Packages/manifest.json` `"testables"`.** Without it UPM never compiles the test assembly. TVD gained this entry 2026-08-02.
2. **After editing `testables`, force a package resolve** -- `UnityEditor.PackageManager.Client.Resolve()`. An `assets-refresh` alone leaves the assembly absent from `CompilationPipeline.GetAssemblies()`.
3. **They run in PlayMode, not EditMode** -- the asmdef uses `includePlatforms: []` rather than `["Editor"]`, so an EditMode run reports "No tests found" even once the assembly compiles.

**Gotcha -- the MCP `tests-run` call can time out while the run itself succeeds.** A PlayMode run crosses the play-mode domain reload, and on the first run of a new assembly it also builds the InitTestScene, which can exceed the MCP client's idle timeout. The tool then reports a timeout even though Unity finished fine. **The authoritative result is `TestResults.xml`** at `%USERPROFILE%\AppData\LocalLow\<Company>\<Product>\TestResults.xml` (here: `.../LocalLow/DefaultCompany/TecVooDoo/TestResults.xml`) -- its root node carries `total` / `passed` / `failed` / `skipped`. Read that before concluding a run failed.

---

## Sessions

**Session 0 (2026-03-16) -- Scaffold + First Migration + BulletHoleSpawner:**
Package created. package.json, asmdefs (Runtime/Editor/Tests), and docs established. SimpleBoids migrated from TecVooDoo Utilities. BulletHoleSpawner added (Runtime/Pooling/) from adammyhre gist -- var fixed, namespace updated, createFunc extracted to separate method. Version bumped to 1.1.0.

**Session 1 (2026-03-16) -- adammyhre Gist Review + StateMachine + Processing:**
Reviewed all 10 adammyhre gists. Added 2 modules:
- **StateMachine/** -- CRTP state machine (CharacterStateMachine.cs + Transition.cs). Zero external deps. var removed, namespace added.
- **Processing/** -- Generic processor chain (ProcessorChain.cs). IProcessor interface + fluent chain builder. var removed, namespace added.
- AOETargeting.cs REJECTED for TVG -- too coupled to adammyhre's own UnityUtils + custom interfaces (TargetingStrategy, Ability, IDamageable). Uses LINQ. Would need full rewrite.
- DataBindingHelper.cs, AllocCounter.cs flagged as TecVooDoo Utilities candidates (not game logic).
Version bumped to 1.2.0.

**Session 2 (2026-04-09) -- 6 new modules from adammyhre HIGH-priority gists:**
Added 6 new modules from HIGH-priority adammyhre gist candidates:
- **Reactive/Observable.cs** -- Reactive property with ValueChanged event, implicit conversion, equality-checked Set().
- **Functional/Either.cs** -- Either\<TLeft,TRight\> (result monad) + Optional\<T\> (option monad). Match/Select/SelectMany, implicit operators.
- **Functional/Preconditions.cs** -- Guard clauses: CheckNotNull\<T\> (handles Unity Object null via TVU OrNull()), CheckState with format strings.
- **Effects/DamageOverTimeEffect.cs** -- IEffect\<T\> interface + IDamageable + DamageEffect (instant) + DamageOverTimeEffect (tick-based via TVU IntervalTimer).
- **Serialization/SerializableType.cs** -- Serialize System.Type in Inspector. TypeFilterAttribute for dropdown filtering. TypeExtensions.InheritsOrImplements(). Editor drawer wrapped in #if UNITY_EDITOR.
- **Collections/PriorityQueue.cs** -- Generic PriorityQueue\<TElement,TPriority\> backed by SortedList with per-priority Queue buckets. Enqueue/Dequeue/Peek/Clear.
All adapted: var removed, namespace TecVooDoo.Games, headers with attribution. ObservableList not found in adammyhre gists (only Observable exists). Version bumped to 1.3.0.

---

## Active TODO

| Task | Priority | Notes |
|------|----------|-------|
| Review DataBindingHelper.cs for TVU | Medium | UI Toolkit data binding helper -- Utilities candidate |
| Review AllocCounter.cs for TVU | Low | GC allocation profiler -- Utilities candidate |

---

## Candidate Backlog

Gameplay scripts identified as candidates but not yet integrated:

| Candidate | Source | Notes |
|-----------|--------|-------|
| BulletHoleSpawner.cs | adammyhre gist #9 | DONE -- integrated Runtime/Pooling/ |
| CharacterStateMachine.cs | adammyhre gist #7 | DONE -- integrated Runtime/StateMachine/ |
| Processor.cs | adammyhre gist #6 | DONE -- integrated Runtime/Processing/ as ProcessorChain |
| AOETargeting.cs | adammyhre gist #10 | REJECTED -- too coupled to adammyhre's framework |
| Observable.cs | adammyhre gist #54 | DONE -- integrated Runtime/Reactive/ (Session 2) |
| ObservableList.cs | adammyhre gist #53 | DROPPED -- gist does not exist in adammyhre's repos |
| DamageOverTimeEffect.cs | adammyhre gist #11 | DONE -- integrated Runtime/Effects/ with IEffect\<T\>, IDamageable (Session 2) |
| PriorityQueue.cs | adammyhre gist #46 | DONE -- integrated Runtime/Collections/ as generic variant (Session 2) |
| SerializableType.cs | adammyhre gist #51 | DONE -- integrated Runtime/Serialization/ with editor drawer (Session 2) |
| Either.cs | adammyhre gist #30 | DONE -- integrated Runtime/Functional/ with Optional\<T\> (Session 2) |
| Preconditions.cs | adammyhre gist #55 | DONE -- integrated Runtime/Functional/ using TVU OrNull() (Session 2) |
| **CullingManager.cs** | adammyhre gist #13 | HIGH -- CullingGroup API wrapper. Performance for 3D projects. |
| **Signal.cs** | adammyhre gist #31 | HIGH -- event bus. Compare against existing GameEvent system first. |
| Targeting.cs (Combinator) | adammyhre gist #14 | MEDIUM -- composable targeting foundation. More architectural than AOE. |
| PushDownAutomata.cs | adammyhre gist #34 | MEDIUM -- stack-based FSM. Menus, dialogue, game states. |
| AbilityData.cs | adammyhre gist #18 | MEDIUM -- modular ability effects via SO. Fits vanilla SO arch. |
| DamageCalculator.cs | adammyhre gist #32 | MEDIUM -- expression trees for damage formulas. Data-driven. |
| GridSystem2D.cs + GridObject.cs | adammyhre gist #59/#58 | MEDIUM -- generic grid. Word game is grid-based. |
| DataBindingHelper.cs | adammyhre gist #1 | Redirected to TVU (Utilities) |
| AllocCounter.cs | adammyhre gist #8 | Redirected to TVU (Utilities) |
| HierarchyIconDrawer.cs | adammyhre gist #33 | Redirected to TVU (Utilities) |
| Flare Engine Chronos rewind | ENTRY-298 | Time manipulation mechanic -- needs extraction study |
| Flare Engine Reaction Profile | ENTRY-298 | Game-feel system -- needs extraction study |
| 2D Art Maker SpineCharacterCustomizer | ENTRY-300 | PartsManager pattern -- needs Spine dependency review |

**Full gist catalog:** 60 gists at `https://gist.github.com/adammyhre` (6 pages). 12 HIGH, ~19 MEDIUM, ~20 LOW. LOW items are editor tools, rendering demos, or config files.

**Note:** ImprovedTimers.cs (gist #48) already absorbed into TecVooDoo Utilities -- do not duplicate.

---

## Add Process

1. Review candidate script in full -- fetch gist or read source
2. Write/adapt in Sandbox2D scratch area (`Assets/_Scratch/GamesDev/`)
3. Confirm dependencies (TecVooDoo.Utilities allowed; no game-specific assets)
4. Copy to correct module in `Runtime/` (or create new module subfolder)
5. Bump `package.json`: patch (x.x.1) for additions, minor (x.1.0) for new modules
6. Update `TVG_Reference.md` module table
7. Update this doc's Contents table and Sessions entry

---

## Rule: Games vs Utilities

- **TecVooDoo.Games** -- gameplay systems with game logic (targeting, pooling effects, ability frameworks, state machines)
- **TecVooDoo.Utilities** -- engine/C# utilities with no game logic (extensions, timers, patterns, logging)

When in doubt: if it would feel wrong in a Unity editor tool, it belongs here.

---

## Session Close Checklist

- [ ] Update Sessions with summary of changes
- [ ] Update Contents table if modules changed
- [ ] Bump version in package.json
- [ ] Update TVG_Reference.md if APIs added or changed
- [ ] Move promoted candidates from backlog to Contents

---

**End of Status**
