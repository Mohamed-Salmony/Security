using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NetMvcApp.Models;
using NetMvcApp.Services;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.IO; // Required for Path.Combine
using System;
using System.Collections.Generic; // Required for List<T>
using System.Globalization; // Required for DateTime parsing if not already present

namespace NetMvcApp.Controllers
{
    public class MessageController : Controller
    {
        private readonly FileStorageService _fileStorageService;
        private readonly EncryptionService _encryptionService;

        public MessageController()
        {
            _fileStorageService = new FileStorageService();
            _encryptionService = new EncryptionService();
        }

        private async Task<bool> IsUserLoggedInAsync()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return false;
            }
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                return false;
            }
            var user = await _fileStorageService.GetUserByEmailAsync(userEmail);
            return user != null && user.UserId == HttpContext.Session.GetInt32("UserId");
        }

        [HttpGet]
        public async Task<IActionResult> SendTripleDESMessage()
        {
            if (!await IsUserLoggedInAsync())
            {
                return RedirectToAction("Login", "Account");
            }

            var users = await _fileStorageService.GetAllUsersAsync();
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            ViewBag.Users = new SelectList(users.Where(u => u.UserId != currentUserId), "UserId", "Name");
            return View(new SendMessageViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendTripleDESMessage(SendMessageViewModel model)
        {
            if (!await IsUserLoggedInAsync())
            {
                return RedirectToAction("Login", "Account");
            }

            var senderUserId = HttpContext.Session.GetInt32("UserId");
            if (senderUserId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                var receiver = await _fileStorageService.GetUserByIdAsync(model.ReceiverUserId);
                if (receiver == null)
                {
                    ModelState.AddModelError("ReceiverUserId", "المستخدم المستلم غير موجود.");
                }
                else if (receiver.UserId == senderUserId)
                {
                    ModelState.AddModelError("ReceiverUserId", "لا يمكنك إرسال رسالة إلى نفسك.");
                }

                if (ModelState.IsValid)
                {
                    string encryptedTextBase64 = _encryptionService.EncryptTripleDES(model.Content, out string keyBase64, out string ivBase64);

                    int messageId = await _fileStorageService.GetNextMessageIdAsync(
                        Path.Combine(Directory.GetCurrentDirectory(), "Data", "messages_tdes.txt"),
                        new object() // Placeholder for lock object, consider FileStorageService internal lock
                    );

                    await _fileStorageService.AddTripleDESMessageAsync(messageId, senderUserId.Value, model.ReceiverUserId, encryptedTextBase64);
                    await _fileStorageService.AddTripleDESKeyAndIvAsync(messageId, keyBase64, ivBase64);

                    TempData["SuccessMessage"] = "تم إرسال الرسالة المشفرة بـ Triple DES بنجاح!";
                    return RedirectToAction("SendTripleDESMessage");
                }
            }
            var usersForDropdown = await _fileStorageService.GetAllUsersAsync();
            var currentUserIdForDropdown = HttpContext.Session.GetInt32("UserId");
            ViewBag.Users = new SelectList(usersForDropdown.Where(u => u.UserId != currentUserIdForDropdown), "UserId", "Name", model.ReceiverUserId);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> SendRSAMessage()
        {
            if (!await IsUserLoggedInAsync())
            {
                return RedirectToAction("Login", "Account");
            }

            var users = await _fileStorageService.GetAllUsersAsync();
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            ViewBag.Users = new SelectList(users.Where(u => u.UserId != currentUserId && !string.IsNullOrEmpty(u.RSAPublicKey)), "UserId", "Name");
            return View(new SendMessageViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendRSAMessage(SendMessageViewModel model)
        {
            if (!await IsUserLoggedInAsync())
            {
                return RedirectToAction("Login", "Account");
            }

            var senderUserId = HttpContext.Session.GetInt32("UserId");
            if (senderUserId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                var receiver = await _fileStorageService.GetUserByIdAsync(model.ReceiverUserId);
                if (receiver == null)
                {
                    ModelState.AddModelError("ReceiverUserId", "المستخدم المستلم غير موجود.");
                }
                else if (receiver.UserId == senderUserId)
                {
                    ModelState.AddModelError("ReceiverUserId", "لا يمكنك إرسال رسالة إلى نفسك.");
                }
                else if (string.IsNullOrEmpty(receiver.RSAPublicKey))
                {
                    ModelState.AddModelError("ReceiverUserId", "المستخدم المستلم ليس لديه مفتاح عام RSA لإرسال الرسالة.");
                }

                if (ModelState.IsValid)
                {
                    string encryptedTextBase64 = _encryptionService.EncryptRSA(model.Content, receiver.RSAPublicKey);

                    int messageId = await _fileStorageService.GetNextMessageIdAsync(
                        Path.Combine(Directory.GetCurrentDirectory(), "Data", "messages_rsa.txt"),
                        new object() // Placeholder for lock object
                    );

                    await _fileStorageService.AddRSAMessageAsync(messageId, senderUserId.Value, model.ReceiverUserId, encryptedTextBase64);

                    TempData["SuccessMessage"] = "تم إرسال الرسالة المشفرة بـ RSA بنجاح!";
                    return RedirectToAction("SendRSAMessage");
                }
            }

            var usersForDropdownRsa = await _fileStorageService.GetAllUsersAsync();
            var currentUserIdRsa = HttpContext.Session.GetInt32("UserId");
            ViewBag.Users = new SelectList(usersForDropdownRsa.Where(u => u.UserId != currentUserIdRsa && !string.IsNullOrEmpty(u.RSAPublicKey)), "UserId", "Name", model.ReceiverUserId);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Inbox()
        {
            if (!await IsUserLoggedInAsync())
            {
                return RedirectToAction("Login", "Account");
            }

            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var inboxMessages = new List<InboxMessageViewModel>();
            var tdesMessagesRaw = await _fileStorageService.GetReceivedTripleDESMessagesAsync(currentUserId.Value);
            foreach (var rawMsg in tdesMessagesRaw)
            {
                var sender = await _fileStorageService.GetUserByIdAsync(rawMsg.SenderUserId);
                inboxMessages.Add(new InboxMessageViewModel
                {
                    MessageId = rawMsg.MessageId,
                    SenderName = sender?.Name ?? "مستخدم غير معروف",
                    ReceivedTimestamp = rawMsg.Timestamp,
                    EncryptionType = "Triple DES",
                    ShortPreview = "[رسالة مشفرة بـ Triple DES]",
                    OriginalEncryptedContentBase64 = rawMsg.EncryptedTextBase64,
                    OriginalSenderId = rawMsg.SenderUserId,
                    OriginalReceiverId = rawMsg.ReceiverUserId,
                    MessageType = "tdes"
                });
            }

            var rsaMessagesRaw = await _fileStorageService.GetReceivedRSAMessagesAsync(currentUserId.Value);
            foreach (var rawMsg in rsaMessagesRaw)
            {
                var sender = await _fileStorageService.GetUserByIdAsync(rawMsg.SenderUserId);
                inboxMessages.Add(new InboxMessageViewModel
                {
                    MessageId = rawMsg.MessageId,
                    SenderName = sender?.Name ?? "مستخدم غير معروف",
                    ReceivedTimestamp = rawMsg.Timestamp,
                    EncryptionType = "RSA",
                    ShortPreview = "[رسالة مشفرة بـ RSA]",
                    OriginalEncryptedContentBase64 = rawMsg.EncryptedTextBase64,
                    OriginalSenderId = rawMsg.SenderUserId,
                    OriginalReceiverId = rawMsg.ReceiverUserId,
                    MessageType = "rsa"
                });
            }
            var sortedMessages = inboxMessages.OrderByDescending(m => m.ReceivedTimestamp).ToList();
            return View(sortedMessages);
        }

        // New Action Method for Viewing Message Detail
        [HttpGet]
        public async Task<IActionResult> ViewMessageDetail(int messageId, string messageType)
        {
            if (!await IsUserLoggedInAsync())
            {
                return RedirectToAction("Login", "Account");
            }

            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            string decryptedContent = null;
            string senderName = "مستخدم غير معروف";
            DateTime receivedTimestamp = DateTime.MinValue;
            string encryptionTypeDisplay = "غير معروف";
            string errorMessage = null;
            User sender = null;

            try
            {
                if (messageType == "tdes")
                {
                    var messages = await _fileStorageService.GetReceivedTripleDESMessagesAsync(currentUserId.Value);
                    var message = messages.FirstOrDefault(m => m.MessageId == messageId);
                    if (message != null && message.ReceiverUserId == currentUserId.Value)
                    {
                        var (keyBase64, ivBase64) = await _fileStorageService.GetTripleDESKeyAndIvAsync(messageId);
                        if (keyBase64 != null && ivBase64 != null)
                        {
                            decryptedContent = _encryptionService.DecryptTripleDES(message.EncryptedTextBase64, keyBase64, ivBase64);
                            sender = await _fileStorageService.GetUserByIdAsync(message.SenderUserId);
                            senderName = sender?.Name ?? "مستخدم غير معروف";
                            receivedTimestamp = message.Timestamp;
                            encryptionTypeDisplay = "Triple DES";
                        }
                        else
                        {
                            errorMessage = "لم يتم العثور على مفتاح فك التشفير لهذه الرسالة.";
                        }
                    }
                    else
                    {
                        errorMessage = "الرسالة غير موجودة أو لا تملك صلاحية لعرضها.";
                    }
                }
                else if (messageType == "rsa")
                {
                    var messages = await _fileStorageService.GetReceivedRSAMessagesAsync(currentUserId.Value);
                    var message = messages.FirstOrDefault(m => m.MessageId == messageId);
                    if (message != null && message.ReceiverUserId == currentUserId.Value)
                    {
                        var privateKeyBase64 = await _fileStorageService.GetRsaPrivateKeyAsync(currentUserId.Value);
                        if (!string.IsNullOrEmpty(privateKeyBase64))
                        {
                            decryptedContent = _encryptionService.DecryptRSA(message.EncryptedTextBase64, privateKeyBase64);
                            sender = await _fileStorageService.GetUserByIdAsync(message.SenderUserId);
                            senderName = sender?.Name ?? "مستخدم غير معروف";
                            receivedTimestamp = message.Timestamp;
                            encryptionTypeDisplay = "RSA";
                        }
                        else
                        {
                            errorMessage = "لم يتم العثور على مفتاحك الخاص لفك تشفير هذه الرسالة.";
                        }
                    }
                    else
                    {
                        errorMessage = "الرسالة غير موجودة أو لا تملك صلاحية لعرضها.";
                    }
                }
                else
                {
                    errorMessage = "نوع رسالة غير صالح.";
                }
            }
            catch (Exception ex)
            {
                // Log the exception ex.Message
                errorMessage = "حدث خطأ أثناء محاولة فك تشفير الرسالة. قد تكون البيانات تالفة أو المفتاح غير صحيح.";
                // For security, don't reveal too much detail from the exception to the user.
            }

            if (errorMessage != null && decryptedContent == null) // If error occurred, ensure decryptedContent is null
            {
                // Keep existing values for senderName, receivedTimestamp, encryptionTypeDisplay if partially retrieved before error
                // Or reset them if appropriate for the error context
            }

            var viewModel = new MessageDetailViewModel
            {
                SenderName = senderName,
                ReceivedTimestamp = receivedTimestamp,
                EncryptionType = encryptionTypeDisplay,
                DecryptedContent = decryptedContent,
                ErrorMessage = errorMessage
            };

            return View(viewModel);
        }
    }
}

