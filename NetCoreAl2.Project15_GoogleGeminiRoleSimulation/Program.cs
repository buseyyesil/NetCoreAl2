using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;

class Program
{
    static async Task Main(string[] args)
    {
        string apiKey = "";
        string model = "gemini-1.5-pro";
        string endpoint =
      $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

        Console.WriteLine("Rolünüzü Seçin: ");
        Console.WriteLine("1-Psikolog");
        Console.WriteLine("2-Hostes");
        Console.WriteLine("3-Polis");
        Console.WriteLine("4-Yazılım Geliştirici");
        Console.WriteLine("5-Pilates Eğitmeni");
        Console.WriteLine("6-Diş Hekimi");
        Console.WriteLine();

        Console.WriteLine("Seçiminiz: ");
        string roleChoice = Console.ReadLine();
        string rolePrompt= roleChoice switch
        {
            "1" => "Psikolog rolünde cevap ver.",
            "2" => "Hostes rolünde cevap ver.",
            "3" => "Polis rolünde cevap ver.",
            "4" => "Yazılım Geliştirici rolünde cevap ver.",
            "5" => "Pilates Eğitmeni rolünde cevap ver.",
            "6" => "Diş Hekimi rolünde cevap ver.",
            
        };
        Console.WriteLine();
        Console.Write("Sormak istediğiniz cümleyi giriniz: ");
        string userInput = Console.ReadLine();

        string finalPrompt =
            $"{rolePrompt}\n\nKullanıcıdan Gelen Soru: {userInput}";
        var requestBody = new
        {
            contents = new[]
          {
                new
                {
                    parts = new[]
                    {
                        new { text = finalPrompt }
                    }
                }
            }
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await client.PostAsync(endpoint, content);
        var responseText = await response.Content.ReadAsStringAsync();

        try
        {
            var doc = JsonDocument.Parse(responseText);

            string answer = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text") 
                .GetString();

            Console.WriteLine("\nYANIT:\n" + answer);
        }
        catch
        {
            Console.WriteLine("Yanıt Hatası:\n" + responseText);
        }
    }
}
