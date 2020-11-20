using CIT160Cheater;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.Extensions;
using OpenQA.Selenium.Support.UI;
using RestSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;

namespace CIT160Grader
{
	public class CIT160Grader
	{
		public void GradeFolder(string path, string template)
		{
			string json = File.ReadAllText(template);
			GradingTemplate t = JsonConvert.DeserializeObject<GradingTemplate>(json);
			double possibleScore = t.PossiblePoints;

			StreamWriter reportStream = File.CreateText(Path.Combine(path, "Report.txt"));
			StreamWriter suspiciousStream = File.CreateText(Path.Combine(path, "Suspicious.txt"));

			foreach (string file in Directory.GetFiles(path))
			{
				if (Path.GetExtension(file) == ".html" || Path.GetExtension(file) == ".htm")
				{
					GradeFile(file, t, reportStream);
					FindSimilarFiles(file, path, suspiciousStream);
				}
			}

			reportStream.Flush();
			reportStream.Close();

			suspiciousStream.Flush();
			suspiciousStream.Close();
		}

		public void FindSimilarFiles(string file, string path, StreamWriter suspiciousStream)
		{
			List<string> suspiciousFiles = CIT160Cheater.CIT160Cheater.GetSimilarFiles(file, path, .9);
			
			if (suspiciousFiles.Count == 0)
				return;

			suspiciousStream.WriteLine(Path.GetFileName(file));
			suspiciousStream.WriteLine();
			suspiciousStream.WriteLine(File.ReadAllText(file));
			suspiciousStream.WriteLine();
			

			foreach (string cfile in suspiciousFiles)
			{
				suspiciousStream.WriteLine(Path.GetFileName(cfile));
				suspiciousStream.WriteLine();
				suspiciousStream.WriteLine(File.ReadAllText(cfile));
				suspiciousStream.WriteLine();
			}
			suspiciousStream.WriteLine("======================================");
		}

		public void GradeFile(string file, string template)
		{
			string json = File.ReadAllText(template);
			GradingTemplate t = JsonConvert.DeserializeObject<GradingTemplate>(json);
			double possibleScore = t.PossiblePoints;

			StreamWriter reportStream = File.CreateText(Path.Combine(Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file) + "-Report.txt"));

			reportStream = File.CreateText(Path.Combine(Path.GetDirectoryName(file), "Report.txt"));
			GradeFile(file, t, reportStream);
		}

		public void GradeFile(string file, GradingTemplate template, StreamWriter reportStream)
		{
			double possibleScore = template.PossiblePoints;
			double score = possibleScore;
			List<string> feedback = new List<string>();
			bool isBroken = false;

			using (IWebDriver driver = new ChromeDriver())
			{
				string all_text = File.ReadAllText(file);

				reportStream.WriteLine("File: " + Path.GetFileName(file));
				reportStream.WriteLine();

				WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
				string url = "file://" + file.Replace("\\", "/");
				driver.Navigate().GoToUrl(url);
				reportStream.WriteLine("===========================================");
				reportStream.WriteLine(all_text);
				reportStream.WriteLine("===========================================");
				reportStream.WriteLine();

				//ValidationResponse validation = ValidationCheck(all_text);
				//if (validation?.Messages?.Length > 0)
				//{
				//	score -= template.ValidationPenalty;
				//	feedback.Add("Validation Errors (-" + template.ValidationPenalty + "): ");
				//	foreach (Message msg in validation.Messages)
				//	{
				//		feedback.Add(msg.MessageMessage);
				//	}
				//}

				Dictionary<string, double> score_penalties = new Dictionary<string, double>();

				foreach (TestTemplate test in template.Tests)
				{
					try
					{
						if (test.TestDateTime != DateTime.MinValue)
						{
							string jstring = "var d = new Date(" + test.TestDateTime.Year + ", " + (test.TestDateTime.Month - 1) + ", " + test.TestDateTime.Day + ");Date = function(){ return d};";
							driver.ExecuteJavaScript(jstring);
						}

						ReadOnlyCollection<IWebElement> elements = driver.FindElements(By.XPath("html/body/input"));

						if (elements.Count != test.Inputs.Count)
						{
							feedback.Add("Wrong number of inputs - expected: " + test.Inputs.Count + ", actual: " + elements.Count + "(-" + template.WrongNumberOfInputsPenalty + ")");
							
							if (!score_penalties.ContainsKey("WrongNumberOfInputsPenalty"))
							{
								score_penalties.Add("WrongNumberOfInputsPenalty", template.WrongNumberOfInputsPenalty);
							}

							if (elements.Count < test.Inputs.Count)
							{
								if (!score_penalties.ContainsKey("InsufficientInputsPenalty"))
								{
									score_penalties.Add("InsufficientInputsPenalty", template.InsufficientInputsPenalty);
								}

								isBroken = true;
								break;
							}
						}

						string inputs = "";
						int idx = 0;
						foreach (string input in test.Inputs)
						{
							inputs += input + " ";
							elements[idx].Clear();
							elements[idx].SendKeys(input);
							idx++;
						}
						feedback.Add("Run with inputs: " + inputs);
						IWebElement button = driver.FindElement(By.XPath("html/body/button"));
						if (button == null)
						{
							feedback.Add("No button found. (-" + template.NoButtonPenalty + ")");

							if (!score_penalties.ContainsKey("NoButtonPenalty"))
							{
								score_penalties.Add("NoButtonPenalty", template.NoButtonPenalty);
							}

							isBroken = true;
							break;
						}
						button.Click();
						Thread.Sleep(100);
						IWebElement firstResult = driver.FindElement(By.XPath("html/body/div"));
						if (firstResult == null)
						{
							feedback.Add("No div tag found. (-" + template.NoDivPenalty + ")");

							if (!score_penalties.ContainsKey("NoDivPenalty"))
							{
								score_penalties.Add("NoDivPenalty", template.NoDivPenalty);
							}
						}

						string actual = firstResult.GetAttribute("textContent");
						if (!actual.Contains(test.ExpectedOutput) && test.AlternativeOutputs?.FindAll(o => actual.Contains(o)).FirstOrDefault() == null)
						{
							feedback.Add("Incorrect Output. (-" + template.IncorrectResponsePenalty + ")");

							if (!score_penalties.ContainsKey("IncorrectResponsePenalty"))
							{
								score_penalties.Add("IncorrectResponsePenalty", template.IncorrectResponsePenalty);
							}
						}
						feedback.Add("Expected: " + test.ExpectedOutput);
						feedback.Add("Actual: " + actual);
												
					}
					catch
					{
						feedback.Add("Program doesn't run without errors. (-" + template.NoRunPenalty + ")");

						if (!score_penalties.ContainsKey("NoRunPenalty"))
						{
							score_penalties.Add("NoRunPenalty", template.NoRunPenalty);
						}
					}
				}

				foreach(string key in score_penalties.Keys)
				{
					score -= score_penalties[key];
				}

				if (score < template.MinimumSubmissionScore)
					score = template.MinimumSubmissionScore;

				feedback.Add("Score: " + score + "/" + possibleScore);

				driver.Close();

				if (isBroken)
					feedback.Add("Program cannot run as written. (-" + template.NoRunPenalty + ")");

				foreach (string line in feedback)
				{
					reportStream.WriteLine(line);
				}
				reportStream.WriteLine();

				//reportStream.Close();
			}
		}
		private ValidationResponse ValidationCheck(string html)
		{
			string url = "https://validator.nu/";
			RestClient client = new RestClient(url);

			RestRequest request = new RestRequest("");

			request.AddHeader("Content-Type", "text/html; charset=UTF-8");
			request.AddHeader("UserAgent", "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2272.101 Safari/537.36");
			request.AddBody(html);
			request.AddQueryParameter("out", "json");

			var response = client.Post(request);
			var content = response.Content;

			return JsonConvert.DeserializeObject<ValidationResponse>(content);

		}

	}
}
