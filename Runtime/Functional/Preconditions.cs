// TecVooDoo Games
// Copyright (c) 2026 TecVooDoo LLC. All rights reserved.
// Based on Preconditions by Adam Myhre (adammyhre)

using System;
using TecVooDoo.Utilities;

namespace TecVooDoo.Games
{
    public static class Preconditions
    {
        public static T CheckNotNull<T>(T reference)
        {
            return CheckNotNull(reference, null);
        }

        public static T CheckNotNull<T>(T reference, string message)
        {
            if (reference is UnityEngine.Object obj && obj.OrNull() == null)
                throw new ArgumentNullException(message);
            if (reference is null)
                throw new ArgumentNullException(message);
            return reference;
        }

        public static void CheckState(bool expression)
        {
            CheckState(expression, null);
        }

        public static void CheckState(bool expression, string message)
        {
            if (expression) return;
            throw message == null ? new InvalidOperationException() : new InvalidOperationException(message);
        }

        public static void CheckState(bool expression, string messageTemplate, params object[] messageArgs)
        {
            CheckState(expression, string.Format(messageTemplate, messageArgs));
        }
    }
}
