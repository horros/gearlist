using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Documents.Client;
using System.Net.Http;
using System.Security.Claims;
using System.Net;
using System.Linq;
using Microsoft.Azure.Documents;
using GearlistFront.Model;
using System.Text.Json;
using System.Text;

namespace AzFunctions.Functions
{
    public static class GetStolenItem
    {
        [FunctionName("GetStolenItem")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            [CosmosDB(
                databaseName: "gear",
                collectionName: "stolen",
                ConnectionStringSetting = "COSMOSDB_CONNECTION_STRING")] DocumentClient documentClient,
            ILogger log)
        {
            ClaimsPrincipal principal;
            var hdr = req.Headers["Authorization"];
            var token = hdr.ToString().Replace("Bearer ", "");
            var auth = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            if ((principal = await Security.ValidateTokenAsync(auth)) == null)
            {
                return new HttpResponseMessage(statusCode: HttpStatusCode.Unauthorized);
            }
            // Get the sub (subject) claim from the principal, 
            // this is an AAD B2C GUID that identifies the user
            var subClaim = principal.Claims.Where(c => c.Type == "sub").FirstOrDefault();

            if (subClaim == null)
            {
                return new HttpResponseMessage(statusCode: HttpStatusCode.Unauthorized);
            }

            string serial = req.Query["serial"];
            string gearId = req.Query["gearId"];

            if (serial == null)
            {
                return new HttpResponseMessage(statusCode: HttpStatusCode.NotFound);
            }

            var options = new FeedOptions { EnableCrossPartitionQuery = true, PartitionKey = new PartitionKey(serial) };

            // Fetch the item from CosmosDB
            Uri collUri = UriFactory.CreateDocumentCollectionUri("gear", "stolen");
            try
            {
                StolenItem doc = documentClient.CreateDocumentQuery<StolenItem>(collUri, options).
                                           Where(d => d.Owner == subClaim.Value && d.ItemRef == gearId).
                                           AsEnumerable().
                                           SingleOrDefault();

                String returnValue = null;
                if (doc != null)
                {
                    // Force the JSON serializer to not change PascalCase names
                    // to camelCase just to make it easier with the frontend
                    var jsonOpts = new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = null
                    };

                    returnValue = System.Text.Json.JsonSerializer.Serialize(doc, jsonOpts);
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(returnValue, Encoding.UTF8, @"application/json")
                    };
                } else
                {
                    return new HttpResponseMessage(HttpStatusCode.NoContent);
                }

                
                

            }
            catch (Exception e)
            {
                return new HttpResponseMessage(statusCode: HttpStatusCode.NotFound);
            }

           
        }
    }
}
