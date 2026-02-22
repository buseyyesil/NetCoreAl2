using NAudio.Wave;
using System.Net.Http.Headers;
using System.Speech.Synthesis;
using System.Text;
using System.Text.Json;

class Program
{
    private static readonly HttpClient Http = new HttpClient();

    private static readonly string OpenAiApiKey = "";

    static async Task Main()
    {
        Console.OutputEncoding = Encoding.UTF8;

        Http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", OpenAiApiKey);

        Console.WriteLine("🎤 Sesli Chatbot Başladı");
        Console.WriteLine("Konuşmak için Enter'a bas (Çıkmak için Ctrl+C)");

        using var synth = new SpeechSynthesizer();

        while (true)
        {
            Console.ReadLine();

            string audioFilePath = "recorded.wav";

            Console.WriteLine("🎙️ Konuşmaya başlayın (4 saniye kayıt)...");
            RecordAudio(audioFilePath, 4);

            Console.WriteLine("⏹️ Kayıt tamamlandı. Yazıya dönüştürülüyor...");

            string transcription = await TranscribeAudioAsync(audioFilePath);

            if (string.IsNullOrWhiteSpace(transcription))
            {
                Console.WriteLine("⚠️ Konuşma algılanamadı.");
                continue;
            }

            Console.WriteLine($"👤 Sen: {transcription}");

            string reply = await AskModelAsync(transcription);

            Console.WriteLine($"🤖 Bot: {reply}");

            synth.Speak(reply);
        }
    }

    static void RecordAudio(string outputFilePath, int seconds)
    {
        using var waveIn = new WaveInEvent
        {
            WaveFormat = new WaveFormat(16000, 1)
        };

        using var writer = new WaveFileWriter(outputFilePath, waveIn.WaveFormat);

        waveIn.DataAvailable += (s, a) =>
        {
            writer.Write(a.Buffer, 0, a.BytesRecorded);
            writer.Flush();
        };

        waveIn.StartRecording();
        Thread.Sleep(TimeSpan.FromSeconds(seconds));
        waveIn.StopRecording();
    }

    static async Task<string> TranscribeAudioAsync(string audioFilePath)
    {
        const string url = "https://api.openai.com/v1/audio/transcriptions";

        using var form = new MultipartFormDataContent();
        await using var fs = File.OpenRead(audioFilePath);

        form.Add(new StreamContent(fs), "file", Path.GetFileName(audioFilePath));
        form.Add(new StringContent("gpt-4o-mini-transcribe"), "model");

        var response = await Http.PostAsync(url, form);
        var responseText = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            return "Transcription hata: " + responseText;

        using var doc = JsonDocument.Parse(responseText);
        return doc.RootElement.GetProperty("text").GetString() ?? "";
    }

    static async Task<string> AskModelAsync(string userMessage)
    {
        const string url = "https://api.openai.com/v1/responses";

        var payload = new
        {
            model = "gpt-4.1-mini",
            input = new object[]
            {
                new {
                    role = "system",
                    content = "Sen yardımcı bir asistansın. Kısa ve net cevap ver."
                },
                new {
                    role = "user",
                    content = userMessage
                }
            }
        };

        var content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json"
        );

        var response = await Http.PostAsync(url, content);
        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            return "Chat hata: " + json;

        using var doc = JsonDocument.Parse(json);

        if (doc.RootElement.TryGetProperty("output_text", out var outputText))
            return outputText.GetString() ?? "";

        return "";
    }
}