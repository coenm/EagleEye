namespace EagleEye.ExifToolWrapper.ExifTool
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;

    public static class TaskExtensions
    {
        /// <summary>
        /// See for more information:
        /// http://stackoverflow.com/questions/14524209/what-is-the-correct-way-to-cancel-an-async-operation-that-doesnt-accept-a-cance/14524565#14524565
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1618:GenericTypeParametersMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
        // ReSharper disable once AsyncConverter.AsyncMethodNamingHighlighting
        public static async Task<T> WithWaitCancellation<T>(this Task<T> task, CancellationToken ct)
        {
            var tcs = new TaskCompletionSource<bool>();

            // Register with the cancellation token.
            using (ct.Register(s => ((TaskCompletionSource<bool>)s).TrySetResult(true), tcs))
            {
                // If the task waited on is the cancellation token...
                if (task != await Task.WhenAny(task, tcs.Task).ConfigureAwait(false))
                    throw new OperationCanceledException(ct);
            }

            // Wait for one or the other to complete.
            return await task.ConfigureAwait(false);
        }

        // ReSharper disable once AsyncConverter.AsyncMethodNamingHighlighting
        public static async Task WithWaitCancellation(this Task task, CancellationToken ct)
        {
            var tcs = new TaskCompletionSource<bool>();

            // Register with the cancellation token.
            using (ct.Register(s => ((TaskCompletionSource<bool>)s).TrySetResult(true), tcs))
            {
                // If the task waited on is the cancellation token...
                if (task != await Task.WhenAny(task, tcs.Task).ConfigureAwait(false))
                    throw new OperationCanceledException(ct);
            }

            // Wait for one or the other to complete.
            await task.ConfigureAwait(false);
        }
    }
}