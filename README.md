# ModelingEvolution.RateCounters

Lightweight rate counting utilities for .NET: thread-safe rate counters for FPS, throughput, and request rate measurement.

## Installation

```bash
dotnet add package ModelingEvolution.RateCounters
```

## RateCounter

Thread-safe rate counter that measures events per second over a configurable window.

```csharp
using ModelingEvolution.RateCounters;

// FPS counter
var fps = new RateCounter();

// In your frame loop:
fps.Tick();  // or: fps++;
Console.WriteLine($"FPS: {fps.Rate:F1}");

// Custom window (default: 5 seconds)
var counter = new RateCounter { MeasureWindow = TimeSpan.FromSeconds(2) };

// Explicit double conversion
double currentRate = (double)counter;
```

### Thread Safety

`RateCounter` is fully thread-safe. Multiple threads can call `Tick()` concurrently, and `Rate` can be read from any thread at any time.

## License

MIT
