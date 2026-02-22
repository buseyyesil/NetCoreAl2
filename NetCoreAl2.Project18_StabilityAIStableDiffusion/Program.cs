using System.Text;

class Program
{
    static async Task Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.WriteLine("🤖 Prompt'tan Görsel Üretici - Stability AI SD3");
        Console.Write("Prompt girin: ");
        string prompt = Console.ReadLine();

        string apiKey = "";
        string engineId = "stable-diffusion-xl-1024-v1-0";
        string apiUrl = $"https://api.stability.ai/v1/generation/{engineId}/text-to-image";
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
        httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        var requestBody = new
        {
            text_prompts = new[]
          {
             new { text = prompt }
         },
            cfg_scale = 12,
            height = 1024,
            width = 1024,
            samples = 1,
            steps = 30
        };
        var jsonContent=new StringContent(System.Text.Json.JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync(apiUrl, jsonContent);
        if(!response.IsSuccessStatusCode)
        {
            Console.WriteLine("Hata:" +response.StatusCode);
            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine(error);
            return;
        }
        var responseString = await response.Content.ReadAsStringAsync();
        var responseJson = System.Text.Json.JsonDocument.Parse(responseString);
        string base64Image = responseJson
            .RootElement
            .GetProperty("artifacts")[0]
            .GetProperty("base64")
            .GetString();
        byte[] imageBytes = Convert.FromBase64String(base64Image);
        string fileName = $"generated_{DateTime.Now:yyyyMMdd_HHmmss}.jpg";
        await File.WriteAllBytesAsync(fileName, imageBytes);
        Console.WriteLine($" 🎉 Görsel başarıyla oluşturuldu: {fileName}");
        Console.ReadLine();
    }
}