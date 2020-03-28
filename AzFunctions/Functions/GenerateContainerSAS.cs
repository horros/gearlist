using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Configuration;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage;
using Microsoft.Azure;

namespace AzFunctions
{
    public static class GenerateContainerSAS
    {
        [FunctionName("GenerateContainerSAS")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            [Blob("images", Connection ="IMAGE_STORAGE")]CloudBlobContainer gearimages,
            ILogger log)
        {
            var permissions = SharedAccessBlobPermissions.Write;
            
            //var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("IMAGE_STORAGE"));           
            //var blobClient = storageAccount.CreateCloudBlobClient();
            //var gearimages = blobClient.GetContainerReference("gearimages");
            var sasToken = GetContainerSasToken(gearimages, permissions);
            return new OkObjectResult(new
            {
                token = sasToken,
                uri = gearimages.Uri + sasToken
            });
        }

        public static string GetContainerSasToken(CloudBlobContainer container, SharedAccessBlobPermissions permissions, string storedPolicyName = null)
        {
            string sasContainerToken;

            // If no stored policy is specified, create a new access policy and define its constraints.
            if (storedPolicyName == null)
            {
                var adHocSas = CreateAdHocSasPolicy(permissions);

                // Generate the shared access signature on the container, setting the constraints directly on the signature.
                sasContainerToken = container.GetSharedAccessSignature(adHocSas, null);
            }
            else
            {
                // Generate the shared access signature on the container. In this case, all of the constraints for the
                // shared access signature are specified on the stored access policy, which is provided by name.
                // It is also possible to specify some constraints on an ad-hoc SAS and others on the stored access policy.
                // However, a constraint must be specified on one or the other; it cannot be specified on both.
                sasContainerToken = container.GetSharedAccessSignature(null, storedPolicyName);
            }

            return sasContainerToken;
        }

        private static SharedAccessBlobPolicy CreateAdHocSasPolicy(SharedAccessBlobPermissions permissions)
        {
            // Create a new access policy and define its constraints.
            // Note that the SharedAccessBlobPolicy class is used both to define the parameters of an ad-hoc SAS, and 
            // to construct a shared access policy that is saved to the container's shared access policies. 

            return new SharedAccessBlobPolicy()
            {
                // Set start time to five minutes before now to avoid clock skew.
                SharedAccessStartTime = DateTime.UtcNow.AddMinutes(-5),
                SharedAccessExpiryTime = DateTime.UtcNow.AddHours(1),
                Permissions = permissions
            };
        }

    }
}
