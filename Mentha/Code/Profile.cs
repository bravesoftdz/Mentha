using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Mentha.Code {
    public class Profile {
        public string Id { get; set; }
        public string Bank { get; set; }
        public string Description { get; set; }
        public string CardNumber { get; set; }
        public string Password { get; set; }
        public List<Account_SecurityQuestion> SecurityQuestions { get; set; } = new List<Account_SecurityQuestion>();

        public Profile() {
            Id = Guid.NewGuid().ToString();
        }
    }

    public class Account_SecurityQuestion {
        public string Question { get; set; }
        public string Answer { get; set; }
    }
}
