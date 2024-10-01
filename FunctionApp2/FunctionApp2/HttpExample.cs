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
        private static int i = 0;

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
                    newId = (int)await cmd.ExecuteScalarAsync();
                }
            }

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

        [Function("GetAllTasks")]
        public async Task<IActionResult> GetAllTasks(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req)
        {
            _logger.LogInformation("Processing GET request to retrieve all tasks.");

            string connectionString = Environment.GetEnvironmentVariable("SqlConnectionString");
            List<TaskModel> tasks = new List<TaskModel>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var query = "SELECT Id, Name FROM Tasks";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                            var task = new TaskModel((int)reader["Id"], reader["Name"].ToString());
                            tasks.Add(task);
                        }
                    }
                }
            }

            if (tasks.Count == 0)
            {
                return new NotFoundObjectResult("No tasks found.");
            }

            return new OkObjectResult(tasks);
        }

        [Function("Timer")]
        public async Task Run([TimerTrigger("*/10 * * * * *")] TimerInfo myTimer)
        {
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            if (myTimer.ScheduleStatus is not null)
            {
                _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
            }

            string connectionString = Environment.GetEnvironmentVariable("SqlConnectionString");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var query = "INSERT INTO Tasks (Name) VALUES (@name)";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@name", $"taskTest{i++}");
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            _logger.LogInformation($"Inserted new task: taskTest{i - 1}");
        }
    }
}
