using Microsoft.JSInterop;
using OnePortal.Blazor.Models;
using OnePortal.Blazor.Services.Http;

namespace OnePortal.Blazor.Services.Auth
{
    public class WebAuthnClient(IJSRuntime js, AuthApi api, ApiClient apiClient)
    {
        private readonly IJSRuntime _js = js;
        private readonly AuthApi _api = api;
        private readonly ApiClient _apiClient = apiClient;


        public async Task<WebAuthnResult> AuthenticateAsync(string email)
        {
            try
            {
                Console.WriteLine($"[WebAuthnClient] Starting authentication for email: {email}");
                
                var start = await _api.WebAuthnBeginAssertionAsync(new WebAuthnStartRequest(email));
                if (start is null) 
                {
                    Console.WriteLine("[WebAuthnClient] Failed to get assertion start response from API");
                    return WebAuthnResult.CreateFailed("Unable to start authentication");
                }

                Console.WriteLine($"[WebAuthnClient] Got assertion start response. PublicKeyOptions type: {start.PublicKeyOptions?.GetType()}");
                Console.WriteLine($"[WebAuthnClient] PublicKeyOptions content: {System.Text.Json.JsonSerializer.Serialize(start.PublicKeyOptions)}");

                // Let the native WebAuthn dialog handle the flow - it will show options for
                // existing passkeys or offer to create a new one
                Console.WriteLine("[WebAuthnClient] Calling webauthn.get with options...");
                var credential = await _js.InvokeAsync<object>("webauthn.get", start.PublicKeyOptions);
                
                if (credential is null) 
                {
                    Console.WriteLine("[WebAuthnClient] webauthn.get returned null - user cancelled or no credential available");
                    return WebAuthnResult.CreateFailed("Authentication cancelled");
                }
                
                Console.WriteLine("[WebAuthnClient] Got credential from webauthn.get, calling finish assertion...");
                var finish = await _api.WebAuthnFinishAssertionAsync(new WebAuthnFinishRequest(email, credential));
                
                if (finish?.AccessToken != null)
                {
                    Console.WriteLine("[WebAuthnClient] Authentication successful!");
                    return WebAuthnResult.CreateSuccess();
                }
                else
                {
                    Console.WriteLine("[WebAuthnClient] Authentication failed - no access token received");
                    return WebAuthnResult.CreateFailed("Authentication failed");
                }
            }
            catch (JSException ex) when (ex.Message.Contains("NotAllowedError"))
            {
                Console.WriteLine($"[WebAuthnClient] NotAllowedError: {ex.Message}");
                return WebAuthnResult.CreateFailed("Authentication cancelled");
            }
            catch (JSException ex) when (ex.Message.Contains("NotSupportedError"))
            {
                Console.WriteLine($"[WebAuthnClient] NotSupportedError: {ex.Message}");
                return WebAuthnResult.CreateFailed("Passkeys not supported on this device");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WebAuthnClient] Unexpected error: {ex}");
                return WebAuthnResult.CreateFailed("Authentication failed");
            }
        }

        public async Task<bool> RegisterAsync(string displayName)
        {
            try
            {
                var start = await _api.WebAuthnBeginRegistrationAsync(new WebAuthnRegistrationRequest(displayName));
                if (start is null) return false;
                
                var credential = await _js.InvokeAsync<object>("webauthn.create", start.PublicKeyOptions);
                if (credential is null) return false;
                
                var finish = await _api.WebAuthnFinishRegistrationAsync(new WebAuthnFinishRequest("", credential));
                return finish?.Ok == true;
            }
            catch (JSException ex) when (ex.Message.Contains("NotAllowedError"))
            {
                // User cancelled or registration failed
                return false;
            }
            catch (JSException ex) when (ex.Message.Contains("NotSupportedError"))
            {
                // WebAuthn not supported
                return false;
            }
            catch
            {
                // Other errors
                return false;
            }
        }

        public async Task<bool> RegisterForLoginAsync(string email, string displayName)
        {
            try
            {
                var start = await _api.WebAuthnBeginRegistrationAsync(new WebAuthnRegistrationRequest(displayName));
                if (start is null) return false;
                
                // The native WebAuthn dialog will appear when we call webauthn.create
                var credential = await _js.InvokeAsync<object>("webauthn.create", start.PublicKeyOptions);
                if (credential is null) return false;
                
                var finish = await _api.WebAuthnFinishRegistrationAsync(new WebAuthnFinishRequest(email, credential));
                return finish?.Ok == true;
            }
            catch (JSException ex) when (ex.Message.Contains("NotAllowedError"))
            {
                // User cancelled or registration failed
                return false;
            }
            catch (JSException ex) when (ex.Message.Contains("NotSupportedError"))
            {
                // WebAuthn not supported
                return false;
            }
            catch (Exception ex)
            {
                // Log other errors for debugging
                Console.Error.WriteLine($"WebAuthn registration error: {ex}");
                return false;
            }
        }

        /// <summary>
        /// GitHub-style passkey authentication with automatic registration fallback
        /// </summary>
        public async Task<WebAuthnResult> AuthenticateWithFallbackAsync(string email, string? displayName = null)
        {
            try
            {
                Console.WriteLine($"[WebAuthnClient] Starting GitHub-style authentication for: {email}");
                
                // Step 1: Begin passkey authentication
                var beginResponse = await _apiClient.PostApiResponseAsync<object>("api/auth/webauthn/passkey/begin", 
                    new { email, displayName });
                
                if (beginResponse == null)
                {
                    Console.WriteLine("[WebAuthnClient] Failed to get passkey options");
                    return WebAuthnResult.CreateFailed("Unable to initialize passkey authentication");
                }

                // Extract options from response
                var responseData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(
                    System.Text.Json.JsonSerializer.Serialize(beginResponse));
                
                var options = responseData["options"];
                var isNewUser = responseData.ContainsKey("isNewUser") && 
                               System.Text.Json.JsonSerializer.Deserialize<bool>(responseData["isNewUser"].ToString()!);
                var hasExistingPasskeys = responseData.ContainsKey("hasExistingPasskeys") && 
                                         System.Text.Json.JsonSerializer.Deserialize<bool>(responseData["hasExistingPasskeys"].ToString()!);

                Console.WriteLine($"[WebAuthnClient] Got options - NewUser: {isNewUser}, HasPasskeys: {hasExistingPasskeys}");

                // Step 2: Use the enhanced JavaScript authentication with fallback
                var credential = await _js.InvokeAsync<object>("passkeyAuth.authenticateWithFallback", 
                    options, email, displayName ?? email);

                if (credential == null)
                {
                    Console.WriteLine("[WebAuthnClient] Authentication was cancelled");
                    return WebAuthnResult.CreateFailed("Authentication cancelled");
                }

                // Step 3: Complete the authentication
                var completeResponse = await _apiClient.PostApiResponseAsync<object>("api/auth/webauthn/passkey/complete", 
                    new { email, credentialResponse = credential });

                if (completeResponse == null)
                {
                    Console.WriteLine("[WebAuthnClient] Failed to complete authentication");
                    return WebAuthnResult.CreateFailed("Authentication failed");
                }

                // Extract login result
                var completeData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(
                    System.Text.Json.JsonSerializer.Serialize(completeResponse));
                
                if (completeData.ContainsKey("loginResult"))
                {
                    var loginResult = System.Text.Json.JsonSerializer.Deserialize<LoginResponse>(
                        System.Text.Json.JsonSerializer.Serialize(completeData["loginResult"]));
                    
                    if (loginResult?.AccessToken != null)
                    {
                        Console.WriteLine("[WebAuthnClient] Authentication successful!");
                        return WebAuthnResult.CreateSuccess();
                    }
                }

                Console.WriteLine("[WebAuthnClient] No access token received");
                return WebAuthnResult.CreateFailed("Authentication failed - no access token");
            }
            catch (JSException ex) when (ex.Message.Contains("NotAllowedError"))
            {
                Console.WriteLine($"[WebAuthnClient] NotAllowedError: {ex.Message}");
                return WebAuthnResult.CreateFailed("Authentication was cancelled");
            }
            catch (JSException ex) when (ex.Message.Contains("NotSupportedError"))
            {
                Console.WriteLine($"[WebAuthnClient] NotSupportedError: {ex.Message}");
                return WebAuthnResult.CreateFailed("Passkeys are not supported on this device");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WebAuthnClient] Unexpected error: {ex}");
                return WebAuthnResult.CreateFailed($"Authentication failed: {ex.Message}");
            }
        }
    }
}
