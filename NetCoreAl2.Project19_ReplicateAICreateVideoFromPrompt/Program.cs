using System.Diagnostics;
using System.Text.Json;

class Program
{
    static async Task Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine("Replicate AI ile Video Üretici Uygulaması:");
        Console.Write("Lütfen video oluşturmak istediğiniz metni girin: ");
        string prompt = Console.ReadLine();
        string apiKey = "";
        string version = "";
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Token", apiKey);
        
        var body = new
        {
            version,
            input = new
            {
                prompt,
                num_frames = 24, //videodaki toplam kare sayısı
                fps = 8, //videodaki karelerin saniyede gösterilme sayısı
                guidance_scale = 12.5, //modelin ürettiği görüntünün verilen metne ne kadar bağlı kalacağını belirler. Yüksek değerler daha fazla bağlılık sağlar, ancak aşırı yüksek değerler görüntünün kalitesini düşürebilir.

                num_inference_steps = 50, //modelin görüntüyü oluşturmak için kaç adımda çalışacağını belirler. Daha fazla adım genellikle daha yüksek kaliteli sonuçlar verir, ancak işlem süresini artırır.
                width = 576,
                height = 320
            }
        };
        var json = System.Text.Json.JsonSerializer.Serialize(body);
        var response = await client.PostAsync("https://api.replicate.com/v1/predictions", new StringContent(json, System.Text.Encoding.UTF8, "application/json"));

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine("API Hatası:" + await response.Content.ReadAsStringAsync());
            return;
        }
        var pred=JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        string id=pred.RootElement.GetProperty("id").GetString();
        Console.WriteLine("📹Video oluşturuluyor, lütfen bekleyin...");
        string status = "";
        string videoUrl = "";

        while (status != "succeeded")
        {
            await Task.Delay(5000);
            var chk = await client.GetAsync($"https://api.replicate.com/v1/predictions/{id}");
            var chkJson = JsonDocument.Parse(await chk.Content.ReadAsStringAsync());

            status = chkJson.RootElement.GetProperty("status").GetString();
            Console.WriteLine($"🔎 Durum: {status}");

            if (status == "failed")
            {
                Console.WriteLine("Üretim başarısız oldu");
                return;
            }

            if (status == "succeeded")
            {
                var output = chkJson.RootElement.GetProperty("output");
                videoUrl = output.ValueKind == JsonValueKind.Array
                    ? output[0].GetString()
                    : output.GetString();
            }
        }

        Console.WriteLine($"✅ Video hazır: {videoUrl}");


        using var stream = await client.GetStreamAsync(videoUrl);
        await using var file = File.Create("generated_video.mp4");
        await stream.CopyToAsync(file);
        Console.WriteLine("🎊 Video indirildi -> generated_video.mp4");


        Process.Start(new ProcessStartInfo
        {
            FileName = "generated_video.mp4",
            UseShellExecute = true
        });
    }
}
