using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;

class Program
{
    static async Task Main(string[] args)
    {
        string apiKey = "";
        string prompt = "bana 'yazılım geliştirici' pozisyonu için hazırlanan, profosyonel ama samimi tonda" +
            " bir iş başvuru e postası yazar mısın ? Adım buse,2 yıldır .net geliştiricisiyim ekip " +
            "çalışmasına yatkınım ve uzaktan çalışmaya uygunum";
        using var client=new HttpClient();  
        client.BaseAddress= new Uri("https://api.anthropic.com/");
        client.DefaultRequestHeaders.Add("x-api-key", apiKey);
        client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("applicaton/json"));
        var requestBody = new
        {
            model = "claude-3-opus-20240229",
            max_tokens = 1000,
            temperature = 0.5,
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
        Console.WriteLine("Claude pdf özeti:");
        Console.WriteLine(responseString);
    }
}