namespace ChangeHound.Common {
    public class NotificationMessage {
        public string Title { get; }
        public string Message { get; }

        public NotificationMessage(string title, string message) {
            Title = title;
            Message = message;
        }
    }
}
