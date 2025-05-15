using System;

namespace NetMvcApp.Models
{
    public class MessageDetailViewModel
    {
        public string SenderName { get; set; }
        public DateTime ReceivedTimestamp { get; set; }
        public string EncryptionType { get; set; } // "Triple DES" or "RSA"
        public string DecryptedContent { get; set; }
        public string ErrorMessage { get; set; } // To display any errors during decryption
    }
}

