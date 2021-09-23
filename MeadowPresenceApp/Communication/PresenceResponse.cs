namespace MeadowPresenceApp.Communication
{
    /// <example>
    /// {
    ///     "@odata.context": "https://graph.microsoft.com/v1.0/$metadata#users('53902bfa-71b8-487f-b7c8-b9fc57d77756')/presence/$entity",
    ///     "id": "53902bfa-71b8-487f-b7c8-b9fc57d77756",
    ///     "availability": "Offline",
    ///     "activity": "Offline"
    /// }
    /// </example>
    public class PresenceResponse
    {
        public string id { get; set; }
        public string availability { get; set; }
        public string activity { get; set; }
    }
}
