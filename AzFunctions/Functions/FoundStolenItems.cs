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
using AzFunctions.Model;
using System.Text.Json;
using System.Text;
using System.Collections.Generic;

namespace AzFunctions.Functions
{
    public static class FoundStolenItems
    {
        [FunctionName("FoundStolenItems")]
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

            var options = new FeedOptions { EnableCrossPartitionQuery = true, PartitionKey = new PartitionKey(subClaim.Value) };

            // Fetch the item from CosmosDB
            Uri collUri = UriFactory.CreateDocumentCollectionUri("gear", "found");
            try
            {
                IEnumerable<StolenItemFound> Items = documentClient.CreateDocumentQuery<StolenItemFound>(collUri, options)
                                                    .Where(g => g.Owner == subClaim.Value).AsEnumerable();
                var jsonOpts = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = null
                };

                var returnValue = System.Text.Json.JsonSerializer.Serialize(Items, jsonOpts);

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(returnValue, Encoding.UTF8, @"application/json"),
                };


            }
            catch (Exception)
            {
                return new HttpResponseMessage(statusCode: HttpStatusCode.NotFound);
            }

            // Owner matches the JWT subject
        }
    }
}
