using Microsoft.Extensions.Hosting;
using Spectre.Console;
using System.Diagnostics;

namespace RqinerBenchmark.Services;

internal sealed class BenchmarkService
(
    DirectoryInfo miners,
    string id,
    string label,
    int threads,
    TimeSpan duration
) : BackgroundService
{
    private static readonly Dictionary<string, float> _results = [];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!miners.Exists)
        {
            AnsiConsole.MarkupLine(WriteError("Directory does not exist."));
            return;
        }

        if (threads < 1 || threads > Environment.ProcessorCount)
        {
            AnsiConsole.MarkupLine(WriteError($"Invalid thread count. Actual range is 1 - {Environment.ProcessorCount}"));
            return;
        }

        if (duration == TimeSpan.Zero)
        {
            AnsiConsole.MarkupLine(WriteError("Invalid duration. Make sure its format is as follows: 00:00:00 (hours:minutes:seconds)"));
            return;
        }

        if (duration == TimeSpan.FromSeconds(10))
        {
            AnsiConsole.MarkupLine(WriteError("A minimum of 10 seconds is allowed for benchmarking."));
            return;
        }

        foreach (FileInfo miner in miners
            .EnumerateFiles("*.exe")
            .Where(x => x.Name.Contains("rqiner") && !x.Name.Contains("cuda")))
        {
            string name = Path.GetFileNameWithoutExtension(miner.Name);
            AnsiConsole.Markup($"Benchmarking {name} ");

            using Process process = new();
            process.StartInfo.FileName = miner.FullName;
            process.StartInfo.Arguments = $"--id {id} --label {label}_benchmark --threads {threads}";
            process.StartInfo.RedirectStandardError = true;
            process.Start();

            using CancellationTokenSource timeout = new(duration);
            using CancellationTokenSource stoppingTokenAndTimeout = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, timeout.Token);

            try
            {
                await process.WaitForExitAsync(stoppingTokenAndTimeout.Token);
            }
            catch
            {
                process.Kill();
            }

            string log = await process.StandardError.ReadToEndAsync(stoppingToken);

            if (!log.Contains("Iterrate:"))
            {
                _results.Add(name, 0);
                AnsiConsole.MarkupLine(WriteError("DNF"));

                continue;
            }

            float averageIts = log
                .Split('\n')
                .Where(line => line.Contains("Iterrate:"))
                .Select(line =>
                {
                    return line
                        .Split('|')[1] // Get part of the log that displays it/s.
                        .Split(':')
                        .Last() // Get the actual value.
                        .Trim()
                        .Split(' ')
                        .First();
                })
                .Average(float.Parse);

            _results.Add(name, averageIts);
            AnsiConsole.MarkupLine(WriteSuccessful("DONE"));
        }

        Table performanceTable = new();
        performanceTable.AddColumns("Miner", "Performance");

        float bestPerformance = _results.Values.Max();

        foreach (KeyValuePair<string, float> result in _results)
        {
            if (result.Value == bestPerformance)
            {
                performanceTable.AddRow(result.Key, WriteSuccessful($"{result.Value} it/s"));
            }
            else
            {
                performanceTable.AddRow(result.Key, $"{result.Value} it/s");
            }
        }

        AnsiConsole.Write(performanceTable);
        AnsiConsole.WriteLine("You can now close this window.");

        static string WriteSuccessful(string value) => $"[green]{value}[/]";
        static string WriteError(string value) => $"[red]{value}[/]";
    }
}
