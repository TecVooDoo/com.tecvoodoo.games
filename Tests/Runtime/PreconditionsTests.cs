// TecVooDoo Games - Tests
// Copyright (c) 2026 TecVooDoo LLC. All rights reserved.

using System;
using NUnit.Framework;
using UnityEngine;

namespace TecVooDoo.Games.Tests
{
    [TestFixture]
    public class PreconditionsTests
    {
        [Test]
        public void CheckNotNull_ReturnsReference_WhenNotNull()
        {
            string value = "present";
            Assert.That(Preconditions.CheckNotNull(value), Is.SameAs(value));
        }

        [Test]
        public void CheckNotNull_Throws_WhenNull()
        {
            string value = null;
            Assert.Throws<ArgumentNullException>(() => Preconditions.CheckNotNull(value));
        }

        [Test]
        public void CheckNotNull_Throws_WithMessage()
        {
            string value = null;
            ArgumentNullException ex = Assert.Throws<ArgumentNullException>(
                () => Preconditions.CheckNotNull(value, "wired"));
            Assert.That(ex.ParamName, Is.EqualTo("wired"));
        }

        [Test]
        public void CheckNotNull_PassesValueType()
        {
            Assert.That(Preconditions.CheckNotNull(0), Is.EqualTo(0));
        }

        [Test]
        public void CheckNotNull_PassesLiveUnityObject()
        {
            ScriptableObject asset = ScriptableObject.CreateInstance<ScriptableObject>();
            try
            {
                Assert.That(Preconditions.CheckNotNull(asset), Is.SameAs(asset));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(asset);
            }
        }

        // The reason CheckNotNull special-cases UnityEngine.Object: a destroyed object
        // is not `null` by C# reference equality, but must still be rejected.
        [Test]
        public void CheckNotNull_Throws_OnDestroyedUnityObject()
        {
            ScriptableObject asset = ScriptableObject.CreateInstance<ScriptableObject>();
            UnityEngine.Object.DestroyImmediate(asset);

            Assert.Throws<ArgumentNullException>(() => Preconditions.CheckNotNull(asset));
        }

        [Test]
        public void CheckState_DoesNotThrow_WhenTrue()
        {
            Assert.DoesNotThrow(() => Preconditions.CheckState(true));
        }

        [Test]
        public void CheckState_Throws_WhenFalse()
        {
            Assert.Throws<InvalidOperationException>(() => Preconditions.CheckState(false));
        }

        [Test]
        public void CheckState_Throws_WithMessage()
        {
            InvalidOperationException ex = Assert.Throws<InvalidOperationException>(
                () => Preconditions.CheckState(false, "not ready"));
            Assert.That(ex.Message, Is.EqualTo("not ready"));
        }

        [Test]
        public void CheckState_FormatsMessageTemplate()
        {
            InvalidOperationException ex = Assert.Throws<InvalidOperationException>(
                () => Preconditions.CheckState(false, "expected {0} but was {1}", 3, 5));
            Assert.That(ex.Message, Is.EqualTo("expected 3 but was 5"));
        }

        [Test]
        public void CheckState_TemplateOverload_DoesNotThrowWhenTrue()
        {
            Assert.DoesNotThrow(() => Preconditions.CheckState(true, "unused {0}", 1));
        }
    }
}
