using NetMvcApp.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Globalization; // Required for DateTime.ParseExact

namespace NetMvcApp.Services
{
    // Helper class to represent a raw message entry from file
    public class RawMessageEntry
    {
        public int MessageId { get; set; }
        public int SenderUserId { get; set; }
        public int ReceiverUserId { get; set; }
        public string EncryptedTextBase64 { get; set; }
        public DateTime Timestamp { get; set; }
        public string RawLine { get; set; }
    }

    public class FileStorageService
    {
        private readonly string _usersFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "users.txt");
        private readonly string _rsaKeysFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "rsa_keys.txt");
        private readonly string _tdesKeysFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "tdes_keys.txt");
        private readonly string _messagesTdesFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "messages_tdes.txt");
        private readonly string _messagesRsaFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "messages_rsa.txt");

        private static readonly object _userFileLock = new object();
        private static readonly object _rsaKeyFileLock = new object();
        private static readonly object _tdesKeyFileLock = new object();
        private static readonly object _messagesTdesFileLock = new object();
        private static readonly object _messagesRsaFileLock = new object();

        public FileStorageService() // Constructor to ensure Data directory exists
        {
            string dataDir = Path.Combine(Directory.GetCurrentDirectory(), "Data");
            if (!Directory.Exists(dataDir))
            {
                Directory.CreateDirectory(dataDir);
            }
        }

        public async Task<int> GetNextUserIdAsync()
        {
            int lastId = 0;
            if (File.Exists(_usersFilePath))
            {
                string[] lines;
                lock (_userFileLock)
                {
                    lines = File.ReadAllLines(_usersFilePath);
                }
                if (lines.Length > 0)
                {
                    var lastLine = lines.LastOrDefault(l => !string.IsNullOrWhiteSpace(l));
                    if (!string.IsNullOrWhiteSpace(lastLine))
                    {
                        var parts = lastLine.Split(';');
                        if (parts.Length > 0 && int.TryParse(parts[0], out int id))
                        {
                            lastId = id;
                        }
                    }
                }
            }
            return await Task.FromResult(lastId + 1);
        }

        public async Task<int> GetNextMessageIdAsync(string filePath, object fileLock)
        {
            int lastId = 0;
            if (File.Exists(filePath))
            {
                string[] lines;
                lock (fileLock)
                {
                    lines = File.ReadAllLines(filePath);
                }
                if (lines.Length > 0)
                {
                    var lastLine = lines.LastOrDefault(l => !string.IsNullOrWhiteSpace(l));
                    if (!string.IsNullOrWhiteSpace(lastLine))
                    {
                        var parts = lastLine.Split(';');
                        if (parts.Length > 0 && int.TryParse(parts[0], out int id))
                        {
                            lastId = id;
                        }
                    }
                }
            }
            return await Task.FromResult(lastId + 1);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            if (!File.Exists(_usersFilePath))
            {
                return false;
            }
            string[] lines;
            lock (_userFileLock)
            {
                lines = File.ReadAllLines(_usersFilePath);
            }
            foreach (var line in lines)
            {
                var parts = line.Split(';');
                if (parts.Length > 2 && parts[2].Equals(email, StringComparison.OrdinalIgnoreCase))
                {
                    return await Task.FromResult(true);
                }
            }
            return await Task.FromResult(false);
        }

        public async Task AddUserAsync(User user)
        {
            var userLine = $"{user.UserId};{user.Name};{user.Email};{user.HashedPassword};{user.PhoneNumber};{user.RSAPublicKey}";
            lock (_userFileLock)
            {
                File.AppendAllText(_usersFilePath, userLine + Environment.NewLine);
            }
            await Task.CompletedTask;
        }

        public async Task AddRsaPrivateKeyAsync(int userId, string privateKey)
        {
            var keyLine = $"{userId};{privateKey}";
            lock (_rsaKeyFileLock)
            {
                File.AppendAllText(_rsaKeysFilePath, keyLine + Environment.NewLine);
            }
            await Task.CompletedTask;
        }

        public async Task<string> GetRsaPrivateKeyAsync(int userId)
        {
            if (!File.Exists(_rsaKeysFilePath))
            {
                return null;
            }
            string[] lines;
            lock (_rsaKeyFileLock)
            {
                lines = File.ReadAllLines(_rsaKeysFilePath);
            }
            foreach (var line in lines)
            {
                var parts = line.Split(';');
                if (parts.Length >= 2 && int.TryParse(parts[0], out int id) && id == userId)
                {
                    return await Task.FromResult(parts[1]);
                }
            }
            return await Task.FromResult<string>(null);
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            if (!File.Exists(_usersFilePath))
            {
                return null;
            }
            string[] lines;
            lock (_userFileLock)
            {
                lines = File.ReadAllLines(_usersFilePath);
            }
            foreach (var line in lines)
            {
                var parts = line.Split(';');
                if (parts.Length >= 6 && parts[2].Equals(email, StringComparison.OrdinalIgnoreCase))
                {
                    return await Task.FromResult(new User
                    {
                        UserId = int.Parse(parts[0]),
                        Name = parts[1],
                        Email = parts[2],
                        HashedPassword = parts[3],
                        PhoneNumber = parts[4],
                        RSAPublicKey = parts[5]
                    });
                }
            }
            return await Task.FromResult<User>(null);
        }

        public async Task<User> GetUserByIdAsync(int userId)
        {
            if (!File.Exists(_usersFilePath))
            {
                return null;
            }
            string[] lines;
            lock (_userFileLock)
            {
                lines = File.ReadAllLines(_usersFilePath);
            }
            foreach (var line in lines)
            {
                var parts = line.Split(';');
                if (parts.Length >= 6 && int.TryParse(parts[0], out int id) && id == userId)
                {
                    return await Task.FromResult(new User
                    {
                        UserId = int.Parse(parts[0]),
                        Name = parts[1],
                        Email = parts[2],
                        HashedPassword = parts[3],
                        PhoneNumber = parts[4],
                        RSAPublicKey = parts[5]
                    });
                }
            }
            return await Task.FromResult<User>(null);
        }

        public async Task<bool> UpdateUserPasswordAsync(string email, string newHashedPassword)
        {
            if (!File.Exists(_usersFilePath))
            {
                return false;
            }

            List<string> linesList;
            bool userFound = false;
            lock (_userFileLock)
            {
                linesList = File.ReadAllLines(_usersFilePath).ToList();
                for (int i = 0; i < linesList.Count; i++)
                {
                    var parts = linesList[i].Split(';');
                    if (parts.Length >= 6 && parts[2].Equals(email, StringComparison.OrdinalIgnoreCase))
                    {
                        parts[3] = newHashedPassword;
                        linesList[i] = string.Join(";", parts);
                        userFound = true;
                        break;
                    }
                }

                if (userFound)
                {
                    File.WriteAllLines(_usersFilePath, linesList);
                }
            }
            return await Task.FromResult(userFound);
        }

        public async Task AddTripleDESMessageAsync(int messageId, int senderId, int receiverId, string encryptedTextBase64)
        {
            var timestamp = DateTime.UtcNow.ToString("o"); // ISO 8601 format
            var messageLine = $"{messageId};{senderId};{receiverId};{encryptedTextBase64};{timestamp}";
            lock (_messagesTdesFileLock)
            {
                File.AppendAllText(_messagesTdesFilePath, messageLine + Environment.NewLine);
            }
            await Task.CompletedTask;
        }

        public async Task AddTripleDESKeyAndIvAsync(int messageId, string keyBase64, string ivBase64)
        {
            var keyLine = $"{messageId};{keyBase64};{ivBase64}";
            lock (_tdesKeyFileLock)
            {
                File.AppendAllText(_tdesKeysFilePath, keyLine + Environment.NewLine);
            }
            await Task.CompletedTask;
        }

        public async Task AddRSAMessageAsync(int messageId, int senderId, int receiverId, string encryptedTextBase64)
        {
            var timestamp = DateTime.UtcNow.ToString("o"); // ISO 8601 format
            var messageLine = $"{messageId};{senderId};{receiverId};{encryptedTextBase64};{timestamp}";
            lock (_messagesRsaFileLock)
            {
                File.AppendAllText(_messagesRsaFilePath, messageLine + Environment.NewLine);
            }
            await Task.CompletedTask;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            var users = new List<User>();
            if (!File.Exists(_usersFilePath))
            {
                return users;
            }
            string[] lines;
            lock (_userFileLock)
            {
                lines = File.ReadAllLines(_usersFilePath);
            }
            foreach (var line in lines)
            {
                var parts = line.Split(';');
                if (parts.Length >= 6)
                {
                    users.Add(new User
                    {
                        UserId = int.Parse(parts[0]),
                        Name = parts[1],
                        Email = parts[2],
                        RSAPublicKey = parts[5]
                    });
                }
            }
            return await Task.FromResult(users);
        }

        // New methods for message retrieval
        private async Task<List<RawMessageEntry>> GetRawMessagesFromFileAsync(string filePath, object fileLock, int receiverUserId)
        {
            var messages = new List<RawMessageEntry>();
            if (!File.Exists(filePath))
            {
                return messages;
            }

            string[] lines;
            lock (fileLock)
            {
                lines = File.ReadAllLines(filePath);
            }

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split(';');
                // Format: MessageId;SenderUserId;ReceiverUserId;EncryptedTextBase64;Timestamp
                if (parts.Length >= 5 && int.TryParse(parts[2], out int msgReceiverId) && msgReceiverId == receiverUserId)
                {
                    if (int.TryParse(parts[0], out int msgId) &&
                        int.TryParse(parts[1], out int senderId) &&
                        DateTime.TryParseExact(parts[4], "o", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime timestamp))
                    {
                        messages.Add(new RawMessageEntry
                        {
                            MessageId = msgId,
                            SenderUserId = senderId,
                            ReceiverUserId = msgReceiverId,
                            EncryptedTextBase64 = parts[3],
                            Timestamp = timestamp,
                            RawLine = line
                        });
                    }
                }
            }
            return await Task.FromResult(messages.OrderByDescending(m => m.Timestamp).ToList());
        }

        public async Task<List<RawMessageEntry>> GetReceivedTripleDESMessagesAsync(int receiverUserId)
        {
            return await GetRawMessagesFromFileAsync(_messagesTdesFilePath, _messagesTdesFileLock, receiverUserId);
        }

        public async Task<List<RawMessageEntry>> GetReceivedRSAMessagesAsync(int receiverUserId)
        {
            return await GetRawMessagesFromFileAsync(_messagesRsaFilePath, _messagesRsaFileLock, receiverUserId);
        }

        public async Task<(string KeyBase64, string IvBase64)> GetTripleDESKeyAndIvAsync(int messageId)
        {
            if (!File.Exists(_tdesKeysFilePath))
            {
                return (null, null);
            }
            string[] lines;
            lock (_tdesKeyFileLock)
            {
                lines = File.ReadAllLines(_tdesKeysFilePath);
            }
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split(';');
                // Format: MessageId;KeyBase64;IVBase64
                if (parts.Length >= 3 && int.TryParse(parts[0], out int id) && id == messageId)
                {
                    return await Task.FromResult((parts[1], parts[2]));
                }
            }
            return await Task.FromResult<(string, string)>((null, null));
        }
    }
}

