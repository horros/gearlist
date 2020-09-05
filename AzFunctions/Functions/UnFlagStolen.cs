using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Documents.Client;
using System.Security.Claims;
using System.Linq;
using System.Net;
using Microsoft.Azure.Documents;
using System.Net.Http;
using AzFunctions.Model;

namespace AzFunctions.Functions
{
    public static class UnFlagStolen
    {
        [FunctionName("UnFlagStolen")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = null)] HttpRequest req,
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

            string item = req.Query["id"];
            string serial = req.Query["serial"];

            if (item == null)
            {
                return new HttpResponseMessage(statusCode: HttpStatusCode.NotFound);
            }

            var options = new FeedOptions { EnableCrossPartitionQuery = true, PartitionKey = new PartitionKey(serial) };

            // Fetch the item from CosmosDB
            Uri collUri = UriFactory.CreateDocumentCollectionUri("gear", "stolen");
            StolenItem stolenItem = documentClient.CreateDocumentQuery<StolenItem>(collUri, options).
                                           Where(d => d.id == item).
                                           Where(d => d.Owner == subClaim.Value).
                                           AsEnumerable().
                                           Single();

            var foundOptions = new FeedOptions { EnableCrossPartitionQuery = true, PartitionKey = new PartitionKey(subClaim.Value) };

            Uri collUri2 = UriFactory.CreateDocumentCollectionUri("gear", "found");

            StolenItemFound foundItem = documentClient.CreateDocumentQuery<StolenItemFound>(collUri2, foundOptions).
                                           Where(d => d.Serial == serial).
                                           Where(d => d.Owner == subClaim.Value).
                                           AsEnumerable().
                                           Single();

            if (stolenItem != null) {

                var stolenItemURI = UriFactory.CreateDocumentUri("gear", "stolen", stolenItem.id);
                await documentClient.DeleteDocumentAsync(stolenItemURI, new RequestOptions { PartitionKey = new PartitionKey(serial) });

                if (foundItem != null)
                {
                    var foundItemURI = UriFactory.CreateDocumentUri("gear", "found", foundItem.id);
                    await documentClient.DeleteDocumentAsync(foundItemURI, new RequestOptions { PartitionKey = new PartitionKey(subClaim.Value) });
                }

                return new HttpResponseMessage(statusCode: HttpStatusCode.NoContent);
                
            } 
            else
            {
                return new HttpResponseMessage(statusCode: HttpStatusCode.NotFound);
            }

        }        
    }
}
