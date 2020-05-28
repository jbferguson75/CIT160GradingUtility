using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.Extensions;
using OpenQA.Selenium.Support.UI;
using RestSharp;
using System;
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

			foreach (string file in Directory.GetFiles(path))
			{
				if (Path.GetExtension(file) == ".html" || Path.GetExtension(file) == ".htm")
				{
					GradeFile(file, t, reportStream);
				}
			}
		}

		public void GradeFile(string file, string template)
		{
			string json = File.ReadAllText(template);
			GradingTemplate t = JsonConvert.DeserializeObject<GradingTemplate>(json);
			double possibleScore = t.PossiblePoints;

			StreamWriter reportStream = File.CreateText(Path.Combine(Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file) + "-Report.txt"));

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
				reportStream.WriteLine("File: " + Path.GetFileName(file));
				reportStream.WriteLine();

				WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
				string url = "file://" + file.Replace("\\", "/");
				driver.Navigate().GoToUrl(url);
				reportStream.WriteLine("===========================================");
				reportStream.WriteLine(driver.PageSource);
				reportStream.WriteLine("===========================================");
				reportStream.WriteLine();

				ValidationResponse validation = ValidationCheck(driver.PageSource);
				if (validation?.Messages?.Length > 0)
				{
					score -= template.ValidationPenalty;
					feedback.Add("Validation Errors (-" + template.ValidationPenalty + "): ");
					foreach (Message msg in validation.Messages)
					{
						feedback.Add("-"+msg.MessageMessage);
					}
				}

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
							score -= template.WrongNumberOfInputsPenalty;
							if (elements.Count < test.Inputs.Count)
							{
								score -= template.InsufficientInputsPenalty;
								isBroken = true;
								break;
							}
						}

						string inputs = "";
						int idx = 0;
						foreach (string input in test.Inputs)
						{
							inputs += input + " ";
							elements[idx].SendKeys(input);
							idx++;
						}
						feedback.Add("Run with inputs: " + inputs);
						IWebElement button = driver.FindElement(By.XPath("html/body/button"));
						if (button == null)
						{
							feedback.Add("No button found. (-" + template.NoButtonPenalty + ")");
							score -= template.NoButtonPenalty;
							isBroken = true;
							break;
						}
						button.Click();
						Thread.Sleep(100);
						IWebElement firstResult = driver.FindElement(By.XPath("html/body/div"));
						if (firstResult == null)
						{
							feedback.Add("No div tag found. (-" + template.NoDivPenalty + ")");
							score -= template.NoDivPenalty;
						}

						string actual = firstResult.GetAttribute("textContent");
						if (!actual.Contains(test.ExpectedOutput) && test.AlternativeOutputs?.FindAll(o => actual.Contains(o)).FirstOrDefault() == null)
						{
							feedback.Add("Incorrect Output. (-" + template.IncorrectResponsePenalty + ")");
							score -= template.IncorrectResponsePenalty;
						}
						feedback.Add("Expected: " + test.ExpectedOutput);
						feedback.Add("Actual: " + actual);
						if (score < template.MinimumSubmissionScore)
							score = template.MinimumSubmissionScore;

						feedback.Add("Score: " + score + "/" + possibleScore);
					}
					catch
					{
						feedback.Add("Program doesn't run without errors. (-" + template.NoRunPenalty + ")");
						score -= template.NoRunPenalty;
					}
				}
				driver.Close();

				if (isBroken)
					feedback.Add("Program cannot run as written. (-" + template.NoRunPenalty + ")");

				foreach (string line in feedback)
				{
					reportStream.WriteLine(line);
				}

				reportStream.Close();
			}
		}
		private ValidationResponse ValidationCheck(string html)
		{
			string url = "https://validator.nu/";
			RestClient client = new RestClient(url);

			RestRequest request = new RestRequest("");

			request.AddHeader("Content-Type", "text/html; charset=UTF-8");
			request.AddHeader("UserAgent", "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2272.101 Safari/537.36");
			request.AddXmlBody(html);
			request.AddQueryParameter("out", "json");

			var response = client.Post(request);
			var content = response.Content;

			return JsonConvert.DeserializeObject<ValidationResponse>(content);

		}

	}
}
