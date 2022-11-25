using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.MSBuild;
using Xunit;
using Xunit.Abstractions;

namespace Analyzer.Test;

public class CompileTest
{
    private readonly ITestOutputHelper _output;

    public CompileTest(ITestOutputHelper output) =>
        _output = output;

    [Fact]
    public async void Test()
    {
        SomethingRegardingMSBuild();

        using var workspace = MSBuildWorkspace.Create();
        
        // Print message for WorkspaceFailed event to help diagnosing project load failures.
        workspace.WorkspaceFailed += (_, e) => _output.WriteLine(e.Diagnostic.Message);

        var path = Path.Join("..", "..", "..", "..", "Examples", "Examples.csproj");
        var project = await workspace.OpenProjectAsync(path, new TestProgress(_output));

        var compilation = await project.GetCompilationAsync();

        var analyzers = new CompilationWithAnalyzers(compilation, new[] { new AnalyzerAnalyzer() }.ToImmutableArray<DiagnosticAnalyzer>(),
            new AnalyzerOptions(ImmutableArray<AdditionalText>.Empty), CancellationToken.None);

        var result = await analyzers.GetAnalysisResultAsync(CancellationToken.None);
    }

    private static void SomethingRegardingMSBuild()
    {
        // Copied from App/Program.cs
        var visualStudioInstances = MSBuildLocator.QueryVisualStudioInstances().ToArray();
        var instance = visualStudioInstances.First();

        // NOTE: Be sure to register an instance with the MSBuildLocator 
        //       before calling MSBuildWorkspace.Create()
        //       otherwise, MSBuildWorkspace won't MEF compose.
        MSBuildLocator.RegisterInstance(instance);
    }

    private class TestProgress : IProgress<ProjectLoadProgress>
    {
        private readonly ITestOutputHelper _output;

        public TestProgress(ITestOutputHelper output) => _output = output;

        void IProgress<ProjectLoadProgress>.Report(ProjectLoadProgress value)
        {
            // Copied from App/Program.cs
            var projectDisplay = Path.GetFileName(value.FilePath);
            if (value.TargetFramework != null)
            {
                projectDisplay += $" ({value.TargetFramework})";
            }

            _output.WriteLine($"{value.Operation,-15} {value.ElapsedTime,-15:m\\:ss\\.fffffff} {projectDisplay}");
        }
    }
}