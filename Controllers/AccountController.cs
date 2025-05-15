using Microsoft.AspNetCore.Mvc;
using NetMvcApp.Models;
using NetMvcApp.Services;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http; 
using System.Text; 
using Microsoft.AspNetCore.Authorization; 

namespace NetMvcApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly FileStorageService _fileStorageService;
        private readonly EncryptionService _encryptionService;
        private const string PasswordResetEmailKey = "PasswordResetEmail";
        private const string PhoneVerifiedKey = "PhoneVerifiedForPasswordReset";

        public AccountController()
        {
            _fileStorageService = new FileStorageService();
            _encryptionService = new EncryptionService();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (await _fileStorageService.EmailExistsAsync(model.Email))
                {
                    ModelState.AddModelError(string.Empty, "البريد الإلكتروني مسجل بالفعل.");
                    return View(model);
                }

                var (publicKey, privateKey) = _encryptionService.GenerateRSAKeyPair();
                var hashedPassword = _encryptionService.HashPasswordSHA256(model.Password);
                var userId = await _fileStorageService.GetNextUserIdAsync();

                var user = new User
                {
                    UserId = userId,
                    Name = model.Name,
                    Email = model.Email,
                    HashedPassword = hashedPassword,
                    PhoneNumber = model.PhoneNumber,
                    RSAPublicKey = publicKey
                };

                await _fileStorageService.AddUserAsync(user);
                await _fileStorageService.AddRsaPrivateKeyAsync(userId, privateKey);

                TempData["SuccessMessage"] = "تم التسجيل بنجاح! يمكنك الآن تسجيل الدخول.";
                return RedirectToAction("Login", "Account");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View(); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _fileStorageService.GetUserByEmailAsync(model.Email);
                if (user != null)
                {
                    var hashedInputPassword = _encryptionService.HashPasswordSHA256(model.Password);
                    if (user.HashedPassword.Equals(hashedInputPassword, System.StringComparison.OrdinalIgnoreCase))
                    {
                        HttpContext.Session.SetInt32("UserId", user.UserId);
                        HttpContext.Session.SetString("UserName", user.Name);
                        HttpContext.Session.SetString("UserEmail", user.Email); 
                        // return RedirectToAction("Index", "Home"); 
                        // For now, redirect to a page where they can send messages or logout
                        return RedirectToAction("SendTripleDESMessage", "Message"); 
                    }
                }
                ModelState.AddModelError(string.Empty, "البريد الإلكتروني أو كلمة المرور غير صحيحة.");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _fileStorageService.GetUserByEmailAsync(model.Email);
                if (user != null)
                {
                    HttpContext.Session.SetString(PasswordResetEmailKey, model.Email);
                    HttpContext.Session.Remove(PhoneVerifiedKey); 
                    return RedirectToAction("VerifyPhoneNumber", "Account");
                }
                ModelState.AddModelError(string.Empty, "البريد الإلكتروني غير موجود.");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult VerifyPhoneNumber()
        {
            var email = HttpContext.Session.GetString(PasswordResetEmailKey);
            if (string.IsNullOrEmpty(email))
            {
                TempData["ErrorMessage"] = "حدث خطأ ما، يرجى المحاولة مرة أخرى.";
                return RedirectToAction("ForgotPassword");
            }
            return View(new VerifyPhoneNumberViewModel { Email = email }); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyPhoneNumber(VerifyPhoneNumberViewModel model)
        {
            var emailForReset = HttpContext.Session.GetString(PasswordResetEmailKey);
            if (string.IsNullOrEmpty(emailForReset) || !emailForReset.Equals(model.Email, System.StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "جلسة إعادة تعيين كلمة المرور غير صالحة أو منتهية الصلاحية.";
                return RedirectToAction("ForgotPassword");
            }

            if (ModelState.IsValid)
            {
                var user = await _fileStorageService.GetUserByEmailAsync(model.Email);
                if (user != null && user.PhoneNumber.Equals(model.PhoneNumber))
                {
                    HttpContext.Session.SetString(PhoneVerifiedKey, "true");
                    return RedirectToAction("ResetPassword", "Account");
                }
                ModelState.AddModelError(string.Empty, "رقم الهاتف غير صحيح لهذا الحساب.");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult ResetPassword()
        {
            var email = HttpContext.Session.GetString(PasswordResetEmailKey);
            var phoneVerified = HttpContext.Session.GetString(PhoneVerifiedKey);

            if (string.IsNullOrEmpty(email) || phoneVerified != "true")
            {
                TempData["ErrorMessage"] = "يجب التحقق من البريد الإلكتروني ورقم الهاتف أولاً.";
                return RedirectToAction("ForgotPassword");
            }
            return View(new ResetPasswordViewModel { Email = email }); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            var emailForReset = HttpContext.Session.GetString(PasswordResetEmailKey);
            var phoneVerified = HttpContext.Session.GetString(PhoneVerifiedKey);

            if (string.IsNullOrEmpty(emailForReset) || !emailForReset.Equals(model.Email, System.StringComparison.OrdinalIgnoreCase) || phoneVerified != "true")
            {
                TempData["ErrorMessage"] = "جلسة إعادة تعيين كلمة المرور غير صالحة أو منتهية الصلاحية، أو لم يتم التحقق من الهاتف.";
                return RedirectToAction("ForgotPassword");
            }

            if (ModelState.IsValid)
            {
                var newHashedPassword = _encryptionService.HashPasswordSHA256(model.NewPassword);
                bool success = await _fileStorageService.UpdateUserPasswordAsync(model.Email, newHashedPassword);

                if (success)
                {
                    HttpContext.Session.Remove(PasswordResetEmailKey);
                    HttpContext.Session.Remove(PhoneVerifiedKey);

                    TempData["SuccessMessage"] = "تم تغيير كلمة المرور بنجاح. يمكنك الآن تسجيل الدخول بكلمة المرور الجديدة.";
                    return RedirectToAction("Login");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "حدث خطأ أثناء تحديث كلمة المرور. يرجى المحاولة مرة أخرى.");
                }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login"); 
            }
            return View(); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userEmail = HttpContext.Session.GetString("UserEmail");

            if (userId == null || string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login");
            }

            if (ModelState.IsValid)
            {
                var user = await _fileStorageService.GetUserByEmailAsync(userEmail);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "حدث خطأ، المستخدم غير موجود.");
                    return View(model);
                }

                var hashedCurrentPassword = _encryptionService.HashPasswordSHA256(model.CurrentPassword);
                if (!user.HashedPassword.Equals(hashedCurrentPassword, System.StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError("CurrentPassword", "كلمة المرور الحالية غير صحيحة.");
                    return View(model);
                }

                var newHashedPassword = _encryptionService.HashPasswordSHA256(model.NewPassword);
                bool success = await _fileStorageService.UpdateUserPasswordAsync(userEmail, newHashedPassword);

                if (success)
                {
                    TempData["SuccessMessage"] = "تم تغيير كلمة المرور بنجاح.";
                    // return RedirectToAction("Index", "Home"); // Or to a profile page
                    return RedirectToAction("SendTripleDESMessage", "Message"); // Or a more appropriate page
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "حدث خطأ أثناء تحديث كلمة المرور. يرجى المحاولة مرة أخرى.");
                }
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // Good practice for actions that change state
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Clear all session variables
            TempData["SuccessMessage"] = "تم تسجيل الخروج بنجاح.";
            return RedirectToAction("Login", "Account"); // Redirect to login page
        }
    }
}

