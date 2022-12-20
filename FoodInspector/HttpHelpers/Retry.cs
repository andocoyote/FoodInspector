using System.Net;

namespace HttpClientTest.HttpHelpers
{
    public static class Retry
    {
        private const int DefaultAttemptCount = 1;

        /// <summary>
        /// Run action until it succeeds or up to attemptCount times, waiting retryInterval between attempts (linear retry).
        /// </summary>
        /// <param name="action">Action to run.</param>
        /// <param name="retryInterval">duration to wait between retries</param>
        /// <param name="attemptCount">Maximum number of times to attempt to execute action.</param>
        public static async Task<HttpResponseMessage> Do(Func<Task<HttpResponseMessage>> action, TimeSpan retryInterval, int attemptCount = DefaultAttemptCount)
        {
            if (attemptCount < 1)
            {
                attemptCount = 1;
            }

            List<Exception> exceptions = new List<Exception>();

            for (int attempt = 1; attempt <= attemptCount; attempt++)
            {
                HttpResponseMessage response = null;
                try
                {
                    response = await action().ConfigureAwait(false);

                    return response;
                }
                catch (Exception ex)
                {
                    if (attemptCount > attempt && IsRetriable(response))
                    {
                        exceptions.Add(ex);
                    }
                    else
                    {
                        throw;
                    }
                }

                // If we got here, it must mean we are going to retry the action.  But we need to wait for the specified interval before we try it again.
                Thread.Sleep(retryInterval);
            }
            throw new AggregateException(exceptions.Last());
        }

        private static readonly HttpStatusCode[] HttpStatusCodesToRetry =
        {
            HttpStatusCode.RequestTimeout, // 408
            HttpStatusCode.InternalServerError, // 500
            HttpStatusCode.ServiceUnavailable, // 503
            HttpStatusCode.GatewayTimeout // 504
        };

        private static bool IsRetriable(HttpResponseMessage response)
        {
            return HttpStatusCodesToRetry.Contains(response.StatusCode);
        }
    }
}
