﻿using System;
using Net;
using Proto;

namespace Server.logic
{
    public partial class MsgHandler
    {
        public static void MsgPing(ClientState c, MsgBase msgBase)
        {
            c.lastPingTime = NetManager.GetTimeStamp();
            MsgPong msgPong = new MsgPong();
            NetManager.Send(c, msgPong);
        }
    }
}