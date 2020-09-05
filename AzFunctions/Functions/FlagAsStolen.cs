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
using System.Net.Http;
using System.Net;
using System.Linq;

namespace AzFunctions.Functions
{
    public static class FlagAsStolen
    {
        [FunctionName("FlagAsStolen")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            [CosmosDB(
                databaseName: "gear",
                collectionName: "stolen",
                ConnectionStringSetting = "COSMOSDB_CONNECTION_STRING")] IAsyncCollector<object> stolenItem,
            ILogger log)
        {
            var hdr = req.Headers["Authorization"];
            var token = hdr.ToString().Replace("Bearer ", "");

            if (String.IsNullOrEmpty(token))
            {
                return new HttpResponseMessage(statusCode: HttpStatusCode.Unauthorized);
            }

            var auth = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            if (await Security.ValidateTokenAsync(auth) == null)
            {
                return new HttpResponseMessage(statusCode: HttpStatusCode.Unauthorized);
            }

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            var input = JsonConvert.DeserializeObject<dynamic>(requestBody);

            await stolenItem.AddAsync(input);

            return new HttpResponseMessage(statusCode: HttpStatusCode.NoContent);
        }
    }
}
