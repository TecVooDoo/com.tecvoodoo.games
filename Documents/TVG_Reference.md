# TecVooDoo Games - Reference

**Package:** `com.tecvoodoo.games` v1.3.0
**Namespace:** `TecVooDoo.Games`
**Source:** `E:\Unity\DefaultUnityPackages\com.tecvoodoo.games\`
**Depends on:** `com.tecvoodoo.utilities`
**Last Updated:** April 9, 2026

---

## Quick Reference

| Module | What It Gives You |
|--------|-------------------|
| `Simulation/` | SimpleBoids flocking simulation |
| `Pooling/` | BulletHoleSpawner DecalProjector pooling |
| `StateMachine/` | CRTP state machine with transitions |
| `Processing/` | Generic processor chains (fluent builder) |
| `Reactive/` | Observable\<T\> reactive property with change events |
| `Functional/` | Either\<L,R\> result monad, Optional\<T\>, Preconditions guard clauses |
| `Effects/` | IEffect\<T\> interface, DamageEffect, DamageOverTimeEffect (uses TVU IntervalTimer) |
| `Serialization/` | SerializableType (System.Type in Inspector), TypeFilterAttribute |
| `Collections/` | PriorityQueue\<TElement,TPriority\> generic priority queue |

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

## Reactive

### Observable\<T\>

Reactive property that fires `ValueChanged` when the value changes. Equality-checked to avoid redundant events.

```csharp
using TecVooDoo.Games;

Observable<int> health = new Observable<int>(100, newVal => Debug.Log($"Health: {newVal}"));

health.Value = 80;          // fires ValueChanged
health.Set(80);             // no event (same value)
health.AddListener(h => UpdateUI(h));
health.RemoveListener(handler);

int raw = health;           // implicit conversion to T
health.Dispose();           // clears event + resets value
```

---

## Functional

### Either\<TLeft, TRight\>

Result/Error monad. Left = error/failure, Right = success/value.

```csharp
using TecVooDoo.Games;

Either<string, int> result = Either<string, int>.FromRight(42);

string msg = result.Match(
    error => $"Failed: {error}",
    value => $"Got: {value}"
);

// Implicit conversion
Either<string, int> ok = 42;          // Right
Either<string, int> err = "bad";      // Left

// Chaining
Either<string, float> mapped = result.Select(v => v * 1.5f);
```

### Optional\<T\>

Option monad. Avoids null checks.

```csharp
using TecVooDoo.Games;

Optional<string> name = Optional<string>.Some("Kharon");
Optional<string> empty = Optional<string>.None();

string display = name.Match(n => n, () => "Unknown");

if (name) { /* HasValue == true */ }
string val = name.Value;              // throws if None

Optional<int> len = name.Select(n => n.Length);
```

### Preconditions

Guard clauses for defensive programming. Handles Unity Object null correctly via TecVooDoo.Utilities `OrNull()`.

```csharp
using TecVooDoo.Games;

Preconditions.CheckNotNull(gameObject, "Target GO is required");
Preconditions.CheckState(health > 0, "Health must be positive, got {0}", health);
```

---

## Effects

### IEffect\<TTarget\>

Generic effect interface for applying timed/instant effects to targets.

```csharp
using TecVooDoo.Games;

// Instant damage
DamageEffect hit = new DamageEffect { damageAmount = 25 };
hit.OnCompleted += effect => Debug.Log("Hit applied");
hit.Apply(target);

// Damage over time (uses TecVooDoo.Utilities IntervalTimer)
DamageOverTimeEffect dot = new DamageOverTimeEffect
{
    duration = 5f,
    tickInterval = 1f,
    damagePerTick = 10
};
dot.OnCompleted += effect => Debug.Log("DOT finished");
dot.Apply(target);   // ticks 5 times over 5 seconds
dot.Cancel();        // early cancellation
```

**Key types:**

| Type | Purpose |
|------|---------|
| `IEffect<TTarget>` | Interface: Apply, Cancel, OnCompleted event |
| `IDamageable` | Interface: TakeDamage(int) |
| `DamageEffect` | Instant single-hit damage |
| `DamageOverTimeEffect` | Tick-based damage using IntervalTimer |

---

## Serialization

### SerializableType

Serialize `System.Type` references in the Inspector. Includes a property drawer with type dropdown.

```csharp
using TecVooDoo.Games;

// Basic usage -- shows all concrete types in dropdown
public SerializableType effectType;

// Filtered -- only types implementing IEffect<IDamageable>
[TypeFilter(typeof(IEffect<IDamageable>))]
public SerializableType effectType;

// Runtime
Type t = effectType;                    // implicit conversion
IEffect<IDamageable> effect = (IEffect<IDamageable>)Activator.CreateInstance(t);
```

### TypeExtensions

```csharp
bool isEffect = myType.InheritsOrImplements(typeof(IEffect<>));
```

---

## Collections

### PriorityQueue\<TElement, TPriority\>

Generic priority queue. Elements dequeued in priority order (lowest first). Multiple elements at the same priority are FIFO.

```csharp
using TecVooDoo.Games;

PriorityQueue<string, int> queue = new PriorityQueue<string, int>();
queue.Enqueue("low", 10);
queue.Enqueue("high", 1);
queue.Enqueue("medium", 5);

string first = queue.Dequeue();    // "high" (priority 1)
string next = queue.Peek();        // "medium" (priority 5, not removed)

bool empty = queue.IsEmpty;
queue.Clear();
```

---

**End of Reference**
