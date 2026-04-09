## Pipeline Overview

Beacon uses a pipeline model for processing monitoring results.

## Configuration

Configure your monitoring pipeline in `Program.cs`:

```csharp
builder.Services.AddBeacon(options =>
{
    options.Pipeline
        .AddStep<ValidationStep>()
        .AddStep<ThrottlingStep>()
        .AddStep<AlertingStep>();
});
```

## Pipeline Steps

Each step processes the monitoring result and can modify, filter, or trigger actions.
