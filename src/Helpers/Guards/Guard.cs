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
