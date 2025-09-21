using System.Text.RegularExpressions;

namespace OnePortal.Blazor.Services.Auth
{
    public static class PasswordPolicy
    {
        public static bool MeetsPolicy(string password, int minLen = 8) =>
        !string.IsNullOrWhiteSpace(password)
        && password.Length >= minLen
        && Regex.IsMatch(password, "[A-Z]")
        && Regex.IsMatch(password, "[a-z]")
        && Regex.IsMatch(password, "[0-9]")
       && Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':"",.<>/?`~|]");
    }
}
