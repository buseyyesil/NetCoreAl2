using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;


class Program
{
    static async Task Main(string[] args)
    {
        string apiKey = "";
        string model = "gemini-1.5-flash";
        string endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

        string context = "Sen bir yapay zeka içerik planlayıcısısın." +
            " Kullanıcının fikrine göre içerik üretmesine yardım edeceksin. " +
            "Fikri aldıktan sonra kllanıcıya doğru sorular sorarak onu yönlendirecek ve sonunda içerik " +
            "planını çıkarıcaksın";

        Console.Write("Bir fikrin mi var? (örnek: bir kafe açmak istiyorum): ");
        string userIdea = Console.ReadLine();

      
        string prompt = $"{context}\nKullanıcının fikri: {userIdea}\nŞimdi ona adım adım sorular sormaya başla.";

        for (int i = 0; i < 5; i++)
        {
            string question = await SendToGemini(endpoint, prompt);
            Console.WriteLine($"Agent: {question}");

            Console.Write("Sen: ");
            string answer = Console.ReadLine();

            prompt += $"\nAgent: {question}\nKullanıcının cevabı: {answer}\n Yeni sorunu sor";
        }

        prompt += "\nArtık yeterli bilgiye sahipsin. Kullanıcı için detaylı bir içerik planı oluştur.";

        string finalPlan = await SendToGemini(endpoint, prompt);
        Console.WriteLine("\nNihai İçerik Planı:");
        Console.WriteLine(finalPlan);
    }

    static async Task<string> SendToGemini(string endpoint, string prompt)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

    
        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new[] { new { text = prompt } }
                }
            }
        };

        string json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync(endpoint, content);
        string responseText = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            return $"Hata: {response.StatusCode} - {responseText}";

        try
        {
            using var doc = JsonDocument.Parse(responseText);

         
            return doc.RootElement
               .GetProperty("candidates")[0]
               .GetProperty("content")
               .GetProperty("parts")[0]
               .GetProperty("text")
               .GetString() ?? "";
        }
        catch
        {
            return "Yanıt parse edilirken hata oluştu.";
        }
    }
}