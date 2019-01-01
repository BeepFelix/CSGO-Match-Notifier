using Microsoft.Win32;
using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Media;
using System.IO;

namespace CSGO_Match_Notifier
{
	class Program
	{
		private static LogReader logReader = null;
		private static Regex matchAcceptRegex = new Regex(@"^Received Steam datagram ticket for server (?<serverid>\[A:\d+:\d+:\d+\]) vport \d+\. match_id=(?<matchid>\d+)$", RegexOptions.Compiled);
		public static string CSGOInstallationPath = null;

		static void Main(string[] args)
		{
			// Continue
			object r;

			// Find Steam installation path. First GetValue() is for 64-Bit, second one is for 32-Bit
			r = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Valve\Steam", "InstallPath", null);
			if (r == null)
			{
				r = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Valve\Steam", "InstallPath", null);
				if (r == null)
				{
					// Steam is not installed or could not be found
					Console.WriteLine("Could not find Steam installation path");
					Console.ReadKey();
					Environment.Exit(0);
					return;
				}
			}

			// Append CSGO and commandoutput path
			r += "/steamapps/common/Counter-Strike Global Offensive/csgo";
			CSGOInstallationPath = r.ToString();

			// Start a new thread with the log reader
			Thread logReaderThread = new Thread(() =>
			{
				logReader = new LogReader();
			});
			logReaderThread.Start();
		}

		public static void processLine(string line)
		{
			// Parse the incoming line using our regex
			Match matchAccept = matchAcceptRegex.Match(line);

			if (matchAccept.Success != true)
			{
				return;
			}

			// Log matchid and serverid
			string serverid = matchAccept.Groups[matchAcceptRegex.GroupNumberFromName("serverid")].ToString();
			string matchid = matchAccept.Groups[matchAcceptRegex.GroupNumberFromName("matchid")].ToString();

			Console.WriteLine("Received match starting info for match " + matchid + " for server " + serverid);

			// Play an audio on our Desktop
			if (File.Exists("./match.wav"))
			{
				SoundPlayer player = new SoundPlayer("./match.wav");
				player.Play();
			}

			// and if available send a notification to our phone
		}
	}
}
