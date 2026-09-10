using Expressium.LivingDoc.Models;
using System;
using System.IO;

namespace Expressium.LivingDoc.UnitTests.Models
{
    public class LivingDocReportOptionsTests
    {
        [Test]
        public void Defaults_UseCurrentDirectoryAndLivingDocFileName()
        {
            var options = new LivingDocReportOptions();

            Assert.That(options.OutputDirectory, Is.EqualTo("."));
            Assert.That(options.OutputFileName, Is.EqualTo("LivingDoc.html"));
            Assert.That(options.MergeWithHistory, Is.True);
            Assert.That(options.ResolveOutputPath(), Is.EqualTo(Path.Combine(Directory.GetCurrentDirectory(), "LivingDoc.html")));
        }

        [Test]
        public void ResolveOutputPath_CombinesRelativeDirectoryAndFileName()
        {
            var baseDirectory = Path.Combine(Path.GetTempPath(), "expressium-options");
            var options = new LivingDocReportOptions
            {
                OutputDirectory = Path.Combine("reports", "living-doc"),
                OutputFileName = "report.html"
            };

            var result = options.ResolveOutputPath(baseDirectory);

            Assert.That(result, Is.EqualTo(Path.Combine(baseDirectory, "reports", "living-doc", "report.html")));
        }

        [Test]
        public void ResolveOutputPath_PreservesAbsoluteDirectory()
        {
            var absoluteDirectory = Path.Combine(Path.GetTempPath(), "expressium-absolute");
            var options = new LivingDocReportOptions
            {
                OutputDirectory = absoluteDirectory,
                OutputFileName = "report.html"
            };

            var result = options.ResolveOutputPath(Path.Combine(Path.GetTempPath(), "ignored"));

            Assert.That(result, Is.EqualTo(Path.Combine(absoluteDirectory, "report.html")));
        }
    }
}