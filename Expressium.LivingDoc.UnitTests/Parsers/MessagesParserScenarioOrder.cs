using Expressium.LivingDoc.Parsers;
using Expressium.LivingDoc.Models;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace Expressium.LivingDoc.UnitTests.Parsers
{
    internal class MessagesParserScenarioOrder
    {
        LivingDocProject livingDocProject;
        IEnumerable<LivingDocScenario> scenarios;

        [OneTimeSetUp] 
        public void SetUp() {
            var inputFilePath = Path.Combine(Directory.GetCurrentDirectory(), "CCK", "Samples", "all-statuses", "all-statuses.ndjson");

            var messagesParser = new MessagesParser();
            livingDocProject = messagesParser.ConvertToLivingDoc(inputFilePath);
            scenarios = livingDocProject.Features.SelectMany(x => x.Scenarios);
        }

        [Test]
        public void Scenarios_HasSix()
        {
            Assert.That(scenarios.Count(), Is.EqualTo(6), "There is 6 scenarios in the ndjson file");
        }

        [Test]
        public void Scenarios_HasOrderedAssigned()
        {
            Assert.That(scenarios.All(s => s.Order > 0), Is.True, "All scenarios has an order assigned");
        }

        [Test]
        public void Scenarios_IsOrderedByTimeStamp()
        {
            var expectedIdOrder = new int[] { 3, 11, 7, 19, 15, 23 };
            var actualIdOrder = scenarios.OrderBy(x => x.Order).Select(x => int.Parse(x.Id));
            Assert.That(actualIdOrder, Is.EqualTo(expectedIdOrder), "Scenarios order as stated in all-statsus");
        }
    }
}
