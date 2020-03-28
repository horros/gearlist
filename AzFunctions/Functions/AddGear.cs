using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Security.Claims;
using AzFunctions.Model;
using Microsoft.Azure.WebJobs.Extensions.CosmosDB;

namespace AzFunctions
{
    public static class AddGear
    {

        [FunctionName("AddGear")]
        public static async Task<IActionResult> Run(
            [HttpTrigger("post", Route = null)] HttpRequest req,
            [CosmosDB(
                databaseName: "gear",
                collectionName: "gear",
                ConnectionStringSetting = "COSMOSDB_CONNECTION_STRING")] IAsyncCollector<object> gear,
            ILogger log)
        {
 
            ClaimsPrincipal principal;
            var hdr = req.Headers["Authorization"];
            var token = hdr.ToString().Replace("Bearer ", "");

            if (String.IsNullOrEmpty(token))
            {
                return new UnauthorizedResult();
            }

            var auth = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            
            if ((principal = await Security.ValidateTokenAsync(auth)) == null)
            {
                return new UnauthorizedResult();
            }
            string sub = null;
            foreach (var claim in principal.Claims)
            {
                if (claim.Type == "sub")
                {
                    sub = claim.Value;
                }
            }

            if (sub == null)
            {
                return new UnauthorizedResult();
            }

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            var input = JsonConvert.DeserializeObject<GearModel>(requestBody);
            input.Owner = sub;

            await gear.AddAsync(input);

            return new OkResult();
        }

    }
}
