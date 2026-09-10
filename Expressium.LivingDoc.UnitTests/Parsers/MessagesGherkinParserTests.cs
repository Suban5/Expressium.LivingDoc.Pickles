using Expressium.LivingDoc.Models;
using Expressium.LivingDoc.Parsers;
using Io.Cucumber.Messages.Types;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Expressium.LivingDoc.UnitTests.Parsers
{
    internal class MessagesGherkinParserTests
    {
        [Test]
        public void ParseScenario_PreservesEachExamplesSectionSeparately()
        {
            var feature = new LivingDocFeature();
            var scenario = new Scenario
            (
                new Location(2, 1),
                new List<Tag>(),
                "Scenario Outline",
                "Login",
                string.Empty,
                new List<Step>
                {
                    new Step(new Location(3, 3), "Given ", default(StepKeywordType), "I enter <username>", null, null, "step-1")
                },
                new List<Examples>
                {
                    CreateExamples(4, "Valid users", "username", "alice"),
                    CreateExamples(9, "Locked users", "username", "locked")
                },
                "scenario-1"
            );

            MessagesGherkinParser.ParseScenario(feature, scenario);

            var parsedScenario = feature.Scenarios[0];

            Assert.That(parsedScenario.Keyword, Is.EqualTo("Scenario Outline"));
            Assert.That(parsedScenario.Examples, Has.Count.EqualTo(1));
            Assert.That(parsedScenario.DocumentationExamples, Has.Count.EqualTo(2));
            Assert.That(parsedScenario.DocumentationExamples[0].Name, Is.EqualTo("Valid users"));
            Assert.That(parsedScenario.DocumentationExamples[1].Name, Is.EqualTo("Locked users"));
            Assert.That(parsedScenario.DocumentationExamples[0].DataTable.Rows[0].Cells[0], Is.EqualTo("username"));
            Assert.That(parsedScenario.DocumentationExamples[1].DataTable.Rows[1].Cells[0], Is.EqualTo("locked"));
            Assert.That(parsedScenario.DocumentationExamples[0].Steps[0].Name, Is.EqualTo("I enter &lt;username&gt;"));
            Assert.That(parsedScenario.DocumentationExamples[1].Steps[0].Name, Is.EqualTo("I enter &lt;username&gt;"));
        }

        [Test]
        public void ParseGherkinDocuments_AssignsCommentsToSourceLocations()
        {
            var inputFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Samples", "background.ndjson");
            var messagesParser = new MessagesParser();
            var project = messagesParser.ConvertToLivingDoc(inputFilePath);

            var feature = project.Features[0];

            Assert.That(feature.Background.Comments, Does.Contain("# set up bank account balance"));
        }

        [Test]
        public void ParseGherkinDocuments_AssignsCommentsToExamplesSections()
        {
            var inputFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Samples", "coffeeshop.ndjson");
            var messagesParser = new MessagesParser();
            var project = messagesParser.ConvertToLivingDoc(inputFilePath);

            var feature = project.Features.Single(candidate => candidate.Name == "Orders");
            var scenario = feature.Scenarios.Single(candidate => candidate.Name == "Ordering Coffee Confirmation Notification");

            Assert.That(scenario.DocumentationExamples[0].Comments, Does.Contain("#Examples: Valid Coffee Orders"));
            Assert.That(scenario.DocumentationExamples[0].Comments, Has.Some.Contains("#These properties represents the coffee prices"));
        }

        [Test]
        public void ParseScenario_PreservesOneHundredExampleColumns()
        {
            var feature = new LivingDocFeature();
            var scenario = new Scenario(
                new Location(2, 1),
                new List<Tag>(),
                "Scenario Outline",
                "Wide table",
                string.Empty,
                new List<Step>(),
                new List<Examples> { CreateExamples(4, "Wide", "column", "value", 100) },
                "scenario-wide");

            MessagesGherkinParser.ParseScenario(feature, scenario);

            Assert.That(feature.Scenarios[0].DocumentationExamples[0].DataTable.Rows, Has.Count.EqualTo(2));
            Assert.That(feature.Scenarios[0].DocumentationExamples[0].DataTable.Rows[0].Cells, Has.Count.EqualTo(100));
            Assert.That(feature.Scenarios[0].DocumentationExamples[0].DataTable.Rows[1].Cells, Has.Count.EqualTo(100));
        }

        private static Examples CreateExamples(long line, string name, string header, string value, int columnCount = 1)
        {
            var headers = Enumerable.Range(0, columnCount)
                .Select(index => new TableCell(new Location(line + 1, index + 5), index == 0 ? header : header + index))
                .ToList();
            var values = Enumerable.Range(0, columnCount)
                .Select(index => new TableCell(new Location(line + 2, index + 5), index == 0 ? value : value + index))
                .ToList();

            return new Examples(
                new Location(line, 1),
                new List<Tag>(),
                "Examples",
                name,
                string.Empty,
                new TableRow(
                    new Location(line + 1, 3),
                    headers,
                    "header-" + line),
                new List<TableRow>
                {
                    new TableRow(
                        new Location(line + 2, 3),
                        values,
                        "row-" + line)
                },
                "examples-" + line);
        }
    }
}
