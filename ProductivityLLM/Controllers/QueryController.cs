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
            // Converter.CSVtoText();
            // ReadDataFromFile.CreateEmbeddingAndSaveData("C:\\Users\\pranoymadadi\\source\\repos\\ProductivityLLM\\ProductivityLLM\\FinalData.txt");
            ReadDataFromFile.ReadData("C:\\Users\\pranoymadadi\\source\\repos\\ProductivityLLM\\ProductivityLLM\\FinalData.txt", workItems);
            ReadDataFromFile.ReadData("C:\\Users\\pranoymadadi\\Downloads\\embedding.txt", dictOfIdAndEmbeddings, workItems);
        }

        [HttpPost]
        public async Task<string> Query([FromBody] WorkItem text)
        {
            return await QueryHelper.callOpenAIWithRetries(text);
        }
    }
}
