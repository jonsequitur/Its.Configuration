// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Its.Recipes
{
    /// <summary>
    ///     Provides methods for evaluating and describing exceptions.
    /// </summary>
#if !RecipesProject
    [System.Diagnostics.DebuggerStepThrough]
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
#endif
    internal static class ExceptionHandlingExtensions
    {
        /// <summary>
        ///     Determines whether the exception has been handled.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns>
        ///     <c>true</c> if the exception has been handled; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasBeenHandled(this Exception exception)
        {
            return exception.Data.Contains("Handled") &&
                   Equals(exception.Data["Handled"], true);
        }

        /// <summary>
        ///     Returns all of the inner exceptions of an exception in a single sequence.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public static IEnumerable<Exception> InnerExceptions(this Exception exception)
        {
            if (exception == null)
            {
                yield break;
            }

            // aggregate exceptions require special treatment
            var aggregate = exception as AggregateException;
            if (aggregate != null)
            {
                foreach (var inner in aggregate.InnerExceptions)
                {
                    yield return inner;

                    foreach (var innerInner in inner.InnerExceptions())
                    {
                        yield return innerInner;
                    }
                }

                yield break;
            }

            // other exceptions are more straightforward
            var next = exception.InnerException;

            while (next != null)
            {
                yield return next;

                next = next.InnerException;
            }
        }

        /// <summary>
        ///     Checks if an exception is considered fatal, i.e. cannot/should not be handled by an application.
        /// </summary>
        /// <param name="exception">Exception instance</param>
        /// <returns>True if exception is considered fatal, or false otherwise</returns>
        public static bool IsFatal(this Exception exception)
        {
            return exception.IsItselfFatal() || exception.InnerExceptions().Any(IsItselfFatal);
        }

        /// <summary>
        ///     Marks the exception as having been handled.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public static TException MarkAsHandled<TException>(this TException exception)
            where TException : Exception
        {
            exception.Data["Handled"] = true;
            return exception;
        }

        private static bool IsItselfFatal(this Exception exception)
        {
            return (exception is ThreadAbortException ||
                    exception is AccessViolationException ||
                    (exception is OutOfMemoryException) && !(exception is InsufficientMemoryException));
        }
    }
}