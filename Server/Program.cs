// See https://aka.ms/new-console-template for more information
using System;
using System.Net;
using System.Net.Sockets;

namespace EchoServer
{
    class ClientState
    {
        public Socket socket;
        public byte[] readBuff = new byte[1024];
    }
    
    class MainClass
    {
        // 监听Socket
        private static Socket _listenfd;
        // 客户端Socket及状态信息
        private static Dictionary<Socket, ClientState> _clients = new Dictionary<Socket, ClientState>();

        public static void Main(string[] args)
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
        }
    }
}


