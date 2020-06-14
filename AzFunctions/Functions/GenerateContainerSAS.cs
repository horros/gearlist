using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Storage.Blob;


namespace AzFunctions
{
    public static class GenerateContainerSAS
    {
        [FunctionName("GenerateContainerSAS")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            [Blob("images", Connection = "IMAGE_STORAGE")] CloudBlobContainer gearimages,
            ILogger log)
        {
            var permissions = SharedAccessBlobPermissions.Write;

            var sasToken = ContainerSASToken.GetContainerSasToken(gearimages, permissions);
            return new OkObjectResult(new
            {
                token = sasToken,
                uri = gearimages.Uri + sasToken
            });
        }
    }
}
