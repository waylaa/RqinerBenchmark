[![Build and Release](https://github.com/waylaa/RqinerBenchmark/actions/workflows/dotnet.yml/badge.svg)](https://github.com/waylaa/RqinerBenchmark/actions/workflows/dotnet.yml)

# RqinerBenchmark
A small console program for automating
[rqiner](https://github.com/Qubic-Solutions/rqiner-builds) benchmarks. Mainly
created for selecting a variant of the miner with the best performance (it/s).

## Usage
ℹ️ It is highly recommended to download every latest available variant of [rqiner](https://github.com/Qubic-Solutions/rqiner-builds) for your OS.

1. Get the latest build from the
[Releases](https://github.com/waylaa/RqinerBenchmark/releases) page.
2. Open your preferred terminal and run the appropriate command from below.
   
⚠️ Make sure to change the values of the given argument examples below. '--miners' must point
to a folder containing the miner executables, '--id' should be your payout ID, '--label' can be
whatever you want, '--threads' is the amount of threads that will be used by the benchmark and 
'--duration' must specify a time format like this: 'hours:minutes:seconds'. Minimum duration
allowed is 10 seconds. A 'help' command is also included by specifying only '--help'.

### Windows

```
.\RqinerBenchmark.exe --miners "folder\path\to\rqiner\variants" --id SLWRNGFJFVFLHEERCSPJPYSGGKOBMJDGCKDZGJLJXBIHAYOVSJQFYKLDMZYC --label miner_1 --threads 12 --duration 00:00:15
```

### MacOS/Linux
```
.\RqinerBenchmark --miners "folder/path/to/rqiner/variants" --id SLWRNGFJFVFLHEERCSPJPYSGGKOBMJDGCKDZGJLJXBIHAYOVSJQFYKLDMZYC --label miner_1 --threads 12 --duration 00:00:15
```

## Building/Contributing
### 1. Prerequisites (Visual Studio Installer)
  - .NET desktop development
    
### 2. Install .NET 8
- Make sure you have .NET 8 installed on your machine.
- If not installed, download and install it from [here](https://dotnet.microsoft.com/en-us/download/dotnet/8.0).

### 3. Clone
- Open your terminal and navigate to the directory where you want to clone 
this repository.

- Clone 
```
git clone https://github.com/waylaa/RqinerBenchmark
```

- Open the .sln file, restore the nuget packages and you're done.

## License
[MIT](https://choosealicense.com/licenses/mit/)
