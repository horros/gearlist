using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace AzFunctions
{
    public static class Security
    {
        private static readonly IConfigurationManager<OpenIdConnectConfiguration> _configurationManager;

        static Security()
        {
            var issuer = "https://mlgearlist.b2clogin.com/mlgearlist.onmicrosoft.com/B2C_1_signup_signin"; 

            var documentRetriever = new HttpDocumentRetriever();
            documentRetriever.RequireHttps = true;


            _configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                $"{issuer}/.well-known/openid-configuration",
                new OpenIdConnectConfigurationRetriever(),
                documentRetriever
            );
        }

        public static async Task<ClaimsPrincipal> ValidateTokenAsync(AuthenticationHeaderValue value)
        {
            if (value?.Scheme != "Bearer")
            {
                return null;
            }

            var config = await _configurationManager.GetConfigurationAsync(CancellationToken.None);
            var issuer = "https://mlgearlist.b2clogin.com/mlgearlist.onmicrosoft.com/B2C_1_signup_signin";
            //var audience = Environment.GetEnvironmentVariable("AUDIENCE");

            var validationParameter = new TokenValidationParameters()
            {
                RequireSignedTokens = true,
                ValidIssuer = issuer,
                ValidateIssuer = false,
                ValidAudience = "1a2c6297-8aca-450f-bd75-605267d0d0b1",
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                IssuerSigningKeys = config.SigningKeys,
                // This claim is in the Azure AD B2C token; this code tells the web app to "absorb" the token "name" and place it in the user object
                NameClaimType = "name"
                
            };

            ClaimsPrincipal result = null;
            var tries = 0;

            while (result == null && tries <= 1)
            {
                try
                {
                    var handler = new JwtSecurityTokenHandler();
                    result = handler.ValidateToken(value.Parameter, validationParameter, out var token);
                }
                catch (SecurityTokenSignatureKeyNotFoundException)
                {
                    // This exception is thrown if the signature key of the JWT could not be found.
                    // This could be the case when the issuer changed its signing keys, so we trigger a 
                    // refresh and retry validation.
                    _configurationManager.RequestRefresh();
                    tries++;
                }
                catch (SecurityTokenException e)
                {
                    throw e;
                    return null;
                }
            }

            return result;
        }
    }
}