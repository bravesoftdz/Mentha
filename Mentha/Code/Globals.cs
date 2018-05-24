using Mentha.Banks;
using System;
using System.Collections.Generic;
using System.IO;

namespace Mentha.Code {
    public class Globals {
        public static List<BaseBank> Banks = new List<BaseBank>();
        public static string MasterPassword;

        static Globals() {
            Banks.AddRange(new BaseBank[] {
                new Banks.CIBC.CIBC(),
                new Banks.Simplii.Simplii(),
                new Banks.Tangerine.Tangerine(),
                new Banks.TD.TD(),
            });
        }

        public static string GetAppDataDirectory() {
            string AppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(AppData, "R&M Software", "Mentha");
        }

        public static string GetAppDataFilename(string fileId) {
            return Path.Combine(GetAppDataDirectory(), $"{fileId}.dat");
        }
    }
}
