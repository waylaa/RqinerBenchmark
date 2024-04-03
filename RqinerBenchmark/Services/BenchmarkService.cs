using Microsoft.Extensions.Hosting;
using Spectre.Console;
using System.Diagnostics;

namespace RqinerBenchmark.Services;

internal sealed class BenchmarkService(DirectoryInfo miners, TimeSpan duration) : BackgroundService
{
    private static readonly Dictionary<string, string> _results = [];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        foreach (FileInfo miner in miners.EnumerateFiles("*.exe").Where(x => x.Name.Contains("rqiner")))
        {
            string name = Path.GetFileNameWithoutExtension(miner.Name);
            AnsiConsole.Markup($"Benchmarking {name} ");

            using Process process = new();
            process.StartInfo.FileName = miner.FullName;
            process.StartInfo.Arguments = $"--threads {Environment.ProcessorCount / 2} --bench";
            process.StartInfo.RedirectStandardError = true;
            process.Start();

            using CancellationTokenSource timeout = new(duration);
            using CancellationTokenSource linked = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, timeout.Token);

            try
            {
                await process.WaitForExitAsync(linked.Token);
            }
            catch
            {
                process.Kill();
            }

            string output = await process.StandardError.ReadToEndAsync(stoppingToken);

            ReadOnlyMemory<char> average = output
                .Split('\n')
                .Where(log => log.Any(char.IsDigit)) // Discard any log that is empty.
                .Last() // Get the last log message containing the latest average it/s.
                .Split('|')
                .Last() // Get part of log that displays the average it/s.
                .Skip("  Average(10):".Length)
                .TakeWhile(x => x == '.' || char.IsDigit(x)) // Get the value of the average it/s and discard all other characters.
                .Select(x => x.ToString())
                .Aggregate((x1, x2) => x1 + x2) // Join every digit (and dot) together.
                .AsMemory();

            _results.Add(name, average.Span.ToString());
            AnsiConsole.MarkupLine(ToGreenText("DONE"));
        }

        Table performanceTable = new();
        performanceTable.AddColumns("Miner", "Performance");

        string bestPerformance = _results.Values.Max()!;

        foreach (KeyValuePair<string, string> result in _results)
        {
            if (result.Value == bestPerformance)
            {
                performanceTable.AddRow(result.Key, ToGreenText($"{result.Value} it/s"));
            }
            else
            {
                performanceTable.AddRow(result.Key, $"{result.Value} it/s");
            }
        }

        AnsiConsole.Write(performanceTable);
        AnsiConsole.WriteLine("You can now close this window.");
    }

    private static string ToGreenText(string value)
        => $"[green]{value}[/]";
}
