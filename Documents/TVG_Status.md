# TecVooDoo Games - Status

**Package:** `com.tecvoodoo.games` v1.2.0
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
| StateMachine | CharacterStateMachine, Transition | New -- CRTP state machine from adammyhre |
| Processing | ProcessorChain | New -- generic processor chains from adammyhre |

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
| **Observable.cs** | adammyhre gist #54 | HIGH -- reactive property. Fits vanilla SO arch. UI binding, stat tracking. |
| **ObservableList.cs** | adammyhre gist #53 | HIGH -- reactive collection. Inventory, quest lists, companion trains. |
| **DamageOverTimeEffect.cs** | adammyhre gist #11 | HIGH -- DOT/HOT/buff pattern. Beat-em-up, metroidvania, fishing. |
| **PriorityQueue.cs** | adammyhre gist #46 | HIGH -- double-key priority queue. AI decisions, pathfinding, event scheduling. |
| **SerializableType.cs** | adammyhre gist #51 | HIGH -- serialize System.Type in Inspector. Ability/effect/factory patterns. |
| **Either.cs** | adammyhre gist #30 | HIGH -- Result/Optional monad. Clean error handling for gameplay systems. |
| **Preconditions.cs** | adammyhre gist #55 | HIGH -- guard clause utilities. Defensive programming. |
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
