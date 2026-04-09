// TecVooDoo Games
// Copyright (c) 2026 TecVooDoo LLC. All rights reserved.
// Based on Either<TLeft, TRight> and Optional<T> by Adam Myhre (adammyhre)

using System;
using System.Collections.Generic;

namespace TecVooDoo.Games
{
    public struct Either<TLeft, TRight>
    {
        readonly TLeft left;
        readonly TRight right;
        readonly bool isRight;

        Either(TLeft left, TRight right, bool isRight)
        {
            this.left = left;
            this.right = right;
            this.isRight = isRight;
        }

        public bool IsLeft => !isRight;
        public bool IsRight => isRight;

        public TLeft Left
        {
            get
            {
                if (IsRight) throw new InvalidOperationException("No Left value present.");
                return left;
            }
        }

        public TRight Right
        {
            get
            {
                if (IsLeft) throw new InvalidOperationException("No Right value present.");
                return right;
            }
        }

        public static Either<TLeft, TRight> FromLeft(TLeft left) => new Either<TLeft, TRight>(left, default, false);
        public static Either<TLeft, TRight> FromRight(TRight right) => new Either<TLeft, TRight>(default, right, true);

        public TResult Match<TResult>(Func<TLeft, TResult> leftFunc, Func<TRight, TResult> rightFunc)
        {
            return IsRight ? rightFunc(right) : leftFunc(left);
        }

        public Either<TLeft, TResult> Select<TResult>(Func<TRight, TResult> map)
        {
            return IsRight
                ? Either<TLeft, TResult>.FromRight(map(right))
                : Either<TLeft, TResult>.FromLeft(left);
        }

        public Either<TLeft, TResult> SelectMany<TResult>(Func<TRight, Either<TLeft, TResult>> bind)
        {
            return IsRight ? bind(right) : Either<TLeft, TResult>.FromLeft(left);
        }

        public static implicit operator Either<TLeft, TRight>(TLeft left) => FromLeft(left);
        public static implicit operator Either<TLeft, TRight>(TRight right) => FromRight(right);

        public override string ToString() => IsRight ? $"Right({right})" : $"Left({left})";
    }

    public struct Optional<T>
    {
        public static readonly Optional<T> NoValue = new Optional<T>();

        readonly bool hasValue;
        readonly T value;

        public Optional(T value)
        {
            this.value = value;
            hasValue = true;
        }

        public T Value => hasValue ? value : throw new InvalidOperationException("No value");
        public bool HasValue => hasValue;

        public T GetValueOrDefault() => value;
        public T GetValueOrDefault(T defaultValue) => hasValue ? value : defaultValue;

        public TResult Match<TResult>(Func<T, TResult> onValue, Func<TResult> onNoValue)
        {
            return hasValue ? onValue(value) : onNoValue();
        }

        public Optional<TResult> SelectMany<TResult>(Func<T, Optional<TResult>> bind)
        {
            return hasValue ? bind(value) : Optional<TResult>.NoValue;
        }

        public Optional<TResult> Select<TResult>(Func<T, TResult> map)
        {
            return hasValue ? new Optional<TResult>(map(value)) : Optional<TResult>.NoValue;
        }

        public static Optional<T> Some(T value) => new Optional<T>(value);
        public static Optional<T> None() => NoValue;

        public override bool Equals(object obj) => obj is Optional<T> other && Equals(other);

        public bool Equals(Optional<T> other)
        {
            return !hasValue ? !other.hasValue : EqualityComparer<T>.Default.Equals(value, other.value);
        }

        public override int GetHashCode() => (hasValue.GetHashCode() * 397) ^ EqualityComparer<T>.Default.GetHashCode(value);

        public override string ToString() => hasValue ? $"Some({value})" : "None";

        public static implicit operator Optional<T>(T value) => new Optional<T>(value);
        public static implicit operator bool(Optional<T> value) => value.hasValue;
        public static explicit operator T(Optional<T> value) => value.Value;
    }
}
