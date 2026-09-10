Feature: LivingDoc report generation
	The formatter should preserve ordinary scenarios and scenario outlines.

	Scenario: Generates a report for a regular scenario
		Given a report generation sample
		When I add 2 and 3
		Then the result should be 5

	Scenario Outline: Generates a report for a scenario outline
		Given a report generation sample
		When I add <left> and <right>
		Then the result should be <sum>

		Examples:
			| left | right | sum |
			| 1    | 4     | 5   |
			| 10   | 5     | 15  |
