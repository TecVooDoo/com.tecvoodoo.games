# TecVooDoo Games - Reference

**Package:** `com.tecvoodoo.games` v1.2.0
**Namespace:** `TecVooDoo.Games`
**Source:** `E:\Unity\DefaultUnityPackages\com.tecvoodoo.games\`
**Depends on:** `com.tecvoodoo.utilities`
**Last Updated:** March 16, 2026

---

## Quick Reference

| Module | What It Gives You |
|--------|-------------------|
| `Simulation/` | SimpleBoids flocking simulation |
| `Pooling/` | BulletHoleSpawner DecalProjector pooling |
| `StateMachine/` | CRTP state machine with transitions |
| `Processing/` | Generic processor chains (fluent builder) |

---

## Simulation

### SimpleBoids

Lightweight flocking simulation. Add to a parent GameObject to manage a flock of boid prefabs. Works for birds, fish, butterflies, or any swarming ambient entity.

Configure all behavior via Inspector. No runtime API beyond Inspector fields. Enable `showDebugGizmos` to visualize flock centers and danger zones in Scene view.

Key parameters:

| Field | Purpose |
|-------|---------|
| `boidCount` | Total boid instances across all flocks |
| `boidSpeed` | Base forward speed |
| `flockCount` | Number of independent sub-flocks |
| `migrationChance` | Chance a boid migrates to a different flock each interval |
| `enableDanger` | Whether boids flee objects on `dangerLayer` |
| `dangerRadius` | Physics overlap radius for danger detection |
| `scaleRange` | Random scale variation per boid (x=min, y=max) |

---

## Pooling

### BulletHoleSpawner

DecalProjector object pooling system. Manages a pool of URP DecalProjectors for bullet impacts or similar surface marks. Based on adammyhre gist.

---

## StateMachine

### CharacterStateMachine

CRTP (Curiously Recurring Template Pattern) state machine. Based on adammyhre gist.

**Key types:**

| Type | Purpose |
|------|---------|
| `CharacterStateMachine` | Drives current state, evaluates transitions, calls `Tick()` |
| `CharacterState<TState>` | Base for concrete states. Override `OnEnter()`, `OnExit()`, `OnTick(float dt)` |
| `Transition<TState>` | Condition-based transition to a target state |

**Usage:**

```csharp
// Define states
public class IdleState : CharacterState<IdleState>
{
    protected override void OnEnter() { /* ... */ }
    protected override void OnExit() { /* ... */ }
    protected override void OnTick(float dt) { /* ... */ }
}

public class MoveState : CharacterState<MoveState>
{
    protected override void OnEnter() { /* ... */ }
    protected override void OnExit() { /* ... */ }
    protected override void OnTick(float dt) { /* ... */ }
}

// Wire up
CharacterStateMachine sm = new CharacterStateMachine();
IdleState idle = new IdleState();
MoveState move = new MoveState();

idle.SetTransition(new Transition<MoveState>(move, () => isMoving));
move.SetTransition(new Transition<IdleState>(idle, () => !isMoving));

sm.ChangeState(idle);

// Each frame
sm.Tick(Time.deltaTime);
```

---

## Processing

### ProcessorChain

Generic processing chain with fluent builder API. Chain `IProcessor<TIn, TOut>` steps together for scoring, filtering, or transformation pipelines. Based on adammyhre gist.

**Key types:**

| Type | Purpose |
|------|---------|
| `IProcessor<TIn, TOut>` | Single processing step interface |
| `ProcessorDelegate<TIn, TOut>` | Delegate form for lightweight processors |
| `CombinedProcessor<A, B, C>` | Combines two processors sequentially |
| `ProcessorChain<TIn, TOut>` | Fluent chain builder with `.Start()`, `.Then()`, `.Run()`, `.Compile()` |

**Usage:**

```csharp
// Define processors
public class DistanceScorer : IProcessor<float, float>
{
    public float Process(float distance) { return 1f / (1f + distance); }
}

public class ThresholdFilter : IProcessor<float, bool>
{
    readonly float threshold;
    public ThresholdFilter(float threshold) { this.threshold = threshold; }
    public bool Process(float score) { return score >= threshold; }
}

// Build chain
ProcessorChain<float, bool> chain = ProcessorChain<float, float>
    .Start(new DistanceScorer())
    .Then(new ThresholdFilter(0.5f));

bool isClose = chain.Run(3.0f);

// Or compile to delegate for hot paths
ProcessorDelegate<float, bool> compiled = chain.Compile();
bool result = compiled(3.0f);
```

---

**End of Reference**
