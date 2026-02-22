using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

class Program
{
    static async Task Main(string[] args)
    {
        Console.Write("Text to Image: ");
        string prompt = Console.ReadLine();

        string token = "";

        string apiUrl = "https://api.replicate.com/v1/models/stability-ai/stable-diffusion-3.5-medium/predictions";

        var requestBody = new
        {
            input = new
            {
                prompt = prompt
            }
        };

        var json = JsonSerializer.Serialize(requestBody);

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", token);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        Console.WriteLine("Image creating....");

        var response = await client.PostAsync(apiUrl, content);
        string responseContent = await response.Content.ReadAsStringAsync();

        Console.WriteLine($"HTTP: {(int)response.StatusCode}");
        Console.WriteLine(responseContent);
    }
}
