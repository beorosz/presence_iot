using System;
using System.Collections.Generic;
using System.Text;

namespace MeadowPresenceApp.Communication
{
    /// <summary>
    ///	{
    ///		"error": {
    ///			"code": "InvalidAuthenticationToken",
    ///			"message": "Access token has expired or is not yet valid.",
    ///			"innerError": {
    ///				"date": "2021-09-22T18:32:58",
    ///				"request-id": "ca41ccdd-ed28-4235-9df1-305864cd279d",
    ///				"client-request-id": "ca41ccdd-ed28-4235-9df1-305864cd279d"
    ///		}
    ///		}
    ///	}
    /// </summary>
    public class UnauthorizedResponse
    {
        public Error Error { get; set; }

        public UnauthorizedResponse(Error error)
        {
            Error = error;
        }
    }

    public class Error
    {
        public string Code { get; }
        public string Message { get; }
        public InnerError InnerError { get; }

        public Error(string code, string message, InnerError innerError)
        {
            Code = code;
            Message = message;
            InnerError = innerError;
        }
    }

    public class InnerError
    {
        public DateTime Date { get; }
        public string RequestId { get; }
        public string ClientRequestId { get; }

        public InnerError(DateTime date, string request_id, string client_request_id)
        {
            Date = date;
            RequestId = request_id;
            ClientRequestId = client_request_id;
        }
    }
}
