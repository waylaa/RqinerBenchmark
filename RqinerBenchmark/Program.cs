using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RqinerBenchmark.Services;

namespace RqinerBenchmark;

internal sealed class Program
{
    /// <summary></summary>
    /// <param name="miners">The directory containing rqiner variants.</param>
    /// <param name="threads">The amount of threads to use.</param>
    /// <param name="duration">The duration of each benchmark. The format is '00:00:00' (hours:minutes:seconds).</param>
    /// <returns></returns>
    private static Task Main(DirectoryInfo miners, int threads, TimeSpan duration)
    {
        Console.Title = "Rqiner Benchmark";

        return Host
            .CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.Configure<ConsoleLifetimeOptions>(options => options.SuppressStatusMessages = true);
                services.AddHostedService(x => new BenchmarkService(miners, threads, duration));
            })
            .RunConsoleAsync();
    }
}
