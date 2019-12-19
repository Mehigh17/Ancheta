namespace Ancheta.Model.Services
{
    public interface IPollService
    {
        bool IsPasswordValid(string secretCode, string hash);
        (string, string) GenerateSecretCode(int length = 8);
    }
}