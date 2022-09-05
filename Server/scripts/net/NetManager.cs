﻿using System;
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
        
        public static void StartLoop(int listenPort)
        {
            //Socket
            listenfd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //Bind
            IPAddress ipAdr = IPAddress.Parse("0.0.0.0");
            IPEndPoint ipEp = new IPEndPoint(ipAdr, listenPort);
            listenfd.Bind(ipEp);
            //Listen
            listenfd.Listen(0);
            Console.WriteLine("[服务器]启动成功");
            //循环
            while(true){
                ResetCheckRead();  //重置checkRead 
                Socket.Select(checkRead, null, null, 1000);
                //检查可读对象
                for(int i = checkRead.Count-1; i>=0; i--){
                    Socket s = checkRead[i];
                    if(s == listenfd){
                        ReadListenfd(s);
                    }
                    else{
                        ReadClientfd(s);
                    }
                }
                //检查超时
                Timer();
            }
        }

        public static void ResetCheckRead()
        {
            checkRead.Clear();
            checkRead.Add(listenfd);
            foreach (ClientState s in clients.Values)
            {
                checkRead.Add(s.socket);
            }
        }
        
        public static void ReadListenfd(Socket listenfd)
        {
            try
            {
                Socket clientfd = listenfd.Accept();
                Console.WriteLine("Accept: " + clientfd.RemoteEndPoint.ToString());
                ClientState state = new ClientState();
                state.socket = clientfd;
                clients.Add(clientfd, state);
            }
            catch (SocketException ex)
            {
                Console.WriteLine("Accept fail: " + ex.ToString());
            }
        }

        public static void ReadClientfd(Socket clientfd)
        {
            ClientState state = clients[clientfd];
            ByteArray readBuff = state.readBuff;
            //接收
            int count = 0;
            //缓冲区不够，清除，若依旧不够，只能返回
            //缓冲区长度只有1024，单条协议超过缓冲区长度时会发生错误，根据需要调整长度
            if (readBuff.remain <= 0)
            {
                OnReceiveData(state);
                readBuff.MoveBytes();
            }

            if (readBuff.remain <= 0)
            {
                Console.WriteLine("Receive fail , maybe msg length > buff capacity");
                Close(state);
                return;
            }

            try
            {
                count = clientfd.Receive(readBuff.bytes, readBuff.writeIdx, readBuff.remain, 0);
            }
            catch (SocketException ex)
            {
                Console.WriteLine("Receive SocketException " + ex.ToString());
                Close(state);
                return;
            }

            // 接受到Fin，客户端关闭
            if (count <= 0)
            {
                Console.WriteLine("Socket Close " + clientfd.RemoteEndPoint.ToString());
                Close(state);
                return;
            }

            //消息处理
            readBuff.writeIdx += count;
            //处理二进制消息
            OnReceiveData(state);
            //移动缓冲区
            readBuff.CheckAndMoveBytes();

        }

        public static void OnReceiveData(ClientState state)
        {
            //数据处理
            ByteArray readBuff = state.readBuff;
            //消息长度
            if (readBuff.length <= 2)
            {
                return;
            }

            Int16 bodyLength = readBuff.ReadInt16();
            //消息体
            if (readBuff.length < bodyLength)
            {
                return;
            }

            //解析协议名
            int nameCount = 0;
            string protoName = MsgBase.DecodeName(readBuff.bytes,
                readBuff.readIdx, out nameCount);
            if (protoName == "")
            {
                Console.WriteLine("OnReceiveData MsgBase.DecodeName fail");
                Close(state);
            }

            readBuff.readIdx += nameCount;
            //解析协议体
            int bodyCount = bodyLength - nameCount;
            MsgBase msgBase = MsgBase.Decode(protoName, readBuff.bytes, readBuff.readIdx, bodyCount);
            readBuff.readIdx += bodyCount;
            readBuff.CheckAndMoveBytes();
            //分发消息
            MethodInfo mi = typeof(MsgHandler).GetMethod(protoName);
            object[] o = {state, msgBase};
            Console.WriteLine("Receive " + protoName);
            if (mi != null)
            {
                mi.Invoke(null, o);
            }
            else
            {
                Console.WriteLine("OnReceiveData Invoke fail " + protoName);
            }

            //继续读取消息
            if (readBuff.length > 2)
            {
                OnReceiveData(state);
            }
        }

        public static void Close(ClientState state)
        {
            //事件分发
            MethodInfo mei = typeof(EventHandler).GetMethod("OnDisconnect");
            object[] ob = {state};
            mei.Invoke(null, ob);
            //关闭
            state.socket.Close();
            clients.Remove(state.socket);
        }

        public static void Timer()
        {
            // 消息分发
            MethodInfo mei = typeof(EventHandler).GetMethod("OnTimer");
            object[] ob = { };
            mei.Invoke(null, ob);
        }
        
        //发送
        public static void Send(ClientState cs, MsgBase msg){
            //状态判断
            if(cs == null){
                return;
            }
            if(!cs.socket.Connected){
                return;
            }
            //数据编码
            byte[] nameBytes = MsgBase.EncodeName(msg);
            byte[] bodyBytes = MsgBase.Encode(msg);
            int len = nameBytes.Length + bodyBytes.Length;
            byte[] sendBytes = new byte[2+len];
            //组装长度
            sendBytes[0] = (byte)(len%256);
            sendBytes[1] = (byte)(len/256);
            //组装名字
            Array.Copy(nameBytes, 0, sendBytes, 2, nameBytes.Length);
            //组装消息体
            Array.Copy(bodyBytes, 0, sendBytes, 2+nameBytes.Length, bodyBytes.Length);
            //为简化代码，不设置回调
            try
            {
                cs.socket.BeginSend(sendBytes,0, sendBytes.Length, 0, null, null);
            }catch(SocketException ex)
            {
                Console.WriteLine("Socket Close on BeginSend" + ex.ToString()); 
            } 
        } 

    }
}