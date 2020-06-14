using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Security.Claims;
using AzFunctions.Model;
using System.Net.Http;
using System.Net;


namespace AzFunctions.Functions
{
    public static class EditGear
    {
        [FunctionName("EditGear")]
        public static async Task<HttpResponseMessage> Run(
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
                return new HttpResponseMessage(statusCode: HttpStatusCode.Unauthorized);
            }

            var auth = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            if ((principal = await Security.ValidateTokenAsync(auth)) == null)
            {
                return new HttpResponseMessage(statusCode: HttpStatusCode.Unauthorized);
            }
            string sub = null;
            foreach (var claim in principal.Claims)
            {
                if (claim.Type == "sub")
                {
                    sub = claim.Value;
                    break;
                }
            }

            if (sub == null)
            {
                return new HttpResponseMessage(statusCode: HttpStatusCode.Unauthorized);
            }

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            // This needs to be a dynamic object, because apparently IAsyncCollectior doesn't
            // deserialize the POCO object as it should, thus using "Id" instead of "id"
            // causing upserts not to work
            var input = JsonConvert.DeserializeObject<dynamic>(requestBody);

            if (input.Owner != sub)
            {
                return new HttpResponseMessage(statusCode: HttpStatusCode.Unauthorized);
            }

            await gear.AddAsync(input);

            return new HttpResponseMessage(statusCode: HttpStatusCode.NoContent);
        }
    }
}
