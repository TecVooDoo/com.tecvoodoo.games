// TecVooDoo Games
// Copyright (c) 2026 TecVooDoo LLC. All rights reserved.
// Based on CRTP State Machine by Adam Myhre (adammyhre)

using System.Collections.Generic;

namespace TecVooDoo.Games
{
    /// <summary>
    /// Simple state machine that evaluates transitions and ticks the current state each frame.
    /// </summary>
    public class CharacterStateMachine
    {
        CharacterState current;

        public void ChangeState<TState>(TState newState) where TState : CharacterState<TState>
        {
            current?.Exit();
            current = newState;
            current.Enter();
        }

        public void Tick(float dt)
        {
            if (current == null) return;
            Transition transition = current.GetTransition();
            if (transition != null)
            {
                transition.Apply(this);
                return;
            }
            current.Tick(dt);
        }
    }

    /// <summary>
    /// Base state class with transition support. Derive from CharacterState{TState} instead.
    /// </summary>
    public abstract class CharacterState
    {
        readonly List<Transition> transitions = new List<Transition>();

        public Transition GetTransition()
        {
            for (int i = 0; i < transitions.Count; i++)
            {
                if (transitions[i].Evaluate()) return transitions[i];
            }
            return null;
        }

        public void SetTransition<TState>(Transition<TState> t) where TState : CharacterState<TState>
        {
            transitions.Add(t);
        }

        public abstract void Enter();
        public abstract void Exit();
        public abstract void Tick(float dt);
    }

    /// <summary>
    /// Typed state base using CRTP. Subclass this for concrete states.
    /// </summary>
    public abstract class CharacterState<TState> : CharacterState where TState : CharacterState<TState>
    {
        public override void Enter()
        {
            ((TState)this).OnEnter();
        }

        public override void Exit()
        {
            ((TState)this).OnExit();
        }

        public override void Tick(float dt)
        {
            ((TState)this).OnTick(dt);
        }

        protected abstract void OnEnter();
        protected abstract void OnExit();
        protected abstract void OnTick(float dt);
    }
}
