using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

class Program
{
    static async Task Main(string[] args)
    {
        var apiKey = "";
        Console.Write("Enter your comment here:");
        string inputText = Console.ReadLine();
        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            var requestBody = new
            {
                inputs = inputText
            };
            var json = JsonSerializer.Serialize(requestBody);
            var context = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("https://router.huggingface.co/hf-inference/models/unitary/toxic-bert", context);
            var responseString = await response.Content.ReadAsStringAsync();
            if (!responseString.TrimStart().StartsWith("["))
            {
                Console.WriteLine("⏳model yükleniyor veya hata oluştu:");
                Console.WriteLine(responseString);
                return;
            }
            var doc = JsonDocument.Parse(responseString);
            Console.WriteLine("\n 💎 yorum analizi sonucu: \n");
            foreach (var item in doc.RootElement[0].EnumerateArray())
            {
                string label = item.GetProperty("label").GetString();
                double score = Math.Round(item.GetProperty("score").GetDouble() * 100, 2);
                if (score >= 50)
                {
                    Console.WriteLine($"{label.ToUpper()}-->%{score}");
                }

            }


        }
    }
}