namespace ProductivityLLM
{
    using MathNet.Numerics.LinearAlgebra;
    using Newtonsoft.Json;

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

        public static List<string> GetTopEmbeddingsList(List<float> embeddings, Dictionary<string, List<float>> dictOfIdAndEmbeddings)
        {
            Dictionary<string, double> dictOfIdAndDistance = new Dictionary<string, double>();
            foreach (var id in dictOfIdAndEmbeddings)
            {
                dictOfIdAndDistance[id.Key] = compareEmbeddings(embeddings, id.Value);
            }
            var sortedDict = dictOfIdAndDistance.OrderBy(kvp => kvp.Value);

            return sortedDict.Take(10).Select(kvp => kvp.Key).ToList();
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
    }
}
