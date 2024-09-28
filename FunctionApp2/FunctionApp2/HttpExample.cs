using FunctionApp2.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FunctionApp2
{
    public class HttpExample
    {
        private readonly ILogger<HttpExample> _logger;

        public HttpExample(ILogger<HttpExample> logger)
        {
            _logger = logger;
        }

        [Function("CreateTask")]
        public async Task<IActionResult> CreateTask(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
        {
            _logger.LogInformation("Processing POST request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            var data = JsonConvert.DeserializeObject<TaskModel>(requestBody);

            if (data == null || string.IsNullOrEmpty(data.Name))
            {
                return new BadRequestObjectResult("Invalid input");
            }

            return new OkObjectResult($"Task {data.Name} created successfully.");
        }

        [Function("GetTask")]
        public async Task<IActionResult> GetTask(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req)
        {
            _logger.LogInformation("Processing GET request.");

            string name = req.Query["name"];

            if (string.IsNullOrEmpty(name))
            {
                return new BadRequestObjectResult("Please provide 'name' parameter.");
            }

            var task = new TaskModel("Carlos");

            return new OkObjectResult($"Task {task.Name} {name} retrieved successfully.");
        }

        [Function("Timer")]
        public void Run([TimerTrigger("0 */1 * * * *")] TimerInfo myTimer)
        {
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            if (myTimer.ScheduleStatus is not null)
            {
                _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
            }
        }
    }
}
