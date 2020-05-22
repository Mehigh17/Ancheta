namespace Ancheta.Model.Services
{
    public interface IPollService
    {
        
        /// <summary>
        /// Verifies whether the code is valid given a hash. 
        /// </summary>
        /// <param name="secretCode">The code to verify.</param>
        /// <param name="hash">The presumed code hash.</param>
        /// <returns>A boolean indicating whether the code is valid.</returns>
        bool IsCodeValid(string secretCode, string hash);
        
        /// <summary>
        /// Generate a random secret code.
        /// </summary>
        /// <param name="length">The length in characters of the code.</param>
        /// <returns>A tuple containing the secret code in plain text and then its hash.</returns>
        (string, string) GenerateSecretCode(int length = 8);
    }
}
