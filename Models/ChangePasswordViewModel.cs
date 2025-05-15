using System.ComponentModel.DataAnnotations;

namespace NetMvcApp.Models
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "كلمة المرور الحالية مطلوبة")]
        [DataType(DataType.Password)]
        [Display(Name = "كلمة المرور الحالية")]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "كلمة المرور الجديدة مطلوبة")]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "يجب أن تكون كلمة المرور الجديدة مكونة من 6 أحرف على الأقل", MinimumLength = 6)]
        [Display(Name = "كلمة المرور الجديدة")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "تأكيد كلمة المرور الجديدة")]
        [Compare("NewPassword", ErrorMessage = "كلمة المرور الجديدة وتأكيدها غير متطابقين")]
        public string ConfirmNewPassword { get; set; }
    }
}

