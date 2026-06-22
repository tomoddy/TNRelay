using TNRelay;
using TNRelay.Config;
using TNRelay.Models;

Config config = Config.Load();

AlertStore alertStore = new(config.Postgres);
RelayClient relayClient = new(config.Relay);

if (!Enum.TryParse(config.TrueNas.MinSeverity, ignoreCase: true, out AlertLevel minSeverity))
    throw new Exception($"Invalid MinSeverity value: \"{config.TrueNas.MinSeverity}\"");

int forwardedCount = 0;
int errorCount = 0;

foreach (TrueNasSource source in config.TrueNas.Sources)
{
    try
    {
        TrueNasClient client = new(source);
        List<TrueNasAlert> alerts = await client.GetAlertsAsync();

        foreach (TrueNasAlert alert in alerts)
        {
            if (alert.Dismissed)
                continue;

            if (!Enum.TryParse(alert.Level, ignoreCase: true, out AlertLevel alertLevel))
                continue;

            if (alertLevel < minSeverity)
                continue;

            if (await alertStore.HasBeenForwardedAsync(source.Name, alert.Uuid))
                continue;

            await relayClient.SendAsync(source.Name, alert);
            await alertStore.MarkForwardedAsync(source.Name, alert.Uuid);
            forwardedCount++;
        }
    }
    catch (Exception ex)
    {
        errorCount++;
        Console.WriteLine($"Error processing source \"{source.Name}\": {ex.Message}");
    }
}

Console.WriteLine($"TNRelay complete: {forwardedCount} alert(s) forwarded, {errorCount} source error(s)");
return errorCount;