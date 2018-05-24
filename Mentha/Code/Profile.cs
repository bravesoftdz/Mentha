using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace Mentha.Code {
    public class Profile {
        private string FileId { get; set; }

        public string Bank { get; set; }
        public string Description { get; set; }
        public string CardNumber { get; set; }
        public string Password { get; set; }
        public List<Account_SecurityQuestion> SecurityQuestions { get; set; } = new List<Account_SecurityQuestion>();

        public Profile(string fileId) {
            this.FileId = fileId;
        }

        public static Profile Load(string fileId) {
            string Filename = Globals.GetAppDataFilename(fileId);
            string EncryptedData = File.ReadAllText(Filename);
            string DecryptedData = Encryption.Decrypt(EncryptedData, Globals.MasterPassword);
            var Result = JsonConvert.DeserializeObject<Profile>(DecryptedData);
            Result.FileId = fileId;
            return Result;
        }

        public void Save() {
            string Filename = Globals.GetAppDataFilename(FileId);
            string EncryptedData = Encryption.Encrypt(JsonConvert.SerializeObject(this), Globals.MasterPassword);
            File.WriteAllText(Filename, EncryptedData);
        }
    }

    public class Account_SecurityQuestion {
        public string Question { get; set; }
        public string Answer { get; set; }
    }
}
