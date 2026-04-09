// TecVooDoo Games
// Copyright (c) 2026 TecVooDoo LLC. All rights reserved.
// Based on DamageOverTimeEffect by Adam Myhre (adammyhre)

using System;
using TecVooDoo.Utilities;
using UnityEngine;

namespace TecVooDoo.Games
{
    /// <summary>
    /// Generic effect interface. Apply an effect to a target, cancel it, and listen for completion.
    /// </summary>
    public interface IEffect<TTarget>
    {
        void Apply(TTarget target);
        void Cancel();
        event Action<IEffect<TTarget>> OnCompleted;
    }

    /// <summary>
    /// Target that can receive numeric damage.
    /// </summary>
    public interface IDamageable
    {
        void TakeDamage(int amount);
    }

    /// <summary>
    /// Instant damage effect. Applies damage once and completes immediately.
    /// </summary>
    [Serializable]
    public class DamageEffect : IEffect<IDamageable>
    {
        public int damageAmount = 10;

        public event Action<IEffect<IDamageable>> OnCompleted;

        public void Apply(IDamageable target)
        {
            target.TakeDamage(damageAmount);
            OnCompleted?.Invoke(this);
        }

        public void Cancel()
        {
            OnCompleted?.Invoke(this);
        }
    }

    /// <summary>
    /// Damage over time effect. Applies damage at regular intervals for a duration.
    /// Uses TecVooDoo.Utilities IntervalTimer for tick scheduling.
    /// </summary>
    [Serializable]
    public class DamageOverTimeEffect : IEffect<IDamageable>
    {
        public float duration = 5f;
        public float tickInterval = 1f;
        public int damagePerTick;

        public event Action<IEffect<IDamageable>> OnCompleted;

        IntervalTimer timer;
        IDamageable currentTarget;

        public void Apply(IDamageable target)
        {
            currentTarget = target;
            timer = new IntervalTimer(duration, tickInterval);
            timer.OnInterval += OnInterval;
            timer.OnTimerStop += OnStop;
            timer.Start();
        }

        void OnInterval()
        {
            currentTarget?.TakeDamage(damagePerTick);
        }

        void OnStop()
        {
            Cleanup();
        }

        public void Cancel()
        {
            timer?.Stop();
            Cleanup();
        }

        void Cleanup()
        {
            if (timer != null)
            {
                timer.OnInterval -= OnInterval;
                timer.OnTimerStop -= OnStop;
                timer.Dispose();
                timer = null;
            }
            currentTarget = null;
            OnCompleted?.Invoke(this);
        }
    }
}
