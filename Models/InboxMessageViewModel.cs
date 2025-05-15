using System;

namespace NetMvcApp.Models
{
    public class InboxMessageViewModel
    {
        public int MessageId { get; set; }
        public string SenderName { get; set; }
        public DateTime ReceivedTimestamp { get; set; }
        public string EncryptionType { get; set; } // "Triple DES" or "RSA"
        public string ShortPreview { get; set; } // e.g., "[Encrypted Message]" or first few words if feasible
        public string OriginalEncryptedContentBase64 { get; set; }
        public int OriginalSenderId { get; set; }
        public int OriginalReceiverId { get; set; }
        public string MessageType { get; set; } // "tdes" or "rsa" to help controller logic
    }
}

