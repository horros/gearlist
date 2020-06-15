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
using System.Collections.Generic;

namespace AzFunctions.Functions
{
    public static class DeleteImage
    {
        [FunctionName("DeleteImage")]
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
                return new HttpResponseMessage(statusCode: HttpStatusCode.Unauthorized);
            }

            string item = req.Query["id"];

            if (item == null)
            {
                return new HttpResponseMessage(statusCode: HttpStatusCode.NotFound);
            }

            string image = req.Query["image"];

            if (image == null)
            {
                return new HttpResponseMessage(statusCode: HttpStatusCode.BadRequest);
            }

            var options = new FeedOptions { EnableCrossPartitionQuery = true, PartitionKey = new PartitionKey(sub) };

            // Fetch the item from CosmosDB
            Uri collUri = UriFactory.CreateDocumentCollectionUri("gear", "gear");
            Document doc = documentClient.CreateDocumentQuery(collUri, options).
                                           Where(d => d.Id == item).
                                           AsEnumerable().
                                           Single();

            // Owner matches the JWT subject
            if (doc != null && doc.GetPropertyValue<string>("Owner") == sub) {

                string gearid = doc.GetPropertyValue<string>("GearId");
                List<string> images = doc.GetPropertyValue<List<string>>("Images");

                var blob = container.GetBlockBlobReference($"{sub}/{gearid}/{image}");
                if (blob != null)
                {

                    blob.DeleteIfExists();
                    images.Remove(image);
                    doc.SetPropertyValue("Images", images);
                }


                // Update the document itself
                await documentClient.UpsertDocumentAsync(collUri, doc);
                return new HttpResponseMessage(statusCode: HttpStatusCode.NoContent);
                
            } 
            else
            {
                return new HttpResponseMessage(statusCode: HttpStatusCode.NotFound);
            }

        }        
    }
}
