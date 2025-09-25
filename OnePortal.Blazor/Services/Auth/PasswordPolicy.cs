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

        public static PasswordValidationResult ValidatePassword(string password, int minLen = 8)
        {
            var result = new PasswordValidationResult();
            
            if (string.IsNullOrWhiteSpace(password))
            {
                result.IsValid = false;
                result.Message = "Password is required";
                return result;
            }

            if (password.Length < minLen)
            {
                result.IsValid = false;
                result.Message = $"Password must be at least {minLen} characters long";
                return result;
            }

            if (!Regex.IsMatch(password, "[A-Z]"))
            {
                result.IsValid = false;
                result.Message = "Password must contain at least one uppercase letter (A-Z)";
                return result;
            }

            if (!Regex.IsMatch(password, "[a-z]"))
            {
                result.IsValid = false;
                result.Message = "Password must contain at least one lowercase letter (a-z)";
                return result;
            }

            if (!Regex.IsMatch(password, "[0-9]"))
            {
                result.IsValid = false;
                result.Message = "Password must contain at least one number (0-9)";
                return result;
            }

            if (!Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':"",.<>/?`~|]"))
            {
                result.IsValid = false;
                result.Message = "Password must contain at least one special character (!@#$%^&*()_+-=[]{}|;':\",./<>?`~)";
                return result;
            }

            result.IsValid = true;
            result.Message = "Password meets all requirements";
            return result;
        }
    }

    public class PasswordValidationResult
    {
        public bool IsValid { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
