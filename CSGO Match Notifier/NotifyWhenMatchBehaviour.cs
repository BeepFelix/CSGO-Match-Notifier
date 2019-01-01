using System;
using System.IO;

using WebSocketSharp;
using WebSocketSharp.Server;

namespace CSGO_Match_Notifier
{
	class NotifyWhenMatchBehaviour : WebSocketBehavior
    {
        protected override void OnOpen()
        {
            Console.WriteLine("A new Connection was Opened for /match");
        }
    }
}
