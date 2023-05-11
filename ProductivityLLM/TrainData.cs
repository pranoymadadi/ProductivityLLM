using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class TrainData
{
    public static async Task trainData()
    {
        var apiKey = "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";
        var trainFileIdOrPath = "";
        var baseModel = "text-davinci-edit-001";
        var modelOutputName = "productivity-llm";

        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        var createFineTuneRequest = new Dictionary<string, object>
        {
            { "model", baseModel },
            { "train_file", trainFileIdOrPath },
            { "model_name", modelOutputName }
        };

        var createFineTuneRequestContent = new StringContent(JsonConvert.SerializeObject(createFineTuneRequest));
        createFineTuneRequestContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

        var createFineTuneResponse = await httpClient.PostAsync("https://api.openai.com/v1/fine-tunes", createFineTuneRequestContent);

        if (!createFineTuneResponse.IsSuccessStatusCode)
        {
            var errorContent = await createFineTuneResponse.Content.ReadAsStringAsync();
            throw new Exception($"Error creating fine-tuned model: {errorContent}");
        }

        var createFineTuneResponseContent = await createFineTuneResponse.Content.ReadAsStringAsync();
        var createFineTuneResponseJson = JsonConvert.DeserializeObject<Dictionary<string, object>>(createFineTuneResponseContent);

        var fineTunedModelId = (string)createFineTuneResponseJson["model_id"];

        Console.WriteLine($"Fine-tuned model created with ID {fineTunedModelId}");
    }
}

