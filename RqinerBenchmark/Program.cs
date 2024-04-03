using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RqinerBenchmark.Services;

namespace RqinerBenchmark;

internal sealed class Program
{
    /// <param name="miners">The directory of every rqiner variant.</param>
    /// <param name="duration">The duration of each benchmark. The format is '00:00:00' (hours:minutes:seconds).</param>
    private static Task Main(DirectoryInfo miners, TimeSpan duration)
    {
        Console.Title = "Rqiner Benchmark";

        return Host
            .CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.Configure<ConsoleLifetimeOptions>(options => options.SuppressStatusMessages = true);
                services.AddHostedService(x => new BenchmarkService(miners, duration));
            })
            .RunConsoleAsync();
    }
}
