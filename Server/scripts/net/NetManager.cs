using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Net
{
    public class NetManager
    {
        // 监听Socket
        public static Socket listenfd;
        // 客户端Socket及状态信息
        public static Dictionary<Socket, ClientState> clients = new Dictionary<Socket, ClientState>();
        // Select的检查列表
        private static List<Socket> checkRead = new List<Socket>();
    }
}