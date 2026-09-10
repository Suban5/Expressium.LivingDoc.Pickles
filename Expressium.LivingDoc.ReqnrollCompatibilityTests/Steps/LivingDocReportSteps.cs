using NUnit.Framework;
using Reqnroll;

namespace Expressium.LivingDoc.ReqnrollCompatibilityTests.Steps
{
    [Binding]
    public class LivingDocReportSteps
    {
        private int result;

        [Given("a report generation sample")]
        public void GivenAReportGenerationSample()
        {
            result = 0;
        }

        [When("I add {int} and {int}")]
        public void WhenIAdd(int left, int right)
        {
            result = left + right;
        }

        [Then("the result should be {int}")]
        public void ThenTheResultShouldBe(int expected)
        {
            Assert.That(result, Is.EqualTo(expected));
        }
    }
}
