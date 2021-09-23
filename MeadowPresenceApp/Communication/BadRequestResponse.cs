using System;

namespace MeadowPresenceApp.Communication
{
    /// <example>
    /// {
    ///     "error": "authorization_pending",
    ///     "error_description": "AADSTS70016: OAuth 2.0 device flow error. Authorization is pending. Continue polling.\r\nTrace ID: f522486a-7cd6-4219-9190-076b99669200\r\nCorrelation ID: 75ed0bdd-ccc3-4c00-8bb8-c49b39d1e489\r\nTimestamp: 2021-09-12 08:14:12Z",
    ///     "error_codes": [
    ///         70016
    ///     ],
    ///     "timestamp": "2021-09-12 08:14:12Z",
    ///     "trace_id": "f522486a-7cd6-4219-9190-076b99669200",
    ///     "correlation_id": "75ed0bdd-ccc3-4c00-8bb8-c49b39d1e489",
    ///     "error_uri": "https://login.microsoftonline.com/error?code=70016"
    /// }
    /// </example>
    public class BadRequestResponse
    {
        public string error { get; set; }

        public string error_description { get; set; }

        public int[] error_codes { get; set; }

        public DateTime timestamp { get; set; }

        public string trace_id { get; set; }

        public string correlation_id { get; set; }

        public string error_uri { get; set; }
    }
}
