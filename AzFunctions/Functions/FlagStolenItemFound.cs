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
using Newtonsoft.Json;

namespace AzFunctions.Functions
{
    public static class FlagStolenItemFound
    {
        [FunctionName("FlagStolenItemFound")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            [CosmosDB(
                databaseName: "gear",
                collectionName: "found",
                ConnectionStringSetting = "COSMOSDB_CONNECTION_STRING")] IAsyncCollector<object> stolenItemFound,
            ILogger log)
        {

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            var input = JsonConvert.DeserializeObject<dynamic>(requestBody);
            
            await stolenItemFound.AddAsync(input);

            return new HttpResponseMessage(statusCode: HttpStatusCode.NoContent);



        }        
    }
}
