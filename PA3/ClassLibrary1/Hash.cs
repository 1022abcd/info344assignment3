using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1
{
    public class HashUrl
    {
        public HashUrl(string input)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(input);
            SHA256Managed hashstring = new SHA256Managed();
            byte[] hash = hashstring.ComputeHash(bytes);
            string hashString = string.Empty;
            foreach(byte x in hash)
            {
                hashString += String.Format("{0:x2}", x);
            }
            this.encoded = hashString;
        }

        public string encoded { get; set; }
    }
}
