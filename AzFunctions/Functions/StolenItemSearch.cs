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

namespace AzFunctions.Functions
{
    public static class StolenItemSearch
    {
        [FunctionName("StolenItemSearch")]
        public static HttpResponseMessage Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            [CosmosDB(
                databaseName: "gear",
                collectionName: "stolen",
                ConnectionStringSetting = "COSMOSDB_CONNECTION_STRING")] DocumentClient documentClient,
            ILogger log)
        {
            
            string serial = req.Query["serial"];

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
                                           Where(d => d.Serial == serial).
                                           AsEnumerable().
                                           Single();
                if (doc != null)
                {
                    // Force the JSON serializer to not change PascalCase names
                    // to camelCase just to make it easier with the frontend
                    var jsonOpts = new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = null
                    };

                    var returnValue = System.Text.Json.JsonSerializer.Serialize(doc, jsonOpts);

                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(returnValue, Encoding.UTF8, @"application/json"),
                    };
                }
                else
                {
                    return new HttpResponseMessage(statusCode: HttpStatusCode.NoContent);
                }


            }
            catch (Exception)
            {
                return new HttpResponseMessage(statusCode: HttpStatusCode.NoContent);
            }

            // Owner matches the JWT subject
        }
    }
}
