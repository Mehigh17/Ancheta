using System;
using Ancheta.Model.Util;

namespace Ancheta.Model.Services
{
    public class PollService : IPollService
    {

        /// <summary>
        /// Generate a new secret code.
        /// </summary>
        /// <param name="length">The length of the generated secret code. (Can't be shorter than 8)</param>
        /// <returns>A tuple containing the secret code and its hash.</returns>
        public (string, string) GenerateSecretCode(int length = 8)
        {
            if(length < 8) throw new ArgumentException("The secret code length can't be shorter than 8 character.");
            
            var secretCode = StringUtil.GetRandomString(length);
            var hash = BCrypt.Net.BCrypt.HashPassword(secretCode);

            return (secretCode, hash);
        }

        public bool IsPasswordValid(string secretCode, string hash)
        {
            if(string.IsNullOrEmpty(secretCode) || string.IsNullOrEmpty(hash)) return false;
            
            return BCrypt.Net.BCrypt.Verify(secretCode, hash);
        }
    }
}