using Microsoft.Extensions.Configuration;
using OnePortal.Application.Abstractions;

namespace OnePortal.Infrastructure.Configuration
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly IConfiguration _configuration;

        public ConfigurationService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GetResetPasswordBaseUrl()
        {
            return _configuration["Auth:ResetBaseUrl"] ?? "https://app.oneportal.local/reset";
        }
    }
}
