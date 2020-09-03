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
using Microsoft.Azure.Storage.Blob;

namespace AzFunctions.Functions
{
    public static class DeleteGear
    {
        [FunctionName("DeleteGear")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = null)] HttpRequest req,
            [CosmosDB(
                databaseName: "gear",
                collectionName: "gear",
                ConnectionStringSetting = "COSMOSDB_CONNECTION_STRING")] DocumentClient documentClient,
            [Blob("images", Connection = "IMAGE_STORAGE")] CloudBlobContainer container,
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

            if (item == null)
            {
                return new HttpResponseMessage(statusCode: HttpStatusCode.NotFound);
            }

            var options = new FeedOptions { EnableCrossPartitionQuery = true, PartitionKey = new PartitionKey(subClaim.Value) };

            // Fetch the item from CosmosDB
            Uri collUri = UriFactory.CreateDocumentCollectionUri("gear", "gear");
            Document doc = documentClient.CreateDocumentQuery(collUri, options).
                                           Where(d => d.Id == item).
                                           AsEnumerable().
                                           Single();

            // Owner matches the JWT subject
            if (doc != null && doc.GetPropertyValue<string>("Owner") == subClaim.Value) {

                string gearid = doc.GetPropertyValue<string>("GearId");

                // Find all images attached to this gear and delete them
                var ctoken = new BlobContinuationToken();
                do
                {
                    var result = await container.ListBlobsSegmentedAsync($"{subClaim.Value}/{gearid}", true, BlobListingDetails.None, null, ctoken, null, null);
                    ctoken = result.ContinuationToken;
                    await Task.WhenAll(result.Results
                        .Select(item => (item as CloudBlob)?.DeleteIfExistsAsync())
                        .Where(task => task != null)
                    );
                } while (ctoken != null);


                // Delete the document itself
                await documentClient.DeleteDocumentAsync(doc.SelfLink,
                    new RequestOptions { PartitionKey = new PartitionKey(subClaim.Value) });
                return new HttpResponseMessage(statusCode: HttpStatusCode.NoContent);
                
            } 
            else
            {
                return new HttpResponseMessage(statusCode: HttpStatusCode.NotFound);
            }

        }        
    }
}
