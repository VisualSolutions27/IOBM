namespace Gijima.IOBM.Infrastructure.Structs
{
    public class ApplicationMessage
    {
        public enum MessageTypes
        {
            Information,
            ProcessError,
            SystemError
        }

        public enum MessageImages
        {
            Error,
            Exclamation,
            Information,
            Question,
            Stop,
            Warning
        }

        public enum MessageButtons
        {
            OK,
            OKCance,
            YesNo,
            YesNoCancel
        }

        public string Owner { get; set; }
        public object Message { get; set; }
        public string Header { get; set; }
        public MessageImages MessageImage { get; set; }
        public MessageButtons MessageButton { get; set; }
        public MessageTypes MessageType { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="owner">Window sending the message.</param>
        /// <param name="message">The message description.</param>
        /// <param name="header">The message box header.</param>
        /// <param name="messageImage">The message box image to display.</param>
        /// <param name="messageButton">The message box buttons to display.</param>
        /// <param name="messageType">The message type.</param>
        public ApplicationMessage(string owner, string message, string header, MessageImages messageImage, MessageButtons messageButton, MessageTypes messageType)
        {
            Owner = owner;
            Message = message;
            Header = header;
            MessageImage = messageImage;
            MessageButton = messageButton;
            MessageType = messageType;
        }
    }
}
