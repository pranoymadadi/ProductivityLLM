namespace ProductivityLLM
{
    using MathNet.Numerics.LinearAlgebra;
    using Newtonsoft.Json;
    using ProductivityLLM.Controllers;

    public class QueryHelper
    {

        public static double compareEmbeddings(List<float> embedding1, List<float> embedding2)
        {
            Vector<float> vector1 = Vector<float>.Build.DenseOfEnumerable(embedding1);
            Vector<float> vector2 = Vector<float>.Build.DenseOfEnumerable(embedding2);

            // Compute the cosine similarity between the vectors
            double cosineSimilarity = vector1.DotProduct(vector2) / (vector1.Norm(2) * vector2.Norm(2));

            return cosineSimilarity;
        }

        public static List<string> GetTopEmbeddingsList(List<float> embeddings, Dictionary<string, List<float>> dictOfIdAndEmbeddings, int numberOfSamples)
        {
            Dictionary<string, double> dictOfIdAndDistance = new Dictionary<string, double>();
            foreach (var id in dictOfIdAndEmbeddings)
            {
                dictOfIdAndDistance[id.Key] = compareEmbeddings(embeddings, id.Value);
            }
            var sortedDict = dictOfIdAndDistance.OrderByDescending(kvp => kvp.Value);

            return sortedDict.Take(numberOfSamples).Select(kvp => kvp.Key).ToList();
        }

        public static List<WorkItem> GeneratePrompt(List<string> ids, Dictionary<string, WorkItem> workItems)
        {
            var prompt = new List<WorkItem>();
            foreach (var id in ids)
            {
                prompt.Add(workItems[id]);
            }

            return prompt;
        }

        public static async Task<string> callOpenAIWithRetries(WorkItem text, int retryCount = 0, bool isDescriptionNeeded = true, int numberOfSamples = 10)
        {
            var client = new ChatGPTClient("");

            try
            {
                var tempWorkItem = new WorkItem();
                tempWorkItem.title = text.title;
                tempWorkItem.tags = text.tags;
                tempWorkItem.description = isDescriptionNeeded ? text.description : "";

                var embeddings = await client.GetEmdedding(JsonConvert.SerializeObject(tempWorkItem));

                var topWorkItemIds = QueryHelper.GetTopEmbeddingsList(embeddings, QueryController.dictOfIdAndEmbeddings, numberOfSamples);
                var prompt = QueryHelper.GeneratePrompt(topWorkItemIds, QueryController.workItems);
                prompt.Add(text);
                var response = await client.GetResponse(JsonConvert.SerializeObject(prompt));
                // [{\"title\":\"Sizeofcardisbiggerinreply/forwardcomposemode\",\"assignedto\":\"AshishSingh\"},{\"title\":\"SilentAuthisfailingonOWA\",\"assignedto\":\"AshishSingh\"},{\"title\":\"SignInisfailingcontinuouslyonLUcards\",\"assignedto\":\"AshishSingh\"},{\"title\":\"ShowfullURLonhoveringovercardtapaction\",\"assignedto\":\"SunnyMitra\"}\r\n\r\n,{\"title\":\"Link does not unfurl even after bot response\",\"assignedto\":\"Pragya Gangber\"},\r\n\r\n{\"title\":\"Jira Cloud search flyout is broken\",\"assignedto\":\"SunnyMitra\"},{\"title\":\"Pastingcardincomposeispastingfallback\",\"assignedto\":\"\"}]
                // await LLMClient.Test();
                if (response == null)
                {
                    return "sumit@microsoft.com";
                }
                var a = JsonConvert.DeserializeObject<List<WorkItem>>(response);
                return a.Where(o => o.id == text.id).ToList()[0].assignedTo;
            }
            catch (BadHttpRequestException ex)
            {
                if (retryCount == 2)
                {
                    return "sumit@microsoft.com";
                }
                else if (retryCount == 1)
                {
                    return await callOpenAIWithRetries(text, retryCount + 1, false, 3);
                }

                return await callOpenAIWithRetries(text, retryCount + 1, false);
            }
            catch (Exception ex)
            {
                return "sumit@microsoft.com";
            }
        }
    }
}
