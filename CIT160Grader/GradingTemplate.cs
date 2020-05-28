using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CIT160Grader
{
	public class GradingTemplate
	{
		public List<TestTemplate> Tests { get; set; }
		public double PossiblePoints { get; set; } = 5;
		public double WrongNumberOfInputsPenalty { get; set; } = .5;
		public double InsufficientInputsPenalty { get; set; } = .5;
		public double NoButtonPenalty { get; set; } = .5;
		public double NoDivPenalty { get; set; } = .5;
		public double ValidationPenalty { get; set; } = .5;
		public double IncorrectResponsePenalty { get; set; } = .5;
		public double NoRunPenalty { get; set; } = 1;
		public double MinimumSubmissionScore { get; set; } = 1.5;
	}

	public class TestTemplate
	{
		public List<string> Inputs { get; set; }
		public DateTime TestDateTime { get; set; } = DateTime.MinValue;
		public string ExpectedOutput { get; set; }
		public List<string> AlternativeOutputs { get; set; }
	}
}
