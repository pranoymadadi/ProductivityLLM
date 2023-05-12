using Microsoft.Identity.Client;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class ChatGPTClient
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public ChatGPTClient(string apiKey)
    {
        _apiKey = apiKey;
        _httpClient = new HttpClient();
    }

    public async Task<string> GetResponse(string prompt)
    {
        var uri = "https://api.openai.com/v1/edits";
        var request = new HttpRequestMessage(HttpMethod.Post, uri);

        request.Headers.Add("Authorization", $"Bearer {_apiKey}");

        var requestBody = JsonSerializer.Serialize(new
        {
            model = "text-davinci-edit-001",
            input = prompt,
            temperature = 0.2,
            instruction = "Fill the empty assignedTo with an email already mentioned in the input after analysing the data where assignedTo is not empty to understand who is most suited for each task based on the assignedTo, input, description, tags field."
        });

        request.Content = new StringContent(requestBody, System.Text.Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            var responseJson = JsonSerializer.Deserialize<ChatGPTResponse>(responseBody);
            return responseJson.choices[0].text;
        }

        if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            throw new BadHttpRequestException("Bad request", (int)response.StatusCode);
        }

        return null;
    }

    public async Task<List<float>> GetEmdedding(string text) 
    {
        var uri = "https://api.openai.com/v1/embeddings";
        var request = new HttpRequestMessage(HttpMethod.Post, uri);

        request.Headers.Add("Authorization", $"Bearer {_apiKey}");

        string requestBody = JsonSerializer.Serialize(new
        {
            input = text,
            model = "text-embedding-ada-002",
        });

        request.Content = new StringContent(requestBody, System.Text.Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request);

        string responseJson = await response.Content.ReadAsStringAsync();

        // Deserialize the response JSON to a float[] object
        var embeddingResponse = JsonSerializer.Deserialize<EmbeddingResponse>(responseJson);

        return embeddingResponse?.data[0].embedding;
    }
}

public class ChatGPTResponse
{
    public ChatGPTChoice[] choices { get; set; }
}

public class ChatGPTChoice
{
    public string text { get; set; }
}

public class Embedding
{
    public int index { get; set; }
    public List<float> embedding { get; set; }
}

public class Usage
{
    public int prompt_tokens { get; set; }
    public int total_tokens { get; set; }
}

public class EmbeddingResponse
{
    public string @object { get; set; }
    public List<Embedding> data { get; set; }
    public string model { get; set; }
    public Usage usage { get; set; }
}



