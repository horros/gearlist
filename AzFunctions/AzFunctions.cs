using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Security.Claims;

namespace AzFunctions
{
    public static class AzFunctions
    {
        [FunctionName("ListGear")]
        public static async Task<IActionResult> Run(
            [HttpTrigger("get", "post", Route = null)] HttpRequest req, ILogger log)
        {
            ClaimsPrincipal principal;
            var hdr = req.Headers["Authorization"];
            var token = hdr.ToString().Replace("Bearer ", "");
            var auth = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            
            

            if ((principal = await Security.ValidateTokenAsync(auth)) == null)
            {
                return new UnauthorizedResult();
            }
            // Authentication boilerplate code end

            return new OkObjectResult("Hello, " + principal.Identity.Name + " you sent " + req.Body.ToString());
        }
    }
}
