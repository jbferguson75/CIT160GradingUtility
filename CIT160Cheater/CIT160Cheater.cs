using F23.StringSimilarity;
using System;
using System.Collections.Generic;
using System.IO;

namespace CIT160Cheater
{
	public static class CIT160Cheater
	{
		public static List<string> GetSimilarFiles(string file_name, string folder, double threshold)
		{
			List<string> similar_files = new List<string>();
			string file_string;

			if (File.Exists(file_name))
				file_string = File.ReadAllText(file_name);
			else
				return similar_files;

			if (Directory.Exists(folder))
			{
				var l = new NormalizedLevenshtein();

				foreach (string file in Directory.GetFiles(folder))
				{
					if (file != file_name && (Path.GetExtension(file) == ".html" || Path.GetExtension(file) == ".htm"))
					{
						string check_file = File.ReadAllText(file);
						double similarity = l.Similarity(file_string, check_file);

						if (similarity > threshold)
							similar_files.Add(file);
					}
				}
			}

			return similar_files;
		}
	}
}
