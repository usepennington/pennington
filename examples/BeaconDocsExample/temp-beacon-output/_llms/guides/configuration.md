## Monitor Configuration

:::tabs
```csharp [C#]
builder.Services.AddBeacon(options =>
{
    options.DefaultInterval = TimeSpan.FromMinutes(5);
    options.AlertThreshold = 3;
    options.TimeoutMs = 5000;
});
```

```json [JSON]
{
  "Beacon": {
    "DefaultInterval": "00:05:00",
    "AlertThreshold": 3,
    "TimeoutMs": 5000
  }
}
```
:::

> [!WARNING]
> Setting `TimeoutMs` below 1000 may cause false positives on slow networks.

## Alert Configuration

Alerts can be sent via email, Slack, or custom webhook:

```csharp
options.Alerts.Add(new SlackAlert("#ops-alerts", webhookUrl));
options.Alerts.Add(new EmailAlert("oncall@example.com"));
```

See the [API Reference](xref:beacon.api-reference) for all alert types.
