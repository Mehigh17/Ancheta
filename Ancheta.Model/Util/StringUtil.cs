using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Ancheta.Model.Util
{
    public static class StringUtil
    {

        /// <summary>
        /// Generate a random string containing only alphanumeric characters.
        /// </summary>
        /// <param name="length">The length of the required string.</param>
        public static string GetRandomString(int length)
        {
            const string validCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            using var rngCsp = new RNGCryptoServiceProvider();

            var bytes = new byte[length];
            var sb = new StringBuilder();
            rngCsp.GetBytes(bytes);

            foreach (var b in bytes)
            {
                sb.Append(validCharacters[b % validCharacters.Length]);
            }

            return sb.ToString();
        }

    }
}