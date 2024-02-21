using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Text;

class Program
{
    static DateTime lastFrameTime = DateTime.Now;

    static void Main(string[] args)
    {
        while (true) { 
            try { 
                Console.WriteLine($"{DateTime.Now} Starting capture.");

                var capture = new VideoCapture(0); // 0 for the default camera
                capture.ImageGrabbed += ProcessFrame;
                capture.Start();
                lastFrameTime = DateTime.Now;

                while ((DateTime.Now - lastFrameTime) < TimeSpan.FromSeconds(5))
                {
                    Thread.Sleep(500);
                }

                Console.WriteLine($"{DateTime.Now} No frame for more than 5 seconds. Restarting stream.");
                capture.Stop();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{DateTime.Now} An error occurred in the capture. Restarting stream.");
                Console.Error.WriteLine(ex.ToString());
            }
        }
    }

    private static void ProcessFrame(object? sender, EventArgs e)
    {
        try { 
            if (sender is not VideoCapture capture) 
                return;

            using var frame = new Mat();
            capture.Retrieve(frame);
            var image = frame.ToImage<Bgr, byte>();
            lastFrameTime = DateTime.Now;

            DetectAndDisplayQRCode(image);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Error while processing frame: " + ex.Message);
        }
    }

    private static string? lastDetectedCode = null;
    private static DateTime lastDetectionTime = DateTime.MinValue;

    private static void DetectAndDisplayQRCode(Image<Bgr, byte> image)
    {

        var points = new Mat(1, 2, DepthType.Cv32S, 1);

        using var qrDetector = new QRCodeDetector();
        var detected = qrDetector.Detect(image, points);
        if (!detected) 
            return;

        var decodedText = qrDetector.Decode(image, points);
        if (string.IsNullOrEmpty(decodedText))
        {
        //    Console.WriteLine($"QR Code Detected but could not be read.");
            return;
        }

        if (decodedText != lastDetectedCode || (DateTime.Now - lastDetectionTime) > TimeSpan.FromMilliseconds(2))
        {
            lastDetectedCode = decodedText;
            lastDetectionTime = DateTime.Now;
            Console.WriteLine($"{DateTime.Now} QR Code Detected: {decodedText}");

            SendCodeAsync(deviceId: "1", code: decodedText);
        }
    }

    private static async void SendCodeAsync(string deviceId, string code)
    {
        try
        {
            var client = new HttpClient();
            var formData = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("device", deviceId),
            new KeyValuePair<string, string>("text", code)
        };
            var content = new FormUrlEncodedContent(formData);
            await client.PostAsync("http://bigtyre8.bigtyre.local:8084/qr-handle.php", content);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to send code: {ex.Message}");
        }
    }
}
