using System;

namespace NextHorizon.Models
{
    public class SupportMessage
    {
        public int Id { get; set; }
        public int ConversationId { get; set; }
        public int UserId { get; set; }
        public int SenderStaffId { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; } = false;
        public string MessageType { get; set; } = "text"; // text, file, etc.
        public string AttachmentUrl { get; set; } = string.Empty;
    }
}
