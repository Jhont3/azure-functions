using FunctionApp2.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Data.SqlClient;
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

            string connectionString = Environment.GetEnvironmentVariable("SqlConnectionString");
            int newId;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var query = "INSERT INTO Tasks (Name) OUTPUT INSERTED.Id VALUES (@name)";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@name", data.Name);
                    newId = (int)await cmd.ExecuteScalarAsync();  // Obtener el ID generado
                }
            }

            // Devolver el nuevo objeto TaskModel con el ID generado
            return new OkObjectResult(new TaskModel(newId, data.Name));
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

            string connectionString = Environment.GetEnvironmentVariable("SqlConnectionString");
            TaskModel task = null;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var query = "SELECT Id, Name FROM Tasks WHERE Name = @name";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@name", name);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (reader.Read())
                        {
                            task = new TaskModel((int)reader["Id"], reader["Name"].ToString());
                        }
                    }
                }
            }

            if (task == null)
            {
                return new NotFoundObjectResult("Task not found.");
            }

            return new OkObjectResult($"Task {task.Name} with ID {task.Id} retrieved successfully.");
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
