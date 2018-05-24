using Mentha.Forms;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace Mentha.Code {
    public class Settings {
        public Point FormLocation { get; set; }
        public Size FormSize { get; set; }
        public FormWindowState FormWindowState { get; set; }
        public List<Profile> Profiles { get; set; } = new List<Profile>();



        private static string Filename { get; set; }
        private static object SaveLock = new object();



        public static Settings Load() {
            // Get the filename we'll load/save
            string AppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            Filename = Path.Combine(AppData, "R&M Software", "Mentha.dat");

            if (File.Exists(Filename)) {
                while (true) {
                    // Prompt for the master password
                    using (var Form = new EnterMasterPasswordForm()) {
                        if (Form.ShowDialog() == DialogResult.OK) {
                            // Load and decrypt the data file
                            try {
                                string EncryptedData = File.ReadAllText(Filename);
                                string DecryptedData = Encryption.Decrypt(EncryptedData, Globals.MasterPassword);
                                return JsonConvert.DeserializeObject<Settings>(DecryptedData);
                            } catch (CryptographicException) {
                                MessageBox.Show("That doesn't appear to be the correct Master Password, please try again.");
                            } catch (Exception) {
                                MessageBox.Show($"An exception was encountered while loading your settings.\r\n\r\nMentha will now terminate.");
                                Application.Exit();
                                break;
                            }
                        } else {
                            MessageBox.Show("A Master Password is required to load your settings.\r\n\r\nMentha will now terminate.");
                            Application.Exit();
                            break;
                        }
                    }
                }
            }

            // If we get here either the file didn't exist, or we're about to terminate the application, so return blank settings
            return new Settings();
        }



        public void Save() {
            // Tangerine calls this to save security answers -- since the user has to type them in I don't think this could be called
            // twice at the same time from two different threads, but doens't hurt to add the lock
            lock (SaveLock) {
                // Create data directory, if it does not yet exist
                var DataDirectory = Path.GetDirectoryName(Filename);
                if (!Directory.Exists(DataDirectory)) {
                    Directory.CreateDirectory(DataDirectory);
                }

                // Save the settings to the file
                string EncryptedData = Encryption.Encrypt(JsonConvert.SerializeObject(this), Globals.MasterPassword);
                File.WriteAllText(Filename, EncryptedData);
            }
        }
    }
}
