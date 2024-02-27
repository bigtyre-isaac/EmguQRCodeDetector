using BigTyre.QR;
using Microsoft.Extensions.Configuration;

var builder = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .AddUserSecrets<Program>(optional: true);

var configuration = builder.Build();

var settings = new AppSettings();
configuration.Bind(settings);

var deviceId = settings.DeviceId;
if (deviceId == default) throw new Exception("Device ID not configured.");

Uri detectionUri;
var detectionUriString = settings.DetectionUri;
if (Uri.TryCreate(detectionUriString, UriKind.Absolute, out var tempUri))
{
    detectionUri = tempUri;
}
else
{
    throw new Exception("Invalid DetectionUri in configuration. Please check your settings.");
}

var detector = new BigTyreQRCodeDetector(deviceId, detectionUri);

detector.Run();

