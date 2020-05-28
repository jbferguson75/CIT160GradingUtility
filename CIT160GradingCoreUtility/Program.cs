using CIT160Grader;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace CIT160GradingCoreUtility
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				if (args[0].ToLower() == "createtemplate")
				{
					TestTemplate t = new TestTemplate();
					t.Inputs = new List<string>();
					t.Inputs.Add("0");
					t.Inputs.Add("0");
					t.Inputs.Add("10");
					t.Inputs.Add("10");
					t.ExpectedOutput = "4.14";
					t.AlternativeOutputs = new List<string>() { "4", "4.1" };
					t.TestDateTime = new DateTime(2000, 01, 01);
					GradingTemplate g = new GradingTemplate();
					g.Tests = new List<TestTemplate>();
					g.Tests.Add(t);
					Console.WriteLine(JsonConvert.SerializeObject(g));
					Console.ReadLine();
					return;
				}
				else if (args[0].ToLower() == "help")
				{
					OutputUsage();
					return;
				}

				CIT160Grader.CIT160Grader grader = new CIT160Grader.CIT160Grader();

				if (File.Exists(args[1]) && (Path.GetExtension(args[1]) == ".html" || Path.GetExtension(args[1]) == ".htm"))
				{
					grader.GradeFile(args[1], args[0]);
				}
				else if (Directory.Exists(args[1]))
				{
					grader.GradeFolder(args[1], args[0]);
				}
				else
				{
					Console.WriteLine(args[1] + " does not exist.");
				}
			}
			catch (Exception ex)
			{
				OutputUsage();
				Console.WriteLine(ex.Message);
				Console.WriteLine(ex.StackTrace);
			}
		}

		private static void OutputUsage()
		{
			Console.WriteLine("CIT160Grading [folder or file name] [template path]");
			Console.WriteLine("or");
			Console.WriteLine("CIT160Grading createtemplate (outputs json for a template)");
			Console.WriteLine("or");
			Console.WriteLine("CIT160Grading help (output this message)");
		}
	}
}
