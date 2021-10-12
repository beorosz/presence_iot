namespace MeadowPresenceApp.Model
{
    public class LogMessage
    {
        public Category Category { get; set; }
        public string Message { get; set; }

        public LogMessage(Category category, string message)
        {
            Category = category;
            Message = message;
        }
    }
}
