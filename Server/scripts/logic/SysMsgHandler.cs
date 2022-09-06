using System;
using Net;
using Proto;

namespace Server.logic
{
    public partial class MsgHandler
    {
        public static void MsgPing(ClientState c, MsgBase msgBase)
        {
            Console.WriteLine("MsgPing");
        }
    }
}