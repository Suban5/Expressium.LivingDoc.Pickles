using System.Collections.Generic;

namespace Expressium.LivingDoc.Models
{
    public class LivingDocBackground
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public string Keyword { get; set; }
        public List<string> Comments { get; set; }

        public List<LivingDocStep> Steps { get; set; }

        public LivingDocBackground()
        {
            Comments = new List<string>();
            Steps = new List<LivingDocStep>();
        }
    }
}
