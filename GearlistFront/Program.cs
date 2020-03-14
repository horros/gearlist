using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Blazor.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Des.Blazor.Authorization.Msal;

namespace GearlistFront
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");
            builder.Services.AddOptions();
            builder.Services.AddAuthorizationCore();
            builder.Services.AddAzureActiveDirectory(
                new Cfg()
            );
            await builder.Build().RunAsync();
        }
    }

    public class Cfg : IMsalConfig
    {
        public string ClientId => "1a2c6297-8aca-450f-bd75-605267d0d0b1";
#if DEBUG
        public string RedirectUri => "https://localhost:5000";
#else
        public string RedirectUri => "https://www.gearlist.cloud";
#endif
        public string Authority => "https://mlgearlist.b2clogin.com/mlgearlist.onmicrosoft.com/B2C_1_signup_signin";
        public LoginModes LoginMode => LoginModes.Redirect;
    }
}
