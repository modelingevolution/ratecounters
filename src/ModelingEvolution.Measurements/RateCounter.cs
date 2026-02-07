using System.Diagnostics;

namespace ModelingEvolution.Measurements;

/// <summary>
/// Thread-safe rate counter that measures events per second over a configurable window.
/// Typical usage: FPS counting, message throughput, request rates.
///
/// <example>
/// <code>
/// var fps = new RateCounter();
/// // In your frame loop:
/// fps.Tick();
/// Console.WriteLine($"FPS: {fps.Rate:F1}");
///
/// // Or use ++ operator:
/// fps++;
/// double currentFps = (double)fps;
/// </code>
/// </example>
/// </summary>
public class RateCounter
{
    private readonly Stopwatch _sw = Stopwatch.StartNew();
    private long _count;
    private double _rate;

    /// <summary>
    /// The time window over which the rate is calculated.
    /// After each window elapses, the rate is updated and the counter resets.
    /// Default: 5 seconds.
    /// </summary>
    public TimeSpan MeasureWindow { get; init; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// The current measured rate (events per second).
    /// Updated at the end of each <see cref="MeasureWindow"/>.
    /// Returns 0.0 until the first window completes.
    /// </summary>
    public double Rate => Volatile.Read(ref _rate);

    /// <summary>
    /// Records a single event and recalculates the rate if the measure window has elapsed.
    /// Thread-safe: may be called from any thread.
    /// </summary>
    public void Tick()
    {
        var newCount = Interlocked.Increment(ref _count);
        var elapsed = _sw.Elapsed;

        if (elapsed < MeasureWindow) return;

        // Attempt to claim the reset. Only one thread wins.
        var captured = Interlocked.Exchange(ref _count, 0);
        if (captured > 0)
        {
            Volatile.Write(ref _rate, captured * 1000.0 / elapsed.TotalMilliseconds);
            _sw.Restart();
        }
    }

    /// <summary>
    /// Resets the counter and rate to zero.
    /// </summary>
    public void Reset()
    {
        Interlocked.Exchange(ref _count, 0);
        Volatile.Write(ref _rate, 0);
        _sw.Restart();
    }

    /// <summary>
    /// Records a single event. Equivalent to <see cref="Tick"/>.
    /// </summary>
    public static RateCounter operator ++(RateCounter counter)
    {
        counter.Tick();
        return counter;
    }

    /// <summary>
    /// Converts to the current rate value.
    /// </summary>
    public static explicit operator double(RateCounter counter) => counter.Rate;

    public override string ToString() => $"{Rate:F1}/s";
}
