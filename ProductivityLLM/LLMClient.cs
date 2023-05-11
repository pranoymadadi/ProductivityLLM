using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensions.Msal;
using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using static System.Formats.Asn1.AsnWriter;

public class LLMClient
{
    const string Endpoint = "https://fe-26.qas.bing.net/completions";

    static IEnumerable<string> Scopes = new List<string>() {
        "api://68df66a4-cad9-4bfd-872b-c6ddde00d6b2/access"
    };

    static IPublicClientApplication app = PublicClientApplicationBuilder.Create("68df66a4-cad9-4bfd-872b-c6ddde00d6b2")
        .WithAuthority("https://login.microsoftonline.com/72f988bf-86f1-41af-91ab-2d7cd011db47")
        .Build();

    public static async Task Test()
    {
        var cacheHelper = await CreateCacheHelperAsync().ConfigureAwait(false);
        cacheHelper.RegisterCache(app.UserTokenCache);

        string requestData = JsonSerializer.Serialize(new ModelPrompt
        {
            Prompt = "Seattle is",
            MaxTokens = 50,
            Temperature = 1,
            TopP = 1,
            N = 5,
            Stream = false,
            LogProbs = null,
            Stop = "\n"
        });
        // Available models are listed here: https://msasg.visualstudio.com/QAS/_wiki/wikis/QAS.wiki/134728/Getting-Started-with-Substrate-LLM-API?anchor=available-models
        var response = await SendRequest("dev-text-davinci-003", requestData);
        Console.WriteLine(response);

        var streamRequestData = JsonSerializer.Serialize(new ModelPrompt
        {
            Prompt = "Instruction: Given an input question, respond with syntactically correct c++. Be creative but the c++ must be correct. \nInput: Create a function in c++ to remove duplicate strings in a std::vector<std::string>\n",
            MaxTokens = 500,
            Temperature = 0.6,
            TopP = 1,
            N = 1,
            Stream = true,
            LogProbs = null,
            Stop = "\r\n"
        });

        await SendStreamRequest("dev-text-davinci-003", streamRequestData);

    }

    public class ModelPrompt
    {
        [JsonPropertyName("prompt")]
        public string? Prompt
        {
            get;
            set;
        }

        [JsonPropertyName("max_tokens")]
        public int MaxTokens
        {
            get;
            set;
        }

        [JsonPropertyName("temperature")]
        public double Temperature
        {
            get;
            set;
        }

        [JsonPropertyName("top_p")]
        public int TopP
        {
            get;
            set;
        }

        [JsonPropertyName("n")]
        public int N
        {
            get;
            set;
        }

        [JsonPropertyName("stream")]
        public bool Stream
        {
            get;
            set;
        }

        [JsonPropertyName("logprobs")]
        public object? LogProbs
        {
            get;
            set;
        }

        [JsonPropertyName("stop")]
        public string? Stop
        {
            get;
            set;
        }
    };

    public class Choice
    {
        [JsonPropertyName("text")]
        public string? Text
        {
            get;
            set;
        }

        [JsonPropertyName("index")]
        public int Index
        {
            get;
            set;
        }

        [JsonPropertyName("logprobs")]
        public object? LogProbs
        {
            get;
            set;
        }

        [JsonPropertyName("finish_reason")]
        public string? FinishReason
        {
            get;
            set;
        }
    }

    public class StreamResponse
    {
        [JsonPropertyName("id")]
        public string? Id
        {
            get;
            set;
        }

        [JsonPropertyName("object")]
        public string? Object
        {
            get;
            set;
        }

        [JsonPropertyName("created")]
        public int Created
        {
            get;
            set;
        }

        [JsonPropertyName("choices")]
        public List<Choice>? Choices
        {
            get;
            set;
        }

        [JsonPropertyName("model")]
        public string? Model
        {
            get;
            set;
        }
    }

    static async Task<string> GetToken()
    {

        var accounts = await app.GetAccountsAsync();
        AuthenticationResult? result = null;
        if (accounts.Any())
        {
            var chosen = accounts.First();

            try
            {
                result = await app.AcquireTokenSilent(Scopes, chosen).ExecuteAsync();
            }
            catch (MsalUiRequiredException)
            {
                // cannot get a token silently, so redirect the user to be challenged 
            }
        }
        if (result == null)
        {
            result = await app.AcquireTokenWithDeviceCode(Scopes,
                deviceCodeResult => {
                    // This will print the message on the console which tells the user where to go sign-in using
                    // a separate browser and the code to enter once they sign in.
                    // The AcquireTokenWithDeviceCode() method will poll the server after firing this
                    // device code callback to look for the successful login of the user via that browser.
                    // This background polling (whose interval and timeout data is also provided as fields in the
                    // deviceCodeCallback class) will occur until:
                    // * The user has successfully logged in via browser and entered the proper code
                    // * The timeout specified by the server for the lifetime of this code (typically ~15 minutes) has been reached
                    // * The developing application calls the Cancel() method on a CancellationToken sent into the method.
                    //   If this occurs, an OperationCanceledException will be thrown (see catch below for more details).
                    Console.WriteLine(deviceCodeResult.Message);
                    return Task.FromResult(0);
                }).ExecuteAsync();

        }
        return (result.AccessToken);
    }

    static async Task<string> SendRequest(string modelType, string requestData)
    {
        var token = await GetToken();
        var httpClient = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Post, Endpoint);
        request.Content = new StringContent(requestData, Encoding.UTF8, "application/json");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        request.Headers.Add("X-ModelType", modelType);

        var httpResponse = await httpClient.SendAsync(request);

        return (await httpResponse.Content.ReadAsStringAsync());
    }

    static async Task SendStreamRequest(string modelType, string requestData)
    {
        var token = await GetToken();
        var httpClient = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Post, Endpoint);
        request.Content = new StringContent(requestData, Encoding.UTF8, "application/json");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        request.Headers.Add("X-ModelType", modelType);

        var httpResponse = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

        var stream = await httpResponse.Content.ReadAsStreamAsync();
        TextReader textReader = new StreamReader(stream);

        string? line;
        while ((line = await textReader.ReadLineAsync()) != null)
        {
            if (line.StartsWith("data: "))
            {
                var lineData = line.Substring(6);
                if (string.Equals(lineData, "[DONE]"))
                {
                    break;
                }

                var result = JsonSerializer.Deserialize<StreamResponse>(line.Substring(6));

                if (result?.Choices?.Count > 0)
                {
                    Console.Write(result.Choices[0].Text);
                }
            }
        }

    }

    private static async Task<MsalCacheHelper> CreateCacheHelperAsync()
    {
        StorageCreationProperties storageProperties;

        try
        {
            storageProperties = new StorageCreationPropertiesBuilder(
                ".llmapi-token-cache.txt",
                ".")
            .WithLinuxKeyring(
                "com.microsoft.substrate.llmapi",
                MsalCacheHelper.LinuxKeyRingDefaultCollection,
                "MSAL token cache for LLM API",
                new KeyValuePair<string, string>("Version", "1"),
                new KeyValuePair<string, string>("ProductGroup", "LLMAPI"))
            .WithMacKeyChain(
                "llmapi_msal_service",
                "llmapi_msla_account")
            .Build();

            var cacheHelper = await MsalCacheHelper.CreateAsync(
                storageProperties).ConfigureAwait(false);

            cacheHelper.VerifyPersistence();
            return cacheHelper;

        }
        catch (MsalCachePersistenceException e)
        {
            Console.WriteLine($"WARNING! Unable to encrypt tokens at rest." +
                $" Saving tokens in plaintext at {Path.Combine(".", ".llmapi-token-cache.txt")} ! Please protect this directory or delete the file after use");
            Console.WriteLine($"Encryption exception: " + e);

            storageProperties =
                new StorageCreationPropertiesBuilder(
                    ".llmapi-token-cache.txt" + ".plaintext", // do not use the same file name so as not to overwrite the encypted version
                    ".")
                .WithUnprotectedFile()
                .Build();

            var cacheHelper = await MsalCacheHelper.CreateAsync(storageProperties).ConfigureAwait(false);
            cacheHelper.VerifyPersistence();

            return cacheHelper;
        }
    }
}