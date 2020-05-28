# CIT 160 Grading Utility
A tool to assist grading programming assignments for CIT 160 at BYU-I

## Description
This utility is written in C# and the solution was made with Visual Studio 2019.  

There are 2 projects (currently) in the solution:  CIT160Grader and CIT160GradingConsole.  All of the grading logic is in the CIT160Grader project.  CIT160GradingConsole is a console application.  In the future I plan on adding a UI project that instructors or students can use.

The Grading Utility is generic.  It's made to be able to grade most all programming assignments from CIT 160 that involve full HTML and JavaScript including allowing to set the current date for the test.  Specfics about each programming assignment is kept in a json template file.  I'll go through and add a template to the templates folder for every assignment as I can throughout the 2020 Spring semester.  Hopefully by summer term we'll have all of the templates that we need.  There is an example template in the template folder called GradingTemplate.json

## Builds
There are zip files in the Builds folder with a compiled copies of the application for 4 different platforms:  win-x64, linux-x64, osx-x64, and portable.  Just unzip the correct file on your computer and run CIT160GradingCoreUtility(.exe) from a command window.  

## CIT160GradingCoreUtility Usage
Usage of the Console application is very simple:

### CIT160GradingCoreUtility createtemplate
This will output example json for a template.

### CIT160GradingCoreUtility help
Outputs how to use the console application

### CIT160GradingCoreUtility [template file and path] [folder or file path to be graded]
The template path is the file that the program will use to grade with.  The other argument can take either a single file or a folder.  It will grade all .htm or .html files in the folder.  

### Report.txt
After the application is run, it creates a text file in the folder that was graded or the same folder where the file specified to grade is found.  The name is either Report.txt or [Filename]-Report.txt.  The report contains all grading information for each file graded including the file name, the actual submitted program, and some basic feedback and a score to use as a jumping off point for the instructor's grading.

## Templates
The templates are in json format and there is a different one for every graded program.  This is the format:

{
  "Tests": [
    {
      "Inputs": [ "0", "0", "10", "10" ],
      "TestDateTime": "2000-01-01T00:00:00",
      "ExpectedOutput": "4.14",
      "AlternativeOutputs": [ "4", "4.1" ]
    }
  ],
  "PossiblePoints": 5.0,
  "WrongNumberOfInputsPenalty": 0.5,
  "InsufficientInputsPenalty": 0.5,
  "NoButtonPenalty": 0.5,
  "NoDivPenalty": 0.5,
  "ValidationPenalty": 0.5,
  "IncorrectResponsePenalty": 0.5,
  "NoRunPenalty": 1.0,
  "MinimumSubmissionScore": 1.5
}

### Tests (Required)
This defines the inputs and expected outputs of each test that's performed.
#### Inputs (Required)
An array of values to put into the program's <input> fields.  They are in the same order as they are expected to be in in the <body> of the HTML page.
  
#### TestDateTime (Optional)
The date to run the test with.  This only applies to a couple of the assignments that are date based.
  
#### ExpectedOutput (Required)
A string that is expected to appear somewhere in the first <div> tag.  It just searches for it.  This is the string reported back as the "Expected" compared to the "Actual" in the report.
  
#### AlternativeOutputs (Optional)
An array of alternative outputs that are also correct.  This can be used to handle multiple levels of rounding that are correct.

### PossiblePoints (Required)
The total number of points possible in for this problem.  This is used for estimating partial credit.

### WrongNumberOfInputsPenalty (Required)
The points deducted if the number of <input> tags in the web page doesn't equal the number of inputs in the "Inputs" array in the template

### InsufficientInputsPenalty (Required)
The points deducted if the number of <input> tags in the web page is less than the number of inputs in the "Inputs" array in the template.  This will cause the grader to not attempt to run the script.  (This is compounded with the WrongNumberOfInputsPenalty - both will be deducted.)

### NoButtonPenalty (Required)
The points deducted if there is no <button> found in the web page.  This will cause the grader to not attempt to run the script since there is no button to press.
  
### NoDivPenalty (Required)
The points deducted if there is no <div> found in the web page.  This will cause the grader to not attempt to run the script since there is no <div> to see the response from.
  
### ValidationPenalty (Required)
The points deducted if the web page does not validate using the w3schools validation api.

### IncorrectResponsePenalty (Required)
The points deducted if the text in the <div> tag doesn't contain the expected nor the alternative outputs.  (It only checks the first (and hopefully the only) <div> tag found.
  
### NoRunPenalty (Required)
The points deducted if the program doesn't run or causes an exception in the grading program.

### MinimumSubmissionScore (Required)
This is the minimum score given just for submitting.  If their actual score is less than this minimum score, it'll give the minimum score.
