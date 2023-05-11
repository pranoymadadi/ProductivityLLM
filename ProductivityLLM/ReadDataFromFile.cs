using Newtonsoft.Json;
using static System.Net.Mime.MediaTypeNames;

namespace ProductivityLLM
{
    public class ReadDataFromFile
    {

        public static void ReadData(string filePath, Dictionary<string, WorkItem> workItems)
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    WorkItem workItem = JsonConvert.DeserializeObject<WorkItem>(line);
                    workItems.TryAdd(workItem.id, workItem);
                    Console.WriteLine(line);
                }
            }

        }

        public async static void CreateEmbeddingAndSaveData(string filePath)
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                var client = new ChatGPTClient("Token1");

                while ((line = reader.ReadLine()) != null)
                {
                    WorkItem workItem = JsonConvert.DeserializeObject<WorkItem>(line);
                    var tempWorkItem = new WorkItem();
                    tempWorkItem.title = workItem.title;
                    tempWorkItem.description = workItem.description;
                    tempWorkItem.resolvedBy = workItem.resolvedBy;
                    tempWorkItem.assignedTo = workItem.assignedTo;
                    var embeddings = await client.GetEmdedding(JsonConvert.SerializeObject(tempWorkItem));
                    File.WriteAllText("embedding.txt", string.Join(",", embeddings));
                    File.WriteAllText("embedding.txt", "\n");
                }
            }

        }



        public static void ReadData(string filePath, Dictionary<string, List<float>> embeddings, Dictionary<string, WorkItem> workItems)
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                int i=0;
                while ((line = reader.ReadLine()) != null)
                {
                    var embedding = line.Split(',').Select(float.Parse).ToList();
                    embeddings.TryAdd(workItems.Keys.ToList()[i], embedding);
                    Console.WriteLine(line);
                    i++;
                }
            }

        }
    }
}
