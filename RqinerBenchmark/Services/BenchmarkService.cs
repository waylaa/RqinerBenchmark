﻿using Microsoft.Extensions.Hosting;
using Spectre.Console;
using System.Diagnostics;

namespace RqinerBenchmark.Services;

internal sealed class BenchmarkService(DirectoryInfo miners, int threads, TimeSpan duration) : BackgroundService
{
    private static readonly Dictionary<string, double> _results = [];

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

        foreach (FileInfo miner in miners.EnumerateFiles("*.exe").Where(x => x.Name.Contains("rqiner")))
        {
            string name = Path.GetFileNameWithoutExtension(miner.Name);
            AnsiConsole.Markup($"Benchmarking {name} ");

            using Process process = new();
            process.StartInfo.FileName = miner.FullName;
            process.StartInfo.Arguments = $"--threads {threads} --bench";
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

            string output = await process.StandardError.ReadToEndAsync(stoppingToken);

            if (!output.Contains("Average(10):"))
            {
                _results.Add(name, 0.0);
                AnsiConsole.MarkupLine(WriteError("DNF"));

                continue;
            }

            string averageIts = output
                .Split('\n')
                .Where(log => log.Any(char.IsDigit)) // Discard any log that is empty.
                .Last() // Get the last log message containing the latest average it/s.
                .Split('|')
                .Last() // Get part of log that displays the average it/s.
                .Skip("  Average(10):".Length)
                .TakeWhile(x => x == '.' || char.IsDigit(x)) // Get the value of the average it/s and discard all other characters.
                .Select(x => x.ToString())
                .Aggregate((x1, x2) => x1 + x2); // Join every digit (and dot) together.

            _results.Add(name, double.Parse(averageIts));
            AnsiConsole.MarkupLine(WriteSuccessful("DONE"));
        }

        Table performanceTable = new();
        performanceTable.AddColumns("Miner", "Performance");

        double bestPerformance = _results.Values.Max();

        foreach (KeyValuePair<string, double> result in _results)
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
