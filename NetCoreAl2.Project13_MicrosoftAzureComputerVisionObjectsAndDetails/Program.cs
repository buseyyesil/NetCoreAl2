using System.Net.Http.Headers;
using System.Text.Json;


class Program
{
    static async Task Main(string[] args)
    {
        string imagePath = "";
        string subscriptionKey = "";
        string endpoint = "";

        string apiUrl = $"{endpoint}/vision/v3.2/analyze";

        string requestParameters = "visualFeatures=Categories,Description,Tags,Color,Faces,Objects,Brands,Adult,ImageType&language=en&model-version=latest";

        string uri = apiUrl + "?" + requestParameters;

        if (!File.Exists(imagePath))
        {
            Console.WriteLine("Görsel dosyası bulunamadı!" + imagePath);
            return;
        }

        byte[] imageBytes = await File.ReadAllBytesAsync(imagePath);

        using (HttpClient client = new HttpClient())
        using (ByteArrayContent content = new ByteArrayContent(imageBytes))
        {
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            HttpResponseMessage response = await client.PostAsync(uri, content);
            string result = await response.Content.ReadAsStringAsync();


            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Azure Yanıtı: ");
                JsonDocument json = JsonDocument.Parse(result);

                var objects = json.RootElement.GetProperty("objects");
                foreach (var obj in objects.EnumerateArray())
                {
                    string name = obj.GetProperty("object").GetString();
                    double confidence = obj.GetProperty("confidence").GetDouble();
                    Console.WriteLine($"Nesne: {name} (Güven: %{confidence * 100:0.00})");
                }

            }
            else
            {
                Console.WriteLine("bir hata oluştu!");
                Console.WriteLine($"Status {response.StatusCode}");
                Console.WriteLine("Yanıt: " + result);
            }
        }
    }
}