// TecVooDoo Games - Tests
// Copyright (c) 2026 TecVooDoo LLC. All rights reserved.

using System.Collections.Generic;
using NUnit.Framework;

namespace TecVooDoo.Games.Tests
{
    [TestFixture]
    public class CharacterStateMachineTests
    {
        // One shared log so tests can assert cross-state ordering (Exit before Enter).
        readonly List<string> log = new List<string>();

        abstract class LoggingState<TState> : CharacterState<TState> where TState : LoggingState<TState>
        {
            readonly List<string> log;
            readonly string name;

            public float LastDt { get; private set; }
            public int Ticks { get; private set; }

            protected LoggingState(List<string> log, string name)
            {
                this.log = log;
                this.name = name;
            }

            protected override void OnEnter() => log.Add(name + ".Enter");
            protected override void OnExit() => log.Add(name + ".Exit");

            protected override void OnTick(float dt)
            {
                Ticks++;
                LastDt = dt;
                log.Add(name + ".Tick");
            }
        }

        sealed class IdleState : LoggingState<IdleState>
        {
            public IdleState(List<string> log) : base(log, "Idle") { }
        }

        sealed class RunState : LoggingState<RunState>
        {
            public RunState(List<string> log) : base(log, "Run") { }
        }

        sealed class JumpState : LoggingState<JumpState>
        {
            public JumpState(List<string> log) : base(log, "Jump") { }
        }

        [SetUp]
        public void SetUp()
        {
            log.Clear();
        }

        [Test]
        public void Tick_WithNoState_DoesNothing()
        {
            CharacterStateMachine machine = new CharacterStateMachine();

            Assert.DoesNotThrow(() => machine.Tick(0.1f));
            Assert.That(log, Is.Empty);
        }

        [Test]
        public void ChangeState_First_EntersWithoutExit()
        {
            CharacterStateMachine machine = new CharacterStateMachine();

            machine.ChangeState(new IdleState(log));

            Assert.That(log, Is.EqualTo(new[] { "Idle.Enter" }));
        }

        [Test]
        public void ChangeState_ExitsOldBeforeEnteringNew()
        {
            CharacterStateMachine machine = new CharacterStateMachine();
            machine.ChangeState(new IdleState(log));

            machine.ChangeState(new RunState(log));

            Assert.That(log, Is.EqualTo(new[] { "Idle.Enter", "Idle.Exit", "Run.Enter" }));
        }

        [Test]
        public void Tick_NoTransition_TicksCurrentWithDelta()
        {
            CharacterStateMachine machine = new CharacterStateMachine();
            IdleState idle = new IdleState(log);
            machine.ChangeState(idle);

            machine.Tick(0.25f);
            machine.Tick(0.5f);

            Assert.That(idle.Ticks, Is.EqualTo(2));
            Assert.That(idle.LastDt, Is.EqualTo(0.5f));
        }

        // A frame that takes a transition changes state and ticks NEITHER state.
        [Test]
        public void Tick_TransitionFires_ChangesStateWithoutTicking()
        {
            CharacterStateMachine machine = new CharacterStateMachine();
            IdleState idle = new IdleState(log);
            RunState run = new RunState(log);
            idle.SetTransition(new Transition<RunState>(run, () => true));
            machine.ChangeState(idle);
            log.Clear();

            machine.Tick(0.1f);

            Assert.That(log, Is.EqualTo(new[] { "Idle.Exit", "Run.Enter" }));
            Assert.That(idle.Ticks, Is.EqualTo(0));
            Assert.That(run.Ticks, Is.EqualTo(0));

            machine.Tick(0.1f);
            Assert.That(run.Ticks, Is.EqualTo(1));
        }

        [Test]
        public void Tick_ConditionIsReEvaluatedEachFrame()
        {
            CharacterStateMachine machine = new CharacterStateMachine();
            IdleState idle = new IdleState(log);
            RunState run = new RunState(log);
            bool go = false;
            idle.SetTransition(new Transition<RunState>(run, () => go));
            machine.ChangeState(idle);

            machine.Tick(0.1f);
            Assert.That(idle.Ticks, Is.EqualTo(1));

            go = true;
            machine.Tick(0.1f);
            Assert.That(log[log.Count - 1], Is.EqualTo("Run.Enter"));
        }

        [Test]
        public void GetTransition_FirstRegisteredMatchWins()
        {
            IdleState idle = new IdleState(log);
            Transition<RunState> toRun = new Transition<RunState>(new RunState(log), () => true);
            Transition<JumpState> toJump = new Transition<JumpState>(new JumpState(log), () => true);
            idle.SetTransition(toRun);
            idle.SetTransition(toJump);

            Assert.That(idle.GetTransition(), Is.SameAs(toRun));
        }

        [Test]
        public void GetTransition_SkipsFalseConditions()
        {
            IdleState idle = new IdleState(log);
            Transition<RunState> toRun = new Transition<RunState>(new RunState(log), () => false);
            Transition<JumpState> toJump = new Transition<JumpState>(new JumpState(log), () => true);
            idle.SetTransition(toRun);
            idle.SetTransition(toJump);

            Assert.That(idle.GetTransition(), Is.SameAs(toJump));
        }

        [Test]
        public void GetTransition_NoneMatch_ReturnsNull()
        {
            IdleState idle = new IdleState(log);
            idle.SetTransition(new Transition<RunState>(new RunState(log), () => false));

            Assert.That(idle.GetTransition(), Is.Null);
        }

        // Re-entering the current state is not short-circuited: it exits and re-enters.
        [Test]
        public void ChangeState_ToSameInstance_ExitsThenReEnters()
        {
            CharacterStateMachine machine = new CharacterStateMachine();
            IdleState idle = new IdleState(log);
            machine.ChangeState(idle);

            machine.ChangeState(idle);

            Assert.That(log, Is.EqualTo(new[] { "Idle.Enter", "Idle.Exit", "Idle.Enter" }));
        }
    }
}
