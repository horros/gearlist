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
using System.Security.Claims;
using System.Collections.Generic;
using AzFunctions.Model;
using System.Linq;
using System.Net;
using Microsoft.Azure.Documents;

namespace AzFunctions.Functions
{
    public static class DeleteItem
    {
        [FunctionName("DeleteItem")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = null)] HttpRequest req,
            [CosmosDB(
                databaseName: "gear",
                collectionName: "gear",
                ConnectionStringSetting = "COSMOSDB_CONNECTION_STRING")] DocumentClient documentClient,
            ILogger log)
        {

            ClaimsPrincipal principal;
            var hdr = req.Headers["Authorization"];
            var token = hdr.ToString().Replace("Bearer ", "");
            var auth = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            if ((principal = await Security.ValidateTokenAsync(auth)) == null)
            {
                return new UnauthorizedResult();
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
                return new UnauthorizedResult();
            }

            string item = req.Query["id"];

            if (item == null)
            {
                return new NotFoundResult();
            }

            var options = new FeedOptions { EnableCrossPartitionQuery = true };

            Uri collUri = UriFactory.CreateDocumentCollectionUri("gear", "gear");
            Document doc = documentClient.CreateDocumentQuery(collUri, options).
                                           Where(d => d.Id == item).
                                           AsEnumerable().
                                           Single();

            if (doc != null && doc.GetPropertyValue<string>("Owner") == sub) {

                await documentClient.DeleteDocumentAsync(doc.SelfLink,
                    new RequestOptions { PartitionKey = new PartitionKey(sub) });
                return new OkResult();
                
            } 
            else
            {
                return new NotFoundResult();
            }

        }
    }
}
