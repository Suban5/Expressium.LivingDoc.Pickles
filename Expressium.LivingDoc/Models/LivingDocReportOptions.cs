using System;
using System.IO;

namespace Expressium.LivingDoc.Models
{
    public class LivingDocReportOptions
    {
        public string OutputDirectory { get; set; }
        public string OutputFileName { get; set; }
        public bool MergeWithHistory { get; set; }

        public LivingDocReportOptions()
        {
            OutputDirectory = ".";
            OutputFileName = "LivingDoc.html";
            MergeWithHistory = true;
        }

        public string ResolveOutputPath(string baseDirectory = null)
        {
            var rootDirectory = string.IsNullOrWhiteSpace(baseDirectory)
                ? Directory.GetCurrentDirectory()
                : baseDirectory;
            var outputDirectory = Path.IsPathRooted(OutputDirectory)
                ? OutputDirectory
                : Path.Combine(rootDirectory, OutputDirectory);

            return Path.GetFullPath(Path.Combine(outputDirectory, OutputFileName));
        }
    }
}