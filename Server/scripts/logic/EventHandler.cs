using System;
using Net;
using Proto;

namespace Server.logic
{
    public class EventHandler
    {
        public static void OnDisconnect(ClientState c)
        {
            Console.WriteLine("OnDisConnect");
        }

        public static void OnTimer()
        {
            
        }
    }
}