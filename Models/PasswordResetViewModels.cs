using System.ComponentModel.DataAnnotations;

namespace NetMvcApp.Models
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة")]
        [Display(Name = "البريد الإلكتروني")]
        public string Email { get; set; }
    }

    public class VerifyPhoneNumberViewModel
    {
        [Required]
        public string Email { get; set; } // To carry over the email

        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        [Phone(ErrorMessage = "صيغة رقم الهاتف غير صحيحة")]
        [Display(Name = "رقم الهاتف المسجل")]
        public string PhoneNumber { get; set; }
    }

    public class ResetPasswordViewModel
    {
        [Required]
        public string Email { get; set; } // To carry over the email for updating

        [Required(ErrorMessage = "كلمة المرور الجديدة مطلوبة")]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "يجب أن تكون كلمة المرور مكونة من 6 أحرف على الأقل", MinimumLength = 6)]
        [Display(Name = "كلمة المرور الجديدة")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "تأكيد كلمة المرور الجديدة")]
        [Compare("NewPassword", ErrorMessage = "كلمة المرور الجديدة وتأكيدها غير متطابقين")]
        public string ConfirmNewPassword { get; set; }
    }
}

