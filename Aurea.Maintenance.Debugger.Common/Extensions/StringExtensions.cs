
namespace Aurea.Maintenance.Debugger.Common.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;

    public static class StringExtensions
    {
        public static string GetStrongHash(this string theString)
        {
            var sha = new SHA1CryptoServiceProvider();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(theString));
            return BitConverter.ToString(hash).Replace("-", "");
        }
    }
}
