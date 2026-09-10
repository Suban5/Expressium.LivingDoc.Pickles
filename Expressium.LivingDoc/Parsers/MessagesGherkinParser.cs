using Expressium.LivingDoc.Models;
using Io.Cucumber.Messages.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Expressium.LivingDoc.Parsers
{
    internal class MessagesGherkinParser
    {
        internal void ParseGherkinDocuments(CucumberMessages messages, LivingDocProject livingDocProject)
        {
            foreach (var gherkinDocument in messages.GherkinDocuments)
            {
                var uri = gherkinDocument.Uri;
                var feature = gherkinDocument.Feature;

                var livingDocFeature = new LivingDocFeature();

                ParseFeature(livingDocFeature, feature, uri);

                foreach (var child in feature.Children)
                {
                    if (child.Background != null)
                        ParseBackground(livingDocFeature, child.Background);

                    if (child.Rule != null)
                        ParseRule(livingDocFeature, child.Rule);

                    if (child.Scenario != null)
                        ParseScenario(livingDocFeature, child.Scenario);
                }

                ParseComments(livingDocFeature, feature, gherkinDocument.Comments);
                livingDocProject.Features.Add(livingDocFeature);
            }
        }

        internal static void ParseFeature(LivingDocFeature livingDocFeature, Feature feature, string uri)
        {
            if (feature.Tags != null)
            {
                foreach (var tag in feature.Tags)
                    livingDocFeature.Tags.Add(tag.Name);
            }

            livingDocFeature.Description = feature.Description;
            livingDocFeature.Uri = uri;
            livingDocFeature.Name = feature.Name;
            livingDocFeature.Keyword = feature.Keyword;
        }

        internal static void ParseBackground(LivingDocFeature livingDocFeature, Background background)
        {
            var livingDocBackground = new LivingDocBackground
            {
                Id = background.Id,
                Description = background.Description,
                Name = background.Name,
                Keyword = background.Keyword
            };

            foreach (var step in background.Steps)
            {
                var livingDocStep = new LivingDocStep
                {
                    Name = WebUtility.HtmlEncode(step.Text),
                    Keyword = step.Keyword.Trim(),
                    Id = step.Id,
                    Type = LivingDocStepTypes.Background.ToString()
                };
                livingDocBackground.Steps.Add(livingDocStep);
            }

            livingDocFeature.Background = livingDocBackground;
        }

        internal static void ParseRule(LivingDocFeature livingDocFeature, Rule rule)
        {
            var livingDocRule = new LivingDocRule
            {
                Id = rule.Id,
                Description = rule.Description,
                Name = rule.Name,
                Keyword = rule.Keyword
            };

            if (rule.Tags != null)
            {
                foreach (var tag in rule.Tags)
                    livingDocRule.Tags.Add(tag.Name);
            }

            livingDocFeature.Rules.Add(livingDocRule);

            foreach (var ruleChild in rule.Children)
            {
                if (ruleChild.Scenario == null)
                    continue;

                ParseScenario(livingDocFeature, ruleChild.Scenario, rule.Id);
            }
        }

        internal static void ParseScenario(LivingDocFeature livingDocFeature, Scenario scenario, string ruleId = null)
        {
            var livingDocScenario = new LivingDocScenario
            {
                RuleId = ruleId,
                Id = scenario.Id,
                Description = scenario.Description,
                Name = scenario.Name,
                Keyword = scenario.Keyword
            };

            if (scenario.Tags != null)
            {
                foreach (var tag in scenario.Tags)
                    livingDocScenario.Tags.Add(tag.Name);
            }

            livingDocFeature.Scenarios.Add(livingDocScenario);

            if (scenario.Examples.Count > 0)
            {
                // Keep source Examples sections separate while retaining the first
                // section in Examples for existing execution and history consumers.
                foreach (var examples in scenario.Examples)
                {
                    var livingDocExample = ParseDocumentationExample(scenario, examples);
                    livingDocScenario.DocumentationExamples.Add(livingDocExample);

                    if (livingDocScenario.Examples.Count == 0)
                        livingDocScenario.Examples.Add(livingDocExample);
                }
            }
            else
            {
                var livingDocExample = new LivingDocExample();
                livingDocScenario.Examples.Add(livingDocExample);
                livingDocScenario.DocumentationExamples.Add(livingDocExample);

                ParseScenarioBackgroundSteps(livingDocExample, livingDocFeature);
                ParseScenarioExampleSteps(livingDocExample, scenario);
            }
        }

        private static LivingDocExample ParseDocumentationExample(Scenario scenario, Examples examples)
        {
            var livingDocExample = new LivingDocExample
            {
                Name = examples.Name,
                Description = examples.Description
            };

            ParseScenarioExampleSteps(livingDocExample, scenario);
            ParseScenarioExampleTableHeaders(livingDocExample, examples);

            foreach (var tableBodyRow in examples.TableBody)
                ParseScenarioExampleTableData(livingDocExample, tableBodyRow);

            return livingDocExample;
        }

        private static void ParseComments(LivingDocFeature livingDocFeature, Feature feature, IList<Comment> comments)
        {
            if (comments == null || comments.Count == 0)
                return;

            var targets = new List<CommentTarget>
            {
                new CommentTarget(feature.Location.Line, 0, livingDocFeature.Comments.Add)
            };

            foreach (var child in feature.Children)
            {
                if (child.Background != null && livingDocFeature.Background != null)
                    AddBackgroundCommentTarget(targets, child.Background, livingDocFeature.Background);

                if (child.Rule != null)
                    AddRuleCommentTargets(targets, child.Rule, livingDocFeature);

                if (child.Scenario != null)
                    AddScenarioCommentTargets(targets, child.Scenario, livingDocFeature);
            }

            foreach (var comment in comments.OrderBy(comment => comment.Location.Line))
            {
                var line = comment.Location.Line;
                var nextTarget = targets
                    .Where(target => target.Line > line)
                    .OrderBy(target => target.Line)
                    .FirstOrDefault();

                var selectedTarget = nextTarget != null && nextTarget.Line - line <= 1
                    ? nextTarget
                    : targets
                        .Where(candidate => candidate.Line <= line)
                        .OrderByDescending(candidate => candidate.Line)
                        .ThenByDescending(candidate => candidate.Depth)
                        .FirstOrDefault();

                (selectedTarget ?? targets[0]).Add(comment.Text);
            }
        }

        private static void AddBackgroundCommentTarget(List<CommentTarget> targets, Background background, LivingDocBackground livingDocBackground)
        {
            targets.Add(new CommentTarget(background.Location.Line, 1, livingDocBackground.Comments.Add));
        }

        private static void AddRuleCommentTargets(List<CommentTarget> targets, Rule rule, LivingDocFeature livingDocFeature)
        {
            var livingDocRule = livingDocFeature.Rules.FirstOrDefault(candidate => candidate.Id == rule.Id);
            if (livingDocRule == null)
                return;

            targets.Add(new CommentTarget(rule.Location.Line, 1, livingDocRule.Comments.Add));

            foreach (var child in rule.Children)
            {
                if (child.Scenario != null)
                    AddScenarioCommentTargets(targets, child.Scenario, livingDocFeature);
            }
        }

        private static void AddScenarioCommentTargets(List<CommentTarget> targets, Scenario scenario, LivingDocFeature livingDocFeature)
        {
            var livingDocScenario = livingDocFeature.Scenarios.FirstOrDefault(candidate => candidate.Id == scenario.Id);
            if (livingDocScenario == null)
                return;

            targets.Add(new CommentTarget(scenario.Location.Line, 2, livingDocScenario.Comments.Add));

            for (var index = 0; index < scenario.Examples.Count && index < livingDocScenario.DocumentationExamples.Count; index++)
            {
                var examples = scenario.Examples[index];
                var livingDocExample = livingDocScenario.DocumentationExamples[index];
                targets.Add(new CommentTarget(examples.Location.Line, 3, livingDocExample.Comments.Add));
            }
        }

        private sealed class CommentTarget
        {
            internal long Line { get; }
            internal int Depth { get; }
            internal Action<string> Add { get; }

            internal CommentTarget(long line, int depth, Action<string> add)
            {
                Line = line;
                Depth = depth;
                Add = add;
            }
        }

        internal static void ParseScenarioBackgroundSteps(LivingDocExample livingDocExample, LivingDocFeature livingDocFeature, int tableIndexId = -1)
        {
            if (livingDocFeature.Background != null)
            {
                foreach (var backgroundStep in livingDocFeature.Background.Steps)
                {
                    var copy = backgroundStep.Copy(backgroundStep);
                    copy.TableIndexId = tableIndexId;
                    livingDocExample.Steps.Add(copy);
                }
            }
        }

        internal static void ParseScenarioExampleSteps(LivingDocExample livingDocExample, Scenario scenario)
        {
            foreach (var step in scenario.Steps)
            {
                var livingDocStep = new LivingDocStep
                {
                    Id = step.Id,
                    Name = WebUtility.HtmlEncode(step.Text),
                    Keyword = step.Keyword.Trim()
                };

                ParseStepDataTable(livingDocStep, step);

                livingDocExample.Steps.Add(livingDocStep);
            }
        }

        internal static void ParseScenarioExampleTableSteps(LivingDocExample livingDocExample, Scenario scenario, string tableBodyRowId)
        {
            foreach (var step in scenario.Steps)
            {
                var livingDocStep = new LivingDocStep
                {
                    Id = step.Id,
                    TableBodyId = tableBodyRowId,
                    Name = WebUtility.HtmlEncode(step.Text),
                    Type = LivingDocStepTypes.Scenario.ToString(),
                    Keyword = step.Keyword.Trim()
                };

                ParseStepDataTable(livingDocStep, step);

                livingDocExample.Steps.Add(livingDocStep);
            }
        }

        private static void ParseStepDataTable(LivingDocStep livingDocStep, Step step)
        {
            if (step.DataTable == null)
                return;

            foreach (var row in step.DataTable.Rows)
            {
                var dataTableRow = new LivingDocDataTableRow();
                foreach (var cell in row.Cells)
                    dataTableRow.Cells.Add(cell.Value);
                livingDocStep.DataTable.Rows.Add(dataTableRow);
            }
        }

        internal static void ParseScenarioExampleTableHeaders(LivingDocExample livingDocExample, Examples examples)
        {
            var dataTableRow = new LivingDocDataTableRow();
            foreach (var tableHeaderRowCell in examples.TableHeader.Cells)
                dataTableRow.Cells.Add(tableHeaderRowCell.Value);
            livingDocExample.DataTable.Rows.Add(dataTableRow);
        }

        internal static void ParseScenarioExampleTableData(LivingDocExample livingDocExample, TableRow tableBodyRow)
        {
            var dataTableRow = new LivingDocDataTableRow();
            foreach (var tableBodyRowCell in tableBodyRow.Cells)
                dataTableRow.Cells.Add(tableBodyRowCell.Value);
            livingDocExample.DataTable.Rows.Add(dataTableRow);
        }
    }
}
