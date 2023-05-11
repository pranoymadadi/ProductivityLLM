using Microsoft.AspNetCore.Mvc;
using MathNet.Numerics.LinearAlgebra;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ProductivityLLM.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class QueryController : Controller
    {

        private readonly ILogger<WeatherForecastController> _logger;
        public static Dictionary<string, WorkItem> workItems = new Dictionary<string, WorkItem>();
        public static Dictionary<string, List<float>> dictOfIdAndEmbeddings = new Dictionary<string, List<float>>();


        public QueryController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        public static void Initialize() {
            Converter.CSVtoText();
            ReadDataFromFile.CreateEmbeddingAndSaveData("C:\\Users\\pranoymadadi\\source\\repos\\ProductivityLLM\\ProductivityLLM\\FinalData.txt");
            ReadDataFromFile.ReadData("C:\\Users\\pranoymadadi\\source\\repos\\ProductivityLLM\\ProductivityLLM\\SampleInputData.txt", workItems);
            ReadDataFromFile.ReadData("C:\\Users\\pranoymadadi\\Downloads\\ADAOutput.txt", dictOfIdAndEmbeddings, workItems);
        }

        [HttpGet]
        public async Task<string> Query([FromBody] WorkItem text)
        {
            var client = new ChatGPTClient("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");
            var tempWorkItem = new WorkItem();
            tempWorkItem.title = text.title;
            tempWorkItem.description = text.description;
            tempWorkItem.resolvedBy = text.resolvedBy;
            var embeddings = await client.GetEmdedding(JsonConvert.SerializeObject(tempWorkItem));

            var topWorkItemIds = QueryHelper.GetTopEmbeddingsList(embeddings, dictOfIdAndEmbeddings);
            var prompt = QueryHelper.GeneratePrompt(topWorkItemIds, workItems);
            prompt.Add(text);
            var response = await client.GetResponse(JsonConvert.SerializeObject(prompt));
            // [{\"title\":\"Sizeofcardisbiggerinreply/forwardcomposemode\",\"assignedto\":\"AshishSingh\"},{\"title\":\"SilentAuthisfailingonOWA\",\"assignedto\":\"AshishSingh\"},{\"title\":\"SignInisfailingcontinuouslyonLUcards\",\"assignedto\":\"AshishSingh\"},{\"title\":\"ShowfullURLonhoveringovercardtapaction\",\"assignedto\":\"SunnyMitra\"}\r\n\r\n,{\"title\":\"Link does not unfurl even after bot response\",\"assignedto\":\"Pragya Gangber\"},\r\n\r\n{\"title\":\"Jira Cloud search flyout is broken\",\"assignedto\":\"SunnyMitra\"},{\"title\":\"Pastingcardincomposeispastingfallback\",\"assignedto\":\"\"}]
            // await LLMClient.Test();
            var a = JsonConvert.DeserializeObject<List<WorkItem>>(response);
            return a.Where(o => o.id == text.id).ToList()[0].assignedTo;
        }
    }
}
