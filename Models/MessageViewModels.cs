using System.ComponentModel.DataAnnotations;

namespace NetMvcApp.Models
{
    public class SendMessageViewModel
    {
        [Required(ErrorMessage = "معرف المستخدم المستلم مطلوب")]
        [Display(Name = "إلى (معرف المستخدم)")]
        public int ReceiverUserId { get; set; }

        [Required(ErrorMessage = "محتوى الرسالة مطلوب")]
        [Display(Name = "الرسالة")]
        public string Content { get; set; }

        // To distinguish between TripleDES and RSA in a unified form (if needed later)
        // public string EncryptionType { get; set; } // "TripleDES" or "RSA"
    }

    public class Message
    {
        public int MessageId { get; set; }
        public int SenderUserId { get; set; }
        public int ReceiverUserId { get; set; }
        public string EncryptedTextBase64 { get; set; }
        public DateTime Timestamp { get; set; }
        // For TripleDES, key is stored separately. For RSA, no key stored with message.

        // Property to hold decrypted content for display purposes
        [Display(Name = "محتوى الرسالة") ]
        public string DecryptedContent { get; set; } // Not stored in file, used for views
        public string SenderName { get; set; } // Not stored in file, used for views
        public string ReceiverName { get; set; } // Not stored in file, used for views
    }
}

