using LitJson;
using System;

namespace MeadowPresenceApp.Communication
{
    public class UnauthorizedResponseJsonReader
    {
        public static UnauthorizedResponse ToObject(string json_text)
        {
            JsonData data = JsonMapper.ToObject(json_text);
            var dateString = data["error"]["innerError"]["date"].ToString();
            var innerError = new InnerError(DateTime.Parse(dateString),
                (string)data["error"]["innerError"]["request-id"],
                (string)data["error"]["innerError"]["client-request-id"]);
            var error = new Error((string)data["error"]["code"], (string)data["error"]["message"], innerError);

            return new UnauthorizedResponse(error);
        }
    }
}
