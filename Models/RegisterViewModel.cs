using System.ComponentModel.DataAnnotations;

namespace NetMvcApp.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "الاسم مطلوب")]
        [Display(Name = "الاسم")]
        public string Name { get; set; }

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة")]
        [Display(Name = "البريد الإلكتروني")]
        public string Email { get; set; }

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "يجب أن تكون كلمة المرور مكونة من 6 أحرف على الأقل", MinimumLength = 6)]
        [Display(Name = "كلمة المرور")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "تأكيد كلمة المرور")]
        [Compare("Password", ErrorMessage = "كلمة المرور وتأكيدها غير متطابقين")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        [Phone(ErrorMessage = "صيغة رقم الهاتف غير صحيحة")]
        [Display(Name = "رقم الهاتف")]
        public string PhoneNumber { get; set; }
    }
}

