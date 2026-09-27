// TecVooDoo Games - Tests
// Copyright (c) 2026 TecVooDoo LLC. All rights reserved.

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace TecVooDoo.Games.Tests
{
    [TestFixture]
    public class DamageEffectTests
    {
        sealed class DamageRecorder : IDamageable
        {
            public int Total;
            public int Hits;

            public void TakeDamage(int amount)
            {
                Total += amount;
                Hits++;
            }
        }

        [Test]
        public void DamageEffect_Apply_DealsDamageOnceAndCompletes()
        {
            DamageEffect effect = new DamageEffect { damageAmount = 7 };
            DamageRecorder target = new DamageRecorder();
            int completed = 0;
            IEffect<IDamageable> completedWith = null;
            effect.OnCompleted += e => { completed++; completedWith = e; };

            effect.Apply(target);

            Assert.That(target.Total, Is.EqualTo(7));
            Assert.That(target.Hits, Is.EqualTo(1));
            Assert.That(completed, Is.EqualTo(1));
            Assert.That(completedWith, Is.SameAs(effect));
        }

        [Test]
        public void DamageEffect_Cancel_CompletesWithoutDamage()
        {
            DamageEffect effect = new DamageEffect();
            int completed = 0;
            effect.OnCompleted += _ => completed++;

            effect.Cancel();

            Assert.That(completed, Is.EqualTo(1));
        }

        [Test]
        public void DamageOverTime_CancelBeforeApply_CompletesOnce()
        {
            DamageOverTimeEffect effect = new DamageOverTimeEffect();
            int completed = 0;
            effect.OnCompleted += _ => completed++;

            effect.Cancel();

            Assert.That(completed, Is.EqualTo(1));
        }

        // Driven by the real TimerManager PlayerLoop hook; Time.captureDeltaTime makes
        // Time.deltaTime exact (powers of two) so tick thresholds land deterministically.
        [UnityTest]
        public IEnumerator DamageOverTime_RunsToCompletion_TicksOncePerInterval()
        {
            DamageOverTimeEffect effect = new DamageOverTimeEffect { duration = 1f, tickInterval = 0.25f, damagePerTick = 3 };
            DamageRecorder target = new DamageRecorder();
            int completed = 0;
            effect.OnCompleted += _ => completed++;

            Time.captureDeltaTime = 0.125f;
            try
            {
                effect.Apply(target);
                for (int i = 0; i < 20 && completed == 0; i++)
                    yield return null;
                for (int i = 0; i < 5; i++)
                    yield return null;
            }
            finally
            {
                Time.captureDeltaTime = 0f;
            }

            Assert.That(target.Hits, Is.EqualTo(4), "1s / 0.25s = 4 ticks (thresholds 0.75, 0.5, 0.25, 0)");
            Assert.That(target.Total, Is.EqualTo(12));
            Assert.That(completed, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator DamageOverTime_CancelMidway_StopsTicking()
        {
            DamageOverTimeEffect effect = new DamageOverTimeEffect { duration = 1f, tickInterval = 0.25f, damagePerTick = 1 };
            DamageRecorder target = new DamageRecorder();

            Time.captureDeltaTime = 0.125f;
            try
            {
                effect.Apply(target);
                for (int i = 0; i < 20 && target.Hits < 2; i++)
                    yield return null;

                int hitsAtCancel = target.Hits;
                effect.Cancel();
                for (int i = 0; i < 12; i++)
                    yield return null;

                Assert.That(hitsAtCancel, Is.EqualTo(2));
                Assert.That(target.Hits, Is.EqualTo(hitsAtCancel), "no ticks after Cancel");
            }
            finally
            {
                Time.captureDeltaTime = 0f;
            }
        }

        [UnityTest]
        public IEnumerator DamageOverTime_ApplyWhileRunning_IsIgnored()
        {
            DamageOverTimeEffect effect = new DamageOverTimeEffect { duration = 1f, tickInterval = 0.25f, damagePerTick = 1 };
            DamageRecorder first = new DamageRecorder();
            DamageRecorder second = new DamageRecorder();
            int completed = 0;
            effect.OnCompleted += _ => completed++;

            Time.captureDeltaTime = 0.125f;
            try
            {
                effect.Apply(first);
                yield return null;
                effect.Apply(second);
                for (int i = 0; i < 20 && completed == 0; i++)
                    yield return null;
                for (int i = 0; i < 5; i++)
                    yield return null;
            }
            finally
            {
                Time.captureDeltaTime = 0f;
            }

            Assert.That(first.Hits, Is.EqualTo(4), "the original run is unaffected");
            Assert.That(second.Hits, Is.EqualTo(0), "the second Apply was ignored");
            Assert.That(completed, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator DamageOverTime_ApplyAfterCompletion_RunsAgain()
        {
            DamageOverTimeEffect effect = new DamageOverTimeEffect { duration = 0.5f, tickInterval = 0.25f, damagePerTick = 1 };
            DamageRecorder target = new DamageRecorder();
            int completed = 0;
            effect.OnCompleted += _ => completed++;

            Time.captureDeltaTime = 0.125f;
            try
            {
                effect.Apply(target);
                for (int i = 0; i < 20 && completed == 0; i++)
                    yield return null;
                effect.Apply(target);
                for (int i = 0; i < 20 && completed == 1; i++)
                    yield return null;
            }
            finally
            {
                Time.captureDeltaTime = 0f;
            }

            Assert.That(target.Hits, Is.EqualTo(4), "2 ticks per run");
            Assert.That(completed, Is.EqualTo(2));
        }

        [UnityTest]
        public IEnumerator DamageOverTime_CancelMidway_CompletesOnce()
        {
            DamageOverTimeEffect effect = new DamageOverTimeEffect { duration = 1f, tickInterval = 0.25f, damagePerTick = 1 };
            DamageRecorder target = new DamageRecorder();
            int completed = 0;
            effect.OnCompleted += _ => completed++;

            Time.captureDeltaTime = 0.125f;
            try
            {
                effect.Apply(target);
                yield return null;
                yield return null;
                effect.Cancel();
                yield return null;
            }
            finally
            {
                Time.captureDeltaTime = 0f;
            }

            Assert.That(completed, Is.EqualTo(1));
        }
    }
}
