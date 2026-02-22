using System;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

class Program
{
    static async Task Main(string[] args)
    {
        var apiKey = "";
        Console.WriteLine("Please input text here:");
        var inputText = Console.ReadLine();
        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            var requestBody=new
            {
                inputs = inputText
            };  
            var json=JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("https://api-inference.huggingface.co/models/dslim/bert-base-NER",content);//content, HttpClient ile gönderilen POST isteğinin gövdesini(request body) temsil eder. JSON formatındaki veriyi UTF-8 encoding ile API’ye iletmemizi sağlar.
            var responseString = await response.Content.ReadAsStringAsync();

            Console.WriteLine("🎭NER Çıktısı");
            Console.WriteLine();

            var doc = JsonDocument.Parse(responseString);//API’den gelen JSON string’i Parçalayıp okunabilir hale getiriyor doc.RootElement üzerinden property’lere erişmeni sağlıyor
            foreach (var item in doc.RootElement.EnumerateArray())
            {
                string word = item.GetProperty("word").GetString();
                string entity = item.GetProperty("entity_group").GetString();
                double score = Math.Round(item.GetProperty("score").GetDouble() * 100, 2);

                Console.WriteLine($"--->{word}");
                Console.WriteLine($" |-Türü: {entity}");
                Console.WriteLine($" |-Güven: %{score}");
            }
        }

    }
}