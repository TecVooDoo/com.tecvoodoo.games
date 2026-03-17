// TecVooDoo Games
// Copyright (c) 2026 TecVooDoo LLC. All rights reserved.
// Based on CRTP State Machine by Adam Myhre (adammyhre)

using System;

namespace TecVooDoo.Games
{
    /// <summary>
    /// Base transition class. Evaluates a condition and applies a state change.
    /// </summary>
    public abstract class Transition
    {
        public abstract bool Evaluate();
        public abstract void Apply(CharacterStateMachine stateMachine);
    }

    /// <summary>
    /// Typed transition that changes to a specific state when the condition is met.
    /// </summary>
    public class Transition<TState> : Transition where TState : CharacterState<TState>
    {
        readonly TState targetState;
        readonly Func<bool> condition;

        public Transition(TState targetState, Func<bool> condition)
        {
            this.targetState = targetState;
            this.condition = condition;
        }

        public override bool Evaluate()
        {
            return condition();
        }

        public override void Apply(CharacterStateMachine stateMachine)
        {
            stateMachine.ChangeState(targetState);
        }
    }
}
