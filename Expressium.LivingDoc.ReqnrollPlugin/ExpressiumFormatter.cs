using Reqnroll.Formatters.Configuration;
using Reqnroll.Formatters.RuntimeSupport;
using Reqnroll.Utils;
using Expressium.LivingDoc.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace Expressium.LivingDoc.ReqnrollPlugin
{
    public class ExpressiumFormatter : ExpressiumMessageFormatter
    {
        public string OutputFilePath { get; private set; }
        public string OutputFileTitle { get; private set; }
        public string HistoryPath { get; private set; }
        public LivingDocReportOptions ReportOptions { get; private set; }

        public ExpressiumFormatter(IFormattersConfigurationProvider configurationProvider, IFormatterLog logger, IFileSystem fileSystem) : base(configurationProvider, logger, fileSystem, "expressium")
        {
        }

        protected override void FinalizeInitialization(string outputPath, IDictionary<string, object> formatterConfiguration, Action<bool> onInitialized)
        {
            base.FinalizeInitialization(outputPath, formatterConfiguration, onInitialized);
            OutputFilePath = outputPath;

            if (formatterConfiguration.ContainsKey("outputFileTitle"))
                OutputFileTitle = formatterConfiguration["outputFileTitle"].ToString();

            if (formatterConfiguration.ContainsKey("historyPath"))
                HistoryPath = formatterConfiguration["historyPath"].ToString();

            ReportOptions = new LivingDocReportOptions
            {
                OutputDirectory = GetConfigurationValue(formatterConfiguration, "outputDirectory", Path.GetDirectoryName(Path.GetFullPath(OutputFilePath))),
                OutputFileName = GetConfigurationValue(formatterConfiguration, "outputFileName", Path.ChangeExtension(Path.GetFileName(OutputFilePath), ".html")),
                MergeWithHistory = GetBooleanConfigurationValue(formatterConfiguration, "mergeWithHistory", true)
            };
        }

        public override void Dispose()
        {
            base.Dispose();

            if (!string.IsNullOrWhiteSpace(HistoryPath) && ReportOptions.MergeWithHistory)
            {
                var livingDocConverter = new LivingDocConverter();
                var livingDocProject = livingDocConverter.Convert(OutputFilePath, OutputFileTitle);

                var historyDirectory = Path.GetDirectoryName(Path.GetFullPath(HistoryPath));
                if (!Directory.Exists(historyDirectory))
                    Directory.CreateDirectory(historyDirectory);

                var historyFileName = Path.Combine(historyDirectory, livingDocProject.Date.ToString("yyyyMMddHHmmss") + ".ndjson");
                File.Copy(OutputFilePath, historyFileName, true);

                livingDocConverter.MergeHistory(livingDocProject, HistoryPath);

                livingDocConverter.GenerateWithOptions(livingDocProject, ReportOptions);
            }
            else
            {
                var livingDocConverter = new LivingDocConverter();
                var livingDocProject = livingDocConverter.Convert(OutputFilePath, OutputFileTitle);
                livingDocConverter.GenerateWithOptions(livingDocProject, ReportOptions);
            }
        }

        private static string GetConfigurationValue(IDictionary<string, object> configuration, string key, string defaultValue)
        {
            return configuration.ContainsKey(key) && configuration[key] != null
                ? configuration[key].ToString()
                : defaultValue;
        }

        private static bool GetBooleanConfigurationValue(IDictionary<string, object> configuration, string key, bool defaultValue)
        {
            var value = GetConfigurationValue(configuration, key, defaultValue.ToString());
            return bool.TryParse(value, out var result) ? result : defaultValue;
        }
    }
}
