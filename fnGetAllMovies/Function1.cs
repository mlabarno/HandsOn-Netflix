using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace fnGetAllMovies
{
    public class Function1
    {
        private readonly ILogger<Function1> _logger;
        private readonly CosmosClient _cosmosClient;

        public Function1(ILogger<Function1> logger, CosmosClient cosmosClient)
        {
            _cosmosClient = cosmosClient;
            _logger = logger;
        }

        [Function("all")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var container = _cosmosClient.GetContainer("DioFlixDB", "movies");
            var id = req.Query["id"];
            
            var query = $"SELECT id FROM c";
            var queryDefinition = new QueryDefinition(query);
            var result = container.GetItemQueryIterator<MovieResult>(queryDefinition);
            var results = new List<MovieResult>();

            while (result.HasMoreResults)
            {
                foreach (var item in await result.ReadNextAsync())
                {
                    results.Add(item);
                }
            }

            var responseMessage = req.CreateResponse(System.Net.HttpStatusCode.OK);
            await responseMessage.WriteAsJsonAsync(results);

            return responseMessage;
        }
    }
}
