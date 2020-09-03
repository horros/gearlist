using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using AzFunctions.Model;
using System.Collections.Generic;
using Microsoft.Azure.Documents.Client;
using System.Linq;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Net.Http;
using System.Net;
using System.Text;

namespace AzFunctions
{
    public static class ListGear
    {
        /// <summary>
        /// Get a list of gear from Cosmos DB and return it to the front end
        /// as a JSON-serialised GearModel -object
        /// </summary>
        /// <param name="req">The HTTP Request, we don't need that here</param>
        /// <param name="documentClient">The Cosmos DB connection</param>
        /// <param name="log">The Logger</param>
        /// <returns></returns>
        [FunctionName("ListGear")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            [CosmosDB(
                databaseName: "gear",
                collectionName: "gear",
                ConnectionStringSetting = "COSMOSDB_CONNECTION_STRING")] DocumentClient documentClient,
            ILogger log)
        {

            /**
             * Slightly wonky, but this gets the raw JWT Bearer from the Authorization header,
             * constructs an AuthenticationHeaderValue and checks against AAD B2C that the JWT
             * is valid
             **/
            ClaimsPrincipal principal;
            var hdr = req.Headers["Authorization"];
            var token = hdr.ToString().Replace("Bearer ", "");
            var auth = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            if ((principal = await Security.ValidateTokenAsync(auth)) == null)
            {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized);
                
            }
            // Get the sub (subject) claim from the principal, 
            // this is an AAD B2C GUID that identifies the user

            var subClaim = principal.Claims.Where(c => c.Type == "sub").FirstOrDefault();

            if (subClaim == null)
            {
                return new HttpResponseMessage(statusCode: HttpStatusCode.Unauthorized);
            }

            Uri gearCollectionUri = UriFactory.CreateDocumentCollectionUri("gear", "gear");
            var options = new FeedOptions { EnableCrossPartitionQuery = true };
            IEnumerable<GearModel> gearmodels = documentClient.CreateDocumentQuery<GearModel>(gearCollectionUri, options)
                                                .Where(g => g.Owner == subClaim.Value).AsEnumerable();

            // Force the JSON serializer to not change PascalCase names
            // to camelCase just to make it easier with the frontend
            var jsonOpts = new JsonSerializerOptions
            {
                PropertyNamingPolicy = null
            };

            var returnValue = JsonSerializer.Serialize(gearmodels, jsonOpts);

            return new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent(returnValue, Encoding.UTF8, @"application/json"),               
            };

        }

    }
}
