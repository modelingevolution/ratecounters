using FluentAssertions;
using Xunit;

namespace ModelingEvolution.RateCounters.Tests;

public class RateCounterTests
{
    [Fact]
    public void Rate_BeforeFirstWindow_ReturnsZero()
    {
        var counter = new RateCounter { MeasureWindow = TimeSpan.FromSeconds(10) };

        counter.Tick();
        counter.Tick();

        counter.Rate.Should().Be(0.0);
    }

    [Fact]
    public void Rate_AfterWindow_ReturnsApproximateRate()
    {
        var counter = new RateCounter { MeasureWindow = TimeSpan.FromMilliseconds(100) };

        // Tick at ~10ms intervals for 150ms to ensure at least one window completes
        var sw = System.Diagnostics.Stopwatch.StartNew();
        while (sw.ElapsedMilliseconds < 150)
        {
            counter.Tick();
            Thread.Sleep(5);
        }

        counter.Rate.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Reset_ClearsRate()
    {
        var counter = new RateCounter { MeasureWindow = TimeSpan.FromMilliseconds(50) };

        // Generate some rate
        var sw = System.Diagnostics.Stopwatch.StartNew();
        while (sw.ElapsedMilliseconds < 100)
        {
            counter.Tick();
            Thread.Sleep(1);
        }

        counter.Reset();

        counter.Rate.Should().Be(0.0);
    }

    [Fact]
    public void IncrementOperator_Works()
    {
        var counter = new RateCounter { MeasureWindow = TimeSpan.FromMilliseconds(50) };

        var sw = System.Diagnostics.Stopwatch.StartNew();
        while (sw.ElapsedMilliseconds < 100)
        {
            counter++;
            Thread.Sleep(1);
        }

        counter.Rate.Should().BeGreaterThan(0);
    }

    [Fact]
    public void ExplicitDoubleConversion_ReturnsRate()
    {
        var counter = new RateCounter();
        double rate = (double)counter;
        rate.Should().Be(0.0);
    }

    [Fact]
    public void ToString_FormatsWithUnit()
    {
        var counter = new RateCounter();
        counter.ToString().Should().Be("0.0/s");
    }

    [Fact]
    public void ThreadSafety_ConcurrentTicks_DoNotThrow()
    {
        var counter = new RateCounter { MeasureWindow = TimeSpan.FromMilliseconds(50) };
        var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(500));

        var tasks = Enumerable.Range(0, 4).Select(_ => Task.Run(() =>
        {
            while (!cts.Token.IsCancellationRequested)
            {
                counter.Tick();
            }
        })).ToArray();

        var act = () => Task.WaitAll(tasks);
        act.Should().NotThrow();

        counter.Rate.Should().BeGreaterThan(0);
    }
}
