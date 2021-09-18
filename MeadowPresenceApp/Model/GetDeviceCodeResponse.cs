namespace MeadowPresenceApp.Model
{
    /// <example>
    /// {
    ///     "user_code": "EMWM8WFNC",
    ///     "device_code": "EAQABAAEAAAD--DLA3VO7QrddgJg7Wevr782YxCLXIxJWjKUA9p8y5T7L9XFVn5MlzBX49zW--0aJfXHPOj9yY_2L4Pk_VtliTtAau11NMojST_-I1ZbiK6pPGii4UKi-Ot-MzFPBzZOh5wgImG3gY0D87h3VPw0gK9zlwNq3Xo40TnuonrqbgyPL_IqfomuaHq_H3ReT9VUgAA",
    ///     "verification_uri": "https://microsoft.com/devicelogin",
    ///     "expires_in": 900,
    ///     "interval": 5,
    ///     "message": "To sign in, use a web browser to open the page https://microsoft.com/devicelogin and enter the code EMWM8WFNC to authenticate."
    /// }
    /// </example>
    public class GetDeviceCodeResponse
    {
        public string user_code { get; set; }

        public string device_code { get; set; }

        public string verification_uri { get; set; }
        
        public int expires_in { get; set; }

        public int interval { get; set; }

        public string message { get; set; }
    }
}
