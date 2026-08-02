// TecVooDoo Games - Tests
// Copyright (c) 2026 TecVooDoo LLC. All rights reserved.

using System;
using NUnit.Framework;

namespace TecVooDoo.Games.Tests
{
    [TestFixture]
    public class EitherTests
    {
        [Test]
        public void FromLeft_IsLeft()
        {
            Either<string, int> either = Either<string, int>.FromLeft("error");
            Assert.That(either.IsLeft, Is.True);
            Assert.That(either.IsRight, Is.False);
            Assert.That(either.Left, Is.EqualTo("error"));
        }

        [Test]
        public void FromRight_IsRight()
        {
            Either<string, int> either = Either<string, int>.FromRight(5);
            Assert.That(either.IsRight, Is.True);
            Assert.That(either.IsLeft, Is.False);
            Assert.That(either.Right, Is.EqualTo(5));
        }

        [Test]
        public void Left_OnRight_Throws()
        {
            Either<string, int> either = Either<string, int>.FromRight(5);
            Assert.Throws<InvalidOperationException>(() => { string unused = either.Left; });
        }

        [Test]
        public void Right_OnLeft_Throws()
        {
            Either<string, int> either = Either<string, int>.FromLeft("error");
            Assert.Throws<InvalidOperationException>(() => { int unused = either.Right; });
        }

        [Test]
        public void Match_SelectsCorrectBranch()
        {
            Either<string, int> left = Either<string, int>.FromLeft("bad");
            Either<string, int> right = Either<string, int>.FromRight(3);

            Assert.That(left.Match(l => "L:" + l, r => "R:" + r), Is.EqualTo("L:bad"));
            Assert.That(right.Match(l => "L:" + l, r => "R:" + r), Is.EqualTo("R:3"));
        }

        // Select maps the RIGHT side only -- Left is the short-circuit channel.
        [Test]
        public void Select_MapsRight()
        {
            Either<string, int> either = Either<string, int>.FromRight(4);
            Either<string, int> mapped = either.Select(value => value * 2);
            Assert.That(mapped.IsRight, Is.True);
            Assert.That(mapped.Right, Is.EqualTo(8));
        }

        [Test]
        public void Select_PropagatesLeftUnchanged()
        {
            Either<string, int> either = Either<string, int>.FromLeft("failure");
            Either<string, int> mapped = either.Select(value => value * 2);
            Assert.That(mapped.IsLeft, Is.True);
            Assert.That(mapped.Left, Is.EqualTo("failure"));
        }

        [Test]
        public void SelectMany_ChainsRight()
        {
            Either<string, int> either = Either<string, int>.FromRight(4);
            Either<string, int> bound = either.SelectMany(value => Either<string, int>.FromRight(value + 1));
            Assert.That(bound.IsRight, Is.True);
            Assert.That(bound.Right, Is.EqualTo(5));
        }

        [Test]
        public void SelectMany_CanShortCircuitToLeft()
        {
            Either<string, int> either = Either<string, int>.FromRight(4);
            Either<string, int> bound = either.SelectMany(value => Either<string, int>.FromLeft("rejected"));
            Assert.That(bound.IsLeft, Is.True);
            Assert.That(bound.Left, Is.EqualTo("rejected"));
        }

        [Test]
        public void SelectMany_OnLeft_DoesNotInvokeBinder()
        {
            Either<string, int> either = Either<string, int>.FromLeft("failure");
            bool invoked = false;
            Either<string, int> bound = either.SelectMany(value =>
            {
                invoked = true;
                return Either<string, int>.FromRight(value);
            });

            Assert.That(invoked, Is.False);
            Assert.That(bound.IsLeft, Is.True);
        }

        [Test]
        public void ToString_LabelsSide()
        {
            Assert.That(Either<string, int>.FromLeft("x").ToString(), Is.EqualTo("Left(x)"));
            Assert.That(Either<string, int>.FromRight(2).ToString(), Is.EqualTo("Right(2)"));
        }
    }

    [TestFixture]
    public class OptionalTests
    {
        [Test]
        public void Some_HasValue()
        {
            Optional<int> optional = Optional<int>.Some(7);
            Assert.That(optional.HasValue, Is.True);
            Assert.That(optional.Value, Is.EqualTo(7));
        }

        [Test]
        public void None_HasNoValue()
        {
            Optional<int> optional = Optional<int>.None();
            Assert.That(optional.HasValue, Is.False);
        }

        [Test]
        public void NoValue_StaticIsEmpty()
        {
            Assert.That(Optional<int>.NoValue.HasValue, Is.False);
        }

        [Test]
        public void Value_OnNone_Throws()
        {
            Optional<int> optional = Optional<int>.None();
            Assert.Throws<InvalidOperationException>(() => { int unused = optional.Value; });
        }

        [Test]
        public void GetValueOrDefault_ReturnsFallbackWhenEmpty()
        {
            Optional<int> optional = Optional<int>.None();
            Assert.That(optional.GetValueOrDefault(), Is.EqualTo(0));
            Assert.That(optional.GetValueOrDefault(99), Is.EqualTo(99));
        }

        [Test]
        public void GetValueOrDefault_ReturnsValueWhenPresent()
        {
            Optional<int> optional = Optional<int>.Some(3);
            Assert.That(optional.GetValueOrDefault(99), Is.EqualTo(3));
        }

        [Test]
        public void Match_SelectsCorrectBranch()
        {
            Assert.That(Optional<int>.Some(2).Match(v => "some" + v, () => "none"), Is.EqualTo("some2"));
            Assert.That(Optional<int>.None().Match(v => "some" + v, () => "none"), Is.EqualTo("none"));
        }

        [Test]
        public void Select_MapsValue()
        {
            Optional<int> mapped = Optional<int>.Some(4).Select(v => v * 3);
            Assert.That(mapped.HasValue, Is.True);
            Assert.That(mapped.Value, Is.EqualTo(12));
        }

        [Test]
        public void Select_OnNone_StaysNone()
        {
            Optional<int> mapped = Optional<int>.None().Select(v => v * 3);
            Assert.That(mapped.HasValue, Is.False);
        }

        [Test]
        public void SelectMany_ChainsValue()
        {
            Optional<int> bound = Optional<int>.Some(4).SelectMany(v => Optional<int>.Some(v + 1));
            Assert.That(bound.HasValue, Is.True);
            Assert.That(bound.Value, Is.EqualTo(5));
        }

        [Test]
        public void SelectMany_OnNone_DoesNotInvokeBinder()
        {
            bool invoked = false;
            Optional<int> bound = Optional<int>.None().SelectMany(v =>
            {
                invoked = true;
                return Optional<int>.Some(v);
            });

            Assert.That(invoked, Is.False);
            Assert.That(bound.HasValue, Is.False);
        }

        [Test]
        public void Equals_ComparesByPresenceAndValue()
        {
            Assert.That(Optional<int>.Some(1), Is.EqualTo(Optional<int>.Some(1)));
            Assert.That(Optional<int>.Some(1), Is.Not.EqualTo(Optional<int>.Some(2)));
            Assert.That(Optional<int>.None(), Is.EqualTo(Optional<int>.None()));
            Assert.That(Optional<int>.Some(1), Is.Not.EqualTo(Optional<int>.None()));
        }

        // Optional<T> converts to bool by presence, so it reads naturally in an if.
        [Test]
        public void ImplicitBool_ReflectsPresence()
        {
            Assert.That((bool)Optional<int>.Some(1), Is.True);
            Assert.That((bool)Optional<int>.None(), Is.False);
        }

        [Test]
        public void ExplicitCast_YieldsValue()
        {
            Assert.That((int)Optional<int>.Some(8), Is.EqualTo(8));
        }

        [Test]
        public void ToString_LabelsPresence()
        {
            Assert.That(Optional<int>.Some(5).ToString(), Is.EqualTo("Some(5)"));
            Assert.That(Optional<int>.None().ToString(), Is.EqualTo("None"));
        }
    }
}
