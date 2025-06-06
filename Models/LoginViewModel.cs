using System.ComponentModel.DataAnnotations;

namespace NetMvcApp.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة")]
        [Display(Name = "البريد الإلكتروني")]
        public string Email { get; set; }

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        [DataType(DataType.Password)]
        [Display(Name = "كلمة المرور")]
        public string Password { get; set; }

        // Optional: Remember me
        // [Display(Name = "تذكرني؟")]
        // public bool RememberMe { get; set; }
    }
}

