using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Net.Http;
using MatBlazor;

namespace GearlistFront
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");
            builder.Services.AddTransient(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddOptions();
            builder.Services.AddAuthorizationCore();
            builder.Services.AddSingleton<GearlistFront.Model.AppData>();


            builder.Services.AddMsalAuthentication(options =>
            {
                var authentication = options.ProviderOptions.Authentication;
                authentication.Authority = "https://gearlist.b2clogin.com/gearlist.onmicrosoft.com/B2C_1_susi";
                authentication.ClientId = "f497bee3-a7ef-4984-a564-41206d334596";
                authentication.ValidateAuthority = false;
                options.ProviderOptions.DefaultAccessTokenScopes.Add(
                    "https://gearlist.onmicrosoft.com/gearlist/api"
                );
            });
            

            builder.Services.AddMatToaster(config =>
                {
                    config.Position = MatToastPosition.TopRight;
                    config.PreventDuplicates = true;
                    config.NewestOnTop = true;
                    config.ShowCloseButton = true;
                    config.MaximumOpacity = 95;
                    config.VisibleStateDuration = 2500;
                });

            await builder.Build().RunAsync();
        }
    }

}
