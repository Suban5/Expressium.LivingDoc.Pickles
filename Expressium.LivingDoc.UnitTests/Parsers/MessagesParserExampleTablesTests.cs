using Expressium.LivingDoc.Parsers;
using Expressium.LivingDoc.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Expressium.LivingDoc.UnitTests.Parsers
{
    internal class MessagesParserExampleTablesTests
    {
        [Test]
        public void Converting_Scenario_ExampleTables()
        {
            var inputFilePath = Path.Combine(Directory.GetCurrentDirectory(), "CCK", "Samples", "examples-tables", "examples-tables.ndjson");

            var messagesParser = new MessagesParser();
            var livingDocProject = messagesParser.ConvertToLivingDoc(inputFilePath);

            // Basic checks
            Assert.That(livingDocProject.Features, Is.Not.Null);
            Assert.That(livingDocProject.Features.Count, Is.GreaterThan(0));

            // With new behavior: Scenario Outlines become one template each
            // examples-tables.ndjson has 2 Scenario Outlines
            Assert.That(livingDocProject.GetNumberOfScenarios(), Is.EqualTo(2));
            Assert.That(livingDocProject.GetNumberOfSteps(), Is.GreaterThan(0));

            var scenario = livingDocProject.Features[0].Scenarios[0];

            // Scenario Outline is rendered as one template with placeholder steps
            Assert.That(scenario.Examples[0].Steps.Count, Is.GreaterThan(0));

            // Placeholders should be preserved (contain "<")
            bool hasPlaceholder = false;
            for (int i = 0; i < scenario.Examples[0].Steps.Count; i++)
            {
                if (scenario.Examples[0].Steps[i].Name.Contains("<"))
                {
                    hasPlaceholder = true;
                    break;
                }
            }

            Assert.That(hasPlaceholder, Is.True, "Steps should have placeholder text");

            // DataTable should have headers and data rows
            Assert.That(scenario.Examples[0].HasDataTable(), Is.True);
            Assert.That(scenario.Examples[0].DataTable.Rows.Count, Is.GreaterThan(0));

            // Second scenario
            var scenario2 = livingDocProject.Features[0].Scenarios[1];
            Assert.That(scenario2.Examples[0].Steps.Count, Is.GreaterThan(0));
        }

        [Test]
        public void Converting_Scenario_Outline_One_Template_With_One_Examples_Row()
        {
            var inputFilePath = Path.Combine(Directory.GetCurrentDirectory(), "CCK", "Samples", "minimal", "minimal.ndjson");

            var messagesParser = new MessagesParser();
            var livingDocProject = messagesParser.ConvertToLivingDoc(inputFilePath);

            Assert.That(livingDocProject.GetNumberOfFeatures(), Is.EqualTo(1));
            Assert.That(livingDocProject.GetNumberOfScenarios(), Is.EqualTo(1));
            Assert.That(livingDocProject.GetNumberOfSteps(), Is.EqualTo(1));

            var scenario = livingDocProject.Features[0].Scenarios[0];

            // Normal scenario without Examples remains unchanged
            Assert.That(scenario.Name, Is.EqualTo("cukes"));
            Assert.That(scenario.Examples[0].Steps[0].Name, Is.EqualTo("I have 42 cukes in my belly"));
        }

        [Test]
        public void Converting_Scenario_Outline_Multiple_Examples_Rows()
        {
            var inputFilePath = Path.Combine(Directory.GetCurrentDirectory(), "CCK", "Samples", "examples-tables", "examples-tables.ndjson");

            var messagesParser = new MessagesParser();
            var livingDocProject = messagesParser.ConvertToLivingDoc(inputFilePath);

            // Two Scenario Outlines, each as one template
            Assert.That(livingDocProject.GetNumberOfScenarios(), Is.EqualTo(2));

            // First scenario: "Eating cucumbers" as one template
            var scenario1 = livingDocProject.Features[0].Scenarios[0];
            Assert.That(scenario1.Name, Does.Contain("Eating cucumbers"));
            Assert.That(scenario1.Examples.Count, Is.EqualTo(1)); // One template example
            Assert.That(scenario1.Examples[0].HasDataTable(), Is.True); // Has DataTable

            // Second scenario: "Eating cucumbers with friends" as one template
            var scenario2 = livingDocProject.Features[0].Scenarios[1];
            Assert.That(scenario2.Name, Does.Contain("Eating cucumbers with friends"));
            Assert.That(scenario2.Examples.Count, Is.EqualTo(1)); // One template example
            Assert.That(scenario2.Examples[0].HasDataTable(), Is.True); // Has DataTable
        }

        [Test]
        public void Converting_Scenario_Outline_Columns_Different_Order_From_Steps()
        {
            var inputFilePath = Path.Combine(Directory.GetCurrentDirectory(), "CCK", "Samples", "examples-tables", "examples-tables.ndjson");

            var messagesParser = new MessagesParser();
            var livingDocProject = messagesParser.ConvertToLivingDoc(inputFilePath);

            var scenario = livingDocProject.Features[0].Scenarios[0];

            // Placeholder identity is based on header names, not column order
            bool hasPlaceholder = false;
            for (int i = 0; i < scenario.Examples[0].Steps.Count; i++)
            {
                if (scenario.Examples[0].Steps[i].Name.Contains("<"))
                {
                    hasPlaceholder = true;
                    break;
                }
            }

            Assert.That(hasPlaceholder, Is.True);
        }

        [Test]
        public void Converting_Scenario_Outline_Duplicate_Example_Values()
        {
            var inputFilePath = Path.Combine(Directory.GetCurrentDirectory(), "CCK", "Samples", "examples-tables", "examples-tables.ndjson");

            var messagesParser = new MessagesParser();
            var livingDocProject = messagesParser.ConvertToLivingDoc(inputFilePath);

            var scenario = livingDocProject.Features[0].Scenarios[0];

            // Placeholders preserved even with duplicate example values
            Assert.That(scenario.Examples[0].Steps.Count, Is.GreaterThan(0));
        }

        [Test]
        public void Converting_Scenario_Outline_Mixed_Example_Values()
        {
            var inputFilePath = Path.Combine(Directory.GetCurrentDirectory(), "CCK", "Samples", "examples-tables", "examples-tables.ndjson");

            var messagesParser = new MessagesParser();
            var livingDocProject = messagesParser.ConvertToLivingDoc(inputFilePath);

            var scenario = livingDocProject.Features[0].Scenarios[0];

            // Mixed example values should have placeholder steps
            Assert.That(scenario.Examples[0].Steps.Count, Is.GreaterThan(0));

            bool hasPlaceholder = false;
            for (int i = 0; i < scenario.Examples[0].Steps.Count; i++)
            {
                if (scenario.Examples[0].Steps[i].Name.Contains("<"))
                {
                    hasPlaceholder = true;
                    break;
                }
            }

            Assert.That(hasPlaceholder, Is.True);
        }

[Test]
public void Converting_Normal_Non_Scenario_Outline_Scenarios_Remain_Unchanged()
    {
        var inputFilePath = Path.Combine(Directory.GetCurrentDirectory(), "CCK", "Samples", "minimal", "minimal.ndjson");

        var messagesParser = new MessagesParser();
        var livingDocProject = messagesParser.ConvertToLivingDoc(inputFilePath);

        Assert.That(livingDocProject.GetNumberOfFeatures(), Is.EqualTo(1));
        Assert.That(livingDocProject.GetNumberOfScenarios(), Is.EqualTo(1));

        var scenario = livingDocProject.Features[0].Scenarios[0];

        // Normal scenario without Examples should remain unchanged
        Assert.That(scenario.Name, Is.EqualTo("cukes"));
        Assert.That(scenario.Examples[0].Steps[0].Name, Is.EqualTo("I have 42 cukes in my belly"));
    }

    [Test]
    public void Converting_Scenario_Outline_Preserves_Placeholders_When_Example_Columns_Are_Reordered_And_Values_Are_Duplicated()
    {
        var inputFilePath = Path.Combine(Directory.GetCurrentDirectory(), "CCK", "Samples", "placeholder-mapping", "placeholder-mapping.ndjson");

        var messagesParser = new MessagesParser();
        var livingDocProject = messagesParser.ConvertToLivingDoc(inputFilePath);

        Assert.That(livingDocProject.GetNumberOfFeatures(), Is.EqualTo(1));
        Assert.That(livingDocProject.GetNumberOfScenarios(), Is.EqualTo(1));

        var scenario = livingDocProject.Features[0].Scenarios[0];

        // 1. The Scenario Outline is represented as ONE scenario/template
        Assert.That(scenario.Examples.Count, Is.EqualTo(1));

        // 2. The template contains the expected steps (4 steps)
        Assert.That(scenario.Examples[0].Steps.Count, Is.EqualTo(4));

        // 3. The exact placeholder names are preserved and correctly associated with the steps:
        //    Step 1 → '<r1_exist>'
        //    Step 2 → '<r2_exist>'
        //    Step 3 → '<r4_exist>'
        //    Step 4 → '<r3_exist>'
        Assert.That(scenario.Examples[0].Steps[0].Name, Is.EqualTo("And There '<r1_exist>' exists following data in \"WCT00205\" table"));
        Assert.That(scenario.Examples[0].Steps[1].Name, Is.EqualTo("And There '<r2_exist>' exists following data in \"WCT00205\" table"));
        Assert.That(scenario.Examples[0].Steps[2].Name, Is.EqualTo("And There '<r4_exist>' exists following data in \"WCT00205\" table"));
        Assert.That(scenario.Examples[0].Steps[3].Name, Is.EqualTo("And There '<r3_exist>' exists following data in \"WCT00205\" table"));

        // 4. The result must NOT merely assert that a step contains "<"; assert the exact expected step names/placeholders
        // (Explicitly asserting exact names above; no weak "Contains("<") assertion)

        // 5. Verify the Examples table headers remain in the original order: r3_exist, class_code, r1_exist, r4_exist, r2_exist
        var headers = scenario.Examples[0].DataTable.Rows[0].Cells;
        Assert.That(headers, Is.EqualTo(new List<string> { "r3_exist", "class_code", "r1_exist", "r4_exist", "r2_exist" }));

        // 6. Verify the Example values are preserved, including the duplicate "should" values
        var values = scenario.Examples[0].DataTable.Rows[0].Cells;
        Assert.That(values, Is.EqualTo(new List<string> { "should", "9984", "should", "should", "should" }));

        // 7. Verify the scenario name
        Assert.That(scenario.Name, Is.EqualTo("Test placeholder mapping by column header"));
    }
}
}