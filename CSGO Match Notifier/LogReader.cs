using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CSGO_Match_Notifier
{
	class LogReader
	{
		public LogReader()
		{
			string commadOutputPath = Program.CSGOInstallationPath + "/matchOutput.log";
			int blockInputOfLines = 0;
			int blockedLines = 0;

			// Clear the output file from last time we played if possible
			try
			{
				File.WriteAllText(commadOutputPath, String.Empty);
				Console.WriteLine("Successfully overridden matchOutput.log");
			}
			catch (IOException)
			{
				using (var csv = new FileStream(commadOutputPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				using (var sr = new StreamReader(csv))
				{
					List<string> file = new List<string>();
					while (!sr.EndOfStream)
					{
						file.Add(sr.ReadLine());
					}

					blockInputOfLines = file.ToArray().Count();
				}

				Console.WriteLine("Failed to override matchOutput.log - Skipping " + blockInputOfLines + " lines");
			}

			// Continously read the file and listen to changes
			using (var fs = new FileStream(commadOutputPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			using (var reader = new StreamReader(fs, Encoding.UTF8))
			{
				while (true)
				{
					string line = reader.ReadLine();

					if (blockedLines < blockInputOfLines)
					{
						blockedLines++;
						continue;
					}

					if (String.IsNullOrWhiteSpace(line))
					{
						continue;
					}

					Program.processLine(line);
				}
			}
		}
	}
}
