using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

class Program
{
    static async Task Main(string[] args)
    {
        string apiKey = "";
        Console.Write("lütfen sormak istediğiniz soruyu yazınız:");
        string prompt = Console.ReadLine();
        using var client = new HttpClient();
        client.BaseAddress = new Uri("https://api.anthropic.com");
        client.DefaultRequestHeaders.Add("x-api-key", apiKey);
        client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        var requestBody = new
        {
            model = "claude-3-opus-20240229",
            max_tokens = 1000,
            temperature = 0.7,
            messages = new[]
            {
                new
                {
                    role = "user",
                    content = prompt
                }
            }
        };
        var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        var response = await client.PostAsync("v1/messages", jsonContent);
        var responseString = await response.Content.ReadAsStringAsync();
        Console.WriteLine("💭 Claude Cevabı:");
        Console.WriteLine(responseString);
    }
}