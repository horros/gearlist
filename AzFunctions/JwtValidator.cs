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
            var issuer = "https://gearlist.b2clogin.com/gearlist.onmicrosoft.com/B2C_1_signup_signin"; 

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
            var issuer = "https://gearlist.b2clogin.com/gearlist.onmicrosoft.com/B2C_1_signup_signin";

            var validationParameter = new TokenValidationParameters()
            {
                RequireSignedTokens = true,
                ValidIssuer = issuer,
                ValidateIssuer = false,
                ValidAudience = "f497bee3-a7ef-4984-a564-41206d334596",
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                IssuerSigningKeys = config.SigningKeys,
                NameClaimType = "name"
            };

            ClaimsPrincipal result = null;
            var tries = 0;

            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;


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
                }
            }

            return result;
        }
    }
}