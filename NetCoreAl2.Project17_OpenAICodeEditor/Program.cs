using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

class Program
{
    static async Task Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.WriteLine("🤖 OpenAI Kod Asistanına Hoş Geldin\n");
        Console.WriteLine("Kodunu yaz ve aşağıdaki işlemlerden birini seç\n");
        Console.WriteLine("1- Açıklama Üret");
        Console.WriteLine("2- Refactor Et");
        Console.WriteLine("3- Test Case Oluştur");

        Console.Write("\nSeçimin (1/2/3): ");
        var choice = Console.ReadLine();

        Console.WriteLine("\nKodunu Gir (bitirmek için 'END' yaz): ");
        StringBuilder userCode = new();

        string? line;
        while ((line = Console.ReadLine()) != null && line.Trim() != "END")
        {
            userCode.AppendLine(line);
        }

        string prompt = choice switch
        {
            "1" => $"Lütfen aşağıdaki C# kodunu açıklayıcı bir şekilde açıkla:\n\n{userCode}",
            "2" => $"Lütfen aşağıdaki C# kodunu daha temiz, okunabilir ve iyi şekilde refactor et:\n\n{userCode}",
            "3" => $"Lütfen aşağıdaki C# kodu için Unit test case üret:\n\n{userCode}"
        };

        var result = await AskOpenAI(prompt);
        Console.WriteLine("\n💬 OPENAI'nin Yanıtı:\n ");
        Console.WriteLine(result);
    }

    static async Task<string> AskOpenAI(string prompt)
    {
        const string apiKey = "";
        const string endpoint = "https://api.openai.com/v1/chat/completions";

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        var requestBody = new
        {
            model = "gpt-4",
            messages = new[]
            {
                new
                {
                    role = "system",
                    content = "Sen uzman bir C# yazılım geliştiricisisin. Kodları açıkla, düzelt veya test case üret."
                },
                new
                {
                    role = "user",
                    content = prompt
                }
            },
            temperature = 0.7
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync(endpoint, content);
        var responseJson = await response.Content.ReadAsStringAsync();

        var doc = JsonDocument.Parse(responseJson);
        return doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .ToString();
    }
}