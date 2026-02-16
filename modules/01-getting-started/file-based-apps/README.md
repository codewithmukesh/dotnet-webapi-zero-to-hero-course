# File-Based Apps in .NET 10

Run C# code directly from a single file — no project file needed!

## Article

**[File-Based Apps in .NET 10 - Run C# Without a Project File](https://codewithmukesh.com/blog/file-based-apps-dotnet-10/)** - Part of the [.NET Web API Zero to Hero](https://codewithmukesh.com/courses/dotnet-webapi-zero-to-hero/) course.

## Prerequisites

- .NET 10 SDK

## Examples

### 1. Hello World (`hello.cs`)

Basic file-based app demonstrating the simplest possible example.

```bash
dotnet hello.cs
```

### 2. System Info (`sysinfo.cs`)

Displays system information using built-in .NET APIs. No external packages needed.

```bash
dotnet sysinfo.cs
```

### 3. Password Generator (`passgen.cs`)

Generates secure random passwords with configurable length.

```bash
# Default 16 characters
dotnet passgen.cs

# Custom length
dotnet passgen.cs -- 24
dotnet passgen.cs -- 32
```

### 4. Data Processor (`data-processor.cs`)

Demonstrates NuGet package usage with `#:package` directive. Reads JSON, processes data, and outputs CSV.

```bash
dotnet data-processor.cs
```

Reads `sales.json` and outputs `summary.csv`.

### 5. File Stats CLI (`file-stats.cs`)

A proper command-line tool with argument parsing using System.CommandLine.

```bash
# Show help
dotnet file-stats.cs -- --help

# Analyze a file
dotnet file-stats.cs -- --file README.md

# With verbose output
dotnet file-stats.cs -- --file README.md --verbose
```

## Key Directives

| Directive | Purpose | Example |
|-----------|---------|---------|
| `#:package` | Add NuGet packages | `#:package Newtonsoft.Json@13.0.3` |
| `#:sdk` | Specify SDK | `#:sdk Microsoft.NET.Sdk.Web` |
| `#:property` | Set MSBuild properties | `#:property LangVersion=preview` |
| `#:project` | Reference other projects | `#:project ../Shared/Shared.csproj` |

## Converting to a Full Project

When your file-based app grows, convert it:

```bash
dotnet project convert data-processor.cs
```

This creates a proper project structure with a `.csproj` file.

## Learn More

- [Official Documentation](https://learn.microsoft.com/en-us/dotnet/core/sdk/file-based-apps)
- [Full Article](https://codewithmukesh.com/blog/file-based-apps-dotnet-10/)
