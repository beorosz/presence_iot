using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Presence.Notifiation
{
  public static class PresenceChangeNotificationTrigger
  {
    [FunctionName("PresenceChangeNotificationTrigger")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
        [Queue("queue"), StorageAccount("AzureWebJobsStorage")] ICollector<string> queue,
        ILogger log)
    {
      log.LogInformation("C# HTTP trigger function processed a request.");

      string validationToken = req.Query["validationToken"];

      log.LogInformation("validationToken: " + validationToken);

      string data = await new StreamReader(req.Body).ReadToEndAsync();

      log.LogInformation("Body of request: " + data);

      queue.Add(data);

      log.LogInformation("Added message to queue");

      return new OkResult();
    }
  }
}
