using System;
using System.Security.Cryptography;
using System.Text;

namespace Mentha.Code {
    class Encryption {
        /// <summary>
        /// Number of iterations to use with Rfc2898DeriveBytes.  The more the merrier.
        /// Safe to change at any time because this value is only used when encrypting.  
        /// (For decrypting the iteration count is stored along with the encrypted data)
        /// </summary>
        private const int Rfc2898DeriveBytes_Iterations = 34906;

        /// <summary>
        /// Encrypts the given plaintext string with the given password (stretched using PBKDF2 with the given number of iterations)
        /// using the AES128 algorithm.
        /// </summary>
        /// <param name="plaintext">The plaintext to encrypt</param>
        /// <param name="password">The password to use to perform the encryption</param>
        /// <param name="iterations">The number of iterations of the key stretching function to perform</param>
        /// <returns>The encrypted version of the plaintext, along with other information required to later decrypt the string</returns>
        public static string Encrypt(string plaintext, string password, int iterations = Rfc2898DeriveBytes_Iterations) {
            // Validate the parameters
            if (string.IsNullOrWhiteSpace(plaintext)) {
                throw new ArgumentNullException(nameof(plaintext));
            }
            if (string.IsNullOrWhiteSpace(password)) {
                throw new ArgumentNullException(nameof(password));
            }
            if (iterations < 10000) {
                throw new ArgumentOutOfRangeException(nameof(iterations), "Number of iterations is too low (10,000 is the minimum)");
            }

            // Get random bytes for salt
            var RNG = new RNGCryptoServiceProvider();
            byte[] SaltBytes = new byte[16];
            RNG.GetBytes(SaltBytes);

            // PBKDF2 key stretching
            var DerivedPassword = new Rfc2898DeriveBytes(password, SaltBytes, iterations);

            // Encryption with 128bit AES (using 192 or 256 bit isn't a good idea because Rfc2898DeriveBytes uses SHA-1, a 160 bit algorithm,
            // so it's not recommended to take out more than 160 bits (increases time required to hash, but not time required to verify
            // a hash, which means a defender does extra work but an attacker doesn't have to...Google it for reasons)
            using (RijndaelManaged MyRijndael = new RijndaelManaged()) {
                MyRijndael.GenerateIV();
                MyRijndael.Key = DerivedPassword.GetBytes(16);
                MyRijndael.Mode = CipherMode.CBC;
                using (ICryptoTransform Encryptor = MyRijndael.CreateEncryptor()) {
                    byte[] PlaintextBytes = Encoding.UTF8.GetBytes(plaintext);
                    byte[] EncryptedBytes = Encryptor.TransformFinalBlock(PlaintextBytes, 0, PlaintextBytes.Length);
                    return $"{iterations}!{Convert.ToBase64String(SaltBytes)}!{Convert.ToBase64String(MyRijndael.IV)}!{Convert.ToBase64String(EncryptedBytes)}";
                }
            }
        }

        /// <summary>
        /// Decrypts the given encrypted text with the given password
        /// </summary>
        /// <param name="encryptedText">The encrypted text (along with the other information required to decrypt the string)</param>
        /// <param name="password">The password to use to perform the decryption</param>
        /// <returns></returns>
        public static string Decrypt(string encryptedText, string password) {
            // Validate the parameters
            if (string.IsNullOrWhiteSpace(encryptedText)) {
                throw new ArgumentNullException(nameof(encryptedText));
            }
            if (string.IsNullOrWhiteSpace(password)) {
                throw new ArgumentNullException(nameof(password));
            }

            // encryptedText is in the format {int:iterations}!{base64:salt}!{base64:iv}!{base64:encrypted_password}, so split it
            string[] Pieces = encryptedText.Split('!');
            int Iterations = int.Parse(Pieces[0]);
            byte[] SaltBytes = Convert.FromBase64String(Pieces[1]);
            byte[] IVBytes = Convert.FromBase64String(Pieces[2]);
            encryptedText = Pieces[3];

            // Match the key stretching that was done during encryption
            var DerivedPassword = new Rfc2898DeriveBytes(password, SaltBytes, Iterations);

            // Decrypt using the same settings previously used to encrypt
            using (RijndaelManaged MyRijndael = new RijndaelManaged()) {
                MyRijndael.IV = IVBytes;
                MyRijndael.Key = DerivedPassword.GetBytes(16);
                MyRijndael.Mode = CipherMode.CBC;
                using (ICryptoTransform Decryptor = MyRijndael.CreateDecryptor()) {
                    byte[] EncryptedBytes = Convert.FromBase64String(encryptedText);
                    byte[] DecryptedBytes = Decryptor.TransformFinalBlock(EncryptedBytes, 0, EncryptedBytes.Length);
                    return Encoding.UTF8.GetString(DecryptedBytes, 0, DecryptedBytes.Length);
                }
            }
        }
    }
}
