﻿// See https://aka.ms/new-console-template for more information
using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

namespace Server
{
    class MainClass
    {
        // 监听Socket
        private static Socket _listenfd;
        // 客户端Socket及状态信息
        public static Dictionary<Socket, ClientState> clients = new Dictionary<Socket, ClientState>();

        public static void Main(string[] args)
        {
            if (!DbManager.Connect("game", "127.0.0.1", 3306, "root", ""))
            {
                return;
            }

            NetManager.StartLoop(8888);
        }

        // 测试代码
        /*public static void Main(string[] args)
        {
            MsgMove msgMove = new MsgMove();
            msgMove.x = 100;
            msgMove.y = -20;
            byte[] bytes = MsgBase.Encode(msgMove);
            string s = System.Text.Encoding.UTF8.GetString(bytes);
            Console.WriteLine(s);

            MsgMove m = (MsgMove)MsgBase.Decode("MsgMove", bytes, 0, bytes.Length);
            Console.WriteLine(m.x);
            Console.WriteLine(m.y);
            Console.WriteLine(m.z);
        }*/

        /*public static void Main(string[] args)
        {
            // Socket
            _listenfd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            
            // Bind
            IPAddress ipAdr = IPAddress.Parse("127.0.0.1");
            IPEndPoint ipEp = new IPEndPoint(ipAdr, 8888);
            _listenfd.Bind(ipEp);
            
            // Listen
            _listenfd.Listen(0);
            Console.WriteLine("[服务器]启动成功");
            
            // checkRead
            List<Socket> checkRead = new List<Socket>();
        
            while (true)
            {
                // 填充checkRead列表
                checkRead.Clear();
                checkRead.Add(_listenfd);
                foreach (ClientState s in clients.Values)
                {
                    checkRead.Add(s.socket);
                }
                
                // Select
                Socket.Select(checkRead, null, null, 1000);
                // 检查可读对象
                foreach (Socket s in checkRead)
                {
                    if (s == _listenfd)
                    {
                        ReadListenfd(s);
                    }
                    else
                    {
                        ReadClientfd(s);
                    }
                }
            }
        }
        
        
        public static void ReadListenfd(Socket listenfd)
        {
            Console.WriteLine("Accept");
            Socket clientfd = listenfd.Accept();
            ClientState state = new ClientState();
            state.socket = clientfd;
            clients.Add(clientfd, state);
        }
        
        public static bool ReadClientfd(Socket clientfd)
        {
            ClientState state = clients[clientfd];
            //接收
            int count = 0;
            try
            {
                count = clientfd.Receive(state.readBuff);
            }
            catch (SocketException ex)
            {
                // MethodInfo mei = typeof(EventHandler).GetMethod("OnDisconnect");
                // object[] ob = {state};
                // mei.Invoke(null, ob);
        
                clientfd.Close();
                clients.Remove(clientfd);
                Console.WriteLine("Receive SocketException " + ex.ToString());
                return false;
            }
        
            //客户端关闭
            if(count == 0)
            {
                // MethodInfo mei = typeof(EventHandler).GetMethod("OnDisconnect");
                // object[] ob = {state};
                // mei.Invoke(null, ob);
                
                clientfd.Close();
                clients.Remove(clientfd);
                Console.WriteLine("Socket Close");
                return false;
            }
            
            //广播 - 大乱斗
            /*string recvStr = System.Text.Encoding.Default.GetString(state.readBuff, 0, count);
            Console.WriteLine("Receive: " + recvStr);
            
            string[] split = recvStr.Split("|");
            string msgName = split[0];
            string msgArgs = split[1];
            string funName = "Msg" + msgName;
            MethodInfo mi = typeof(MsgHandler).GetMethod(funName);
            object[] o = {state, msgArgs};
            mi.Invoke(null, o);
            
            string sendStr = recvStr;
            byte[] sendBytes = System.Text.Encoding.Default.GetBytes(sendStr);
            
            foreach (ClientState cs in clients.Values) 
            {
                cs.socket.Send(sendBytes);
            }#1#
            
            // 广播 - 聊天室
            string recvStr = System.Text.Encoding.Default.GetString(state.readBuff, 2, count - 2);
            Console.WriteLine("Receive " + recvStr);
            byte[] sendBytes = new byte[count];
            Array.Copy(state.readBuff, 0, sendBytes, 0, count);
            foreach (ClientState cs in clients.Values) 
            {
                cs.socket.Send(sendBytes);
            }
            
            return true;
        }
        
        public static void Send(ClientState cs, string sendStr)
        {
            byte[] sendBytes = System.Text.Encoding.Default.GetBytes(sendStr);
            cs.socket.Send(sendBytes);
        }*/

        // Poll状态检测
        /*public static void Main(string[] args)
        {
            // Socket
            _listenfd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            
            // Bind
            IPAddress ipAdr = IPAddress.Parse("127.0.0.1");
            IPEndPoint ipEp = new IPEndPoint(ipAdr, 8888);
            _listenfd.Bind(ipEp);
            
            // Listen
            _listenfd.Listen(0);
            Console.WriteLine("[服务器]启动成功");
            
            // 主循环
            while (true)
            {
                // 检查listenfd
                if (_listenfd.Poll(0, SelectMode.SelectRead))
                {
                    ReadListenfd(_listenfd);
                }
                
                // 检查clientfd
                foreach (ClientState s in _clients.Values)
                {
                    Socket clientfd = s.socket;
                    if (clientfd.Poll(0, SelectMode.SelectRead))
                    {
                        if (!ReadClientfd(clientfd))
                        {
                            // 此处break，是因为在ReadClientfd存在_clients.Remove(clientfd)
                            break;
                        }
                    }
                }
                
                // 防止CPU占用过高
                System.Threading.Thread.Sleep(1);
            }
        }*/

        // Socket
        /*public static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            // Socket
            _listenfd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPAddress ipAdr = IPAddress.Parse("127.0.0.1");
            IPEndPoint ipEp = new IPEndPoint(ipAdr, 8888);

            // Bind
            _listenfd.Bind(ipEp);
            
            // Listen
            _listenfd.Listen(0);
            
            Console.WriteLine("[服务器]启动");
            
            // while (true)
            // {
            //     // Accept
            //     Socket connfd = _listenfd.Accept();
            //     Console.WriteLine("[服务器]Accept");
            //     
            //     // Receive
            //     byte[] readBuff = new byte[1024];
            //     int count = connfd.Receive(readBuff);
            //     string readStr = System.Text.Encoding.Default.GetString(readBuff, 0, count);
            //     Console.WriteLine("[服务器接收]{0}", readStr);
            //     
            //     // Send
            //     byte[] sendBytes = System.Text.Encoding.Default.GetBytes(readStr);
            //     connfd.Send(sendBytes);
            // }
            
            // Accept
            _listenfd.BeginAccept(AcceptCallback, _listenfd);
            
            // 等待
            Console.ReadLine();
        }
        public static void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                Console.WriteLine("[服务器]Accept");
                Socket listenfd = (Socket) ar.AsyncState;
                Socket clientfd = listenfd.EndAccept(ar);

                // clients列表
                ClientState state = new ClientState();
                state.socket = clientfd;
                _clients.Add(clientfd, state);

                // 接收数据BeginReceive
                clientfd.BeginReceive(state.readBuff, 0, 1024, 0, ReceiveCallback, state);

                listenfd.BeginAccept(AcceptCallback, listenfd);
            }
            catch (SocketException ex)
            {
                Console.WriteLine("Socket Accept fail {0}", ex.ToString());
            }
        }
        public static void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                ClientState state = (ClientState) ar.AsyncState;
                Socket clientfd = state.socket;
                int count = clientfd.EndReceive(ar);
                
                // 客户端关闭
                if (count == 0)
                {
                    clientfd.Close();
                    _clients.Remove(clientfd);
                    Console.WriteLine("Socket Close");
                    return;
                }

                string recvStr = System.Text.Encoding.Default.GetString(state.readBuff, 0, count);
                byte[] sendBytes = System.Text.Encoding.Default.GetBytes(string.Format("echo {0}", recvStr));
                
                // clientfd.Send(sendBytes);
                foreach (ClientState s in _clients.Values)
                {
                    s.socket.Send(sendBytes);
                }
                
                clientfd.BeginReceive(state.readBuff, 0, 1024, 0, ReceiveCallback, state);
            }
            catch (SocketException ex)
            {
                Console.WriteLine("Socket Receive fail {0}", ex.ToString() );
            }
        }*/
    }
}


