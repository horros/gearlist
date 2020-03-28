using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace GearlistFront
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");
            builder.Services.AddBaseAddressHttpClient();
            builder.Services.AddOptions();
            builder.Services.AddAuthorizationCore();
            builder.Services.AddMsalAuthentication(options =>
            {
                var authentication = options.ProviderOptions.Authentication;
                authentication.Authority = "https://mlgearlist.b2clogin.com/mlgearlist.onmicrosoft.com/B2C_1_signup_signin";
                authentication.ClientId = "1a2c6297-8aca-450f-bd75-605267d0d0b1";
                authentication.ValidateAuthority = false;
                options.ProviderOptions.DefaultAccessTokenScopes.Add(
                    "https://mlgearlist.onmicrosoft.com/1a2c6297-8aca-450f-bd75-605267d0d0b1/api");
            });
            

            await builder.Build().RunAsync();
        }
    }

}
