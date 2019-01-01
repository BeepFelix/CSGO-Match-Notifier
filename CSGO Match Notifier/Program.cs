using Microsoft.Win32;
using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Media;
using System.IO;
using System.Collections.Generic;

using WebSocketSharp;
using WebSocketSharp.Server;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace CSGO_Match_Notifier
{
	class Program
	{
        // Websocket
        private static WebSocketServer wss = null;
        private static bool useWebsocket = false;

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

            // Ask for using the App
            Console.WriteLine("Start a Websocket Server to communicate with the Match Notifier App? (y/n):");

            // Check if y was pressed if yes set useWebsocket to true if not false...
            useWebsocket = Console.Read() == 121 ? true : false;

			// Start a new thread with the log reader
			Thread logReaderThread = new Thread(() =>
			{
				logReader = new LogReader();
			});
			logReaderThread.Start();

            // Only start Websocket if the user explicitly wants it...
            if (useWebsocket)
            {
                // Websocket Thread for communicating with our App
                Thread webSocketThread = new Thread(() =>
                {
                    Console.WriteLine("Starting Websocket...");
                    // This IP will be used for our websocket server to bind it.
                    string ipForBinding = "";

                    // Get Local Ipv4 addresses
                    string[] localIps = GetAllLocalIPv4(NetworkInterfaceType.Ethernet).Length != 0 ? GetAllLocalIPv4(NetworkInterfaceType.Ethernet) : GetAllLocalIPv4(NetworkInterfaceType.Wireless80211);

                    // Get First Ip out ouf our localIpArray...
                    ipForBinding = localIps.GetValue(0).ToString();

                    // Create new WebSocket Server Object
                    wss = new WebSocketServer("ws://" + ipForBinding);
                    wss.KeepClean = false;
                    //wss.Log.Level = LogLevel.Fatal;

                    // Add new Websocket Path
                    wss.AddWebSocketService<NotifyWhenMatchBehaviour>("/match");

                    // Start Websocket
                    wss.Start();

                    if (wss.IsListening)
                    {
                        Console.WriteLine("Websocket Online, Connect with: " + ipForBinding + " (You need to be on the same network)");
                    }
                });
                webSocketThread.Start();
            }
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
            if (useWebsocket) 
                wss.WebSocketServices.Broadcast("{\"match\": true}");
        }

        // https://stackoverflow.com/questions/6803073/get-local-ip-address
        public static string[] GetAllLocalIPv4(NetworkInterfaceType _type)
        {
            List<string> ipAddrList = new List<string>();
            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (item.NetworkInterfaceType == _type && item.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            ipAddrList.Add(ip.Address.ToString());
                        }
                    }
                }
            }
            return ipAddrList.ToArray();
        }
    }
}
