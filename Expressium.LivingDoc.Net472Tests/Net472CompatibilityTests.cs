using System;
using System.IO;
using Expressium.LivingDoc;

namespace Expressium.LivingDoc.Net472Tests
{
    public class Net472CompatibilityTests
    {
        [Test]
        public void ConvertsCucumberMessagesOnNet472()
        {
            var project = new LivingDocConverter().Convert(GetSamplePath(), "net472 smoke test");

            Assert.That(project, Is.Not.Null);
            Assert.That(project.Features.Count, Is.GreaterThan(0));
            Assert.That(project.Title, Is.EqualTo("net472 smoke test"));
        }

        [Test]
        public void GeneratesAndReloadsReportOnNet472()
        {
            var outputDirectory = Path.Combine(Path.GetTempPath(), "ExpressiumLivingDoc-Net472-" + Guid.NewGuid().ToString("N"));
            var outputPath = Path.Combine(outputDirectory, "LivingDoc.html");

            try
            {
                Directory.CreateDirectory(outputDirectory);
                var converter = new LivingDocConverter();
                var project = converter.Convert(GetSamplePath(), "net472 report smoke test");

                converter.Generate(project, outputPath);

                Assert.That(File.Exists(outputPath), Is.True);
                Assert.That(File.ReadAllText(outputPath), Does.Contain("net472 report smoke test"));
            }
            finally
            {
                if (Directory.Exists(outputDirectory))
                    Directory.Delete(outputDirectory, true);
            }
        }

        private static string GetSamplePath()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Samples", "coffeeshop.ndjson");
        }
    }
}
