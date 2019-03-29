namespace Helpers.Guards
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    using JetBrains.Annotations;

    /// <summary>
    /// Provides methods to protect against invalid parameters.
    /// </summary>
    [DebuggerStepThrough]
    [PublicAPI]
    public static class Guard
    {
        /// <summary>
        /// Verifies that the specified value is greater than or equal to a minimum value and less than
        /// or equal to a maximum value and throws an exception if it is not.
        /// </summary>
        /// <param name="value">The target value, which should be validated.</param>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        /// <param name="parameterName">The name of the parameter that is to be checked.</param>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is less than the minimum value of greater than the maximum value.</exception>
        [MethodImpl(InliningOptions.ShortMethod)]
        public static void MustBeBetweenOrEqualTo<TValue>(TValue value, TValue min, TValue max, string parameterName)
            where TValue : IComparable<TValue>
        {
            if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0)
            {
                ThrowArgumentOutOfRangeException(
                    parameterName,
                    $"Value {value} must be greater than or equal to {min} and less than or equal to {max}.");
            }
        }

        /// <summary>
        /// Verifies that the specified <paramref name="value1"/> is equal to <paramref name="value2"/> and throws an exception if it is not.
        /// </summary>
        /// <param name="value1">Fist value.</param>
        /// <param name="value2">Second value.</param>
        /// <param name="parameterName">The name of the parameter that is to be checked.</param>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value1"/> is not equal to <paramref name="value2"/>.</exception>
        [MethodImpl(InliningOptions.ShortMethod)]
        public static void MustBeEqualTo<TValue>(TValue value1, TValue value2, string parameterName)
            where TValue : IComparable<TValue>
        {
            if (value1.CompareTo(value2) != 0)
                ThrowArgumentOutOfRangeException(parameterName, $"Value {value1} must be equal to {value2}.");
        }

        /// <summary>
        /// Verifies, that the method parameter with specified target value is true
        /// and throws an exception if it is found to be so.
        /// </summary>
        /// <param name="target">The target value, which cannot be false.</param>
        /// <param name="parameterName">The name of the parameter that is to be checked.</param>
        /// <param name="message">The error message, if any to add to the exception.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="target"/> is false.</exception>
        [MethodImpl(InliningOptions.ShortMethod)]
        public static void IsTrue(bool target, string parameterName, string message)
        {
            if (!target)
                ThrowArgumentException(message, parameterName);
        }

        /// <summary>
        /// Verifies, that the method parameter with specified target value is false
        /// and throws an exception if it is found to be so.
        /// </summary>
        /// <param name="target">The target value, which cannot be true.</param>
        /// <param name="parameterName">The name of the parameter that is to be checked.</param>
        /// <param name="message">The error message, if any to add to the exception.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="target"/> is true.</exception>
        [MethodImpl(InliningOptions.ShortMethod)]
        public static void IsFalse(bool target, string parameterName, string message)
        {
            if (target)
                ThrowArgumentException(message, parameterName);
        }

        /// <summary>
        /// Ensures that the <paramref name="target"/> is not <c>Guid.Empty</c>.
        /// </summary>
        /// <param name="target">The target value, which cannot <c>Guid.Empty</c>.</param>
        /// <param name="parameterName">The name of the parameter that is to be checked.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="target"/> is <c>Guid.Empty</c>.</exception>
        [MethodImpl(InliningOptions.ColdPath)]
        [ContractAnnotation("=> value:notnull")]
        public static void NotEmpty(Guid target, string parameterName)
        {
            if (target == Guid.Empty)
                ThrowArgumentException("Guid should not be Guid.Empty.", parameterName);
        }

        [MethodImpl(InliningOptions.ColdPath)]
        private static void ThrowArgumentException(string message, string parameterName)
        {
            throw new ArgumentException(message, parameterName);
        }

        [MethodImpl(InliningOptions.ColdPath)]
        private static void ThrowArgumentOutOfRangeException(string parameterName, string message)
        {
            throw new ArgumentOutOfRangeException(parameterName, message);
        }

        [MethodImpl(InliningOptions.ColdPath)]
        private static void ThrowArgumentNullException(string parameterName)
        {
            throw new ArgumentNullException(parameterName);
        }
    }
}
