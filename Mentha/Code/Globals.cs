using Mentha.Banks;
using System.Collections.Generic;

namespace Mentha.Code {
    public class Globals {
        public static List<BaseBank> Banks = new List<BaseBank>();
        public static string MasterPassword;
        public static Settings Settings = Settings.Load();

        static Globals() {
            // Add supported banks to list
            Banks.AddRange(new BaseBank[] {
                new Banks.CIBC.CIBC(),
                // TODOX They changed something after they got hacked, and now I always get a 403 while logging in
                // Need to investigate what changed...
                // new Banks.Simplii.Simplii(),
                new Banks.Tangerine.Tangerine(),
                new Banks.TD.TD(),
            });
        }
    }
}
