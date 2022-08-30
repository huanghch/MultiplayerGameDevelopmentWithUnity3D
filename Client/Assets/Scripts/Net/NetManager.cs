using System;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;

public static class NetManager
{
    // private const string IP = "127.0.0.1";
    // private const int Port = 8888;
    
    // 定义套接字
    private static Socket _socket;
    // 接收缓冲区
    private static byte[] _readBuff = new byte[1024];
    // 委托类型
    public delegate void MsgListener(String str);
    // 监听列表
    private static Dictionary<string, MsgListener> _listeners = new Dictionary<string, MsgListener>();
    // 消息列表
    private static List<String> _msgList = new List<string>();
    
    // Update
    public static void Update()
    {
        if (_msgList.Count <= 0) return;

        String msgStr = _msgList[0];
        _msgList.RemoveAt(0);

        string[] split = msgStr.Split('|');
        string msgName = split[0];
        string msgArgs = split[1];
        
        // 监听回调
        if (_listeners.ContainsKey(msgName))
        {
            _listeners[msgName](msgArgs);
        }
    }
    
    // 添加监听
    public static void AddListener(string msgName, MsgListener listener)
    {
        _listeners[msgName] = listener;
    }

    // 获取描述
    public static string GetDesc()
    {
        if (_socket == null) return "";
        if (!_socket.Connected) return "";
        return _socket.LocalEndPoint.ToString();
    }

    public static void Connect(string ip, int port)
    {
        // Socket
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        
        // Connect
        _socket.BeginConnect(ip, port, ConnectCallback, _socket);
    }

    public static void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket) ar.AsyncState;
            socket.EndConnect(ar);
            //Debug.Log("Socket Connect Succ");
            socket.BeginReceive(_readBuff, 0, 1024, 0, ReceiveCallback, socket);
        }
        catch (SocketException ex)
        {
            Debug.Log("Socket Connect fail " + ex.ToString());
        }
    }
    
    public static void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket) ar.AsyncState;
            int count = socket.EndReceive(ar);
            string recvStr = System.Text.Encoding.Default.GetString(_readBuff, 0, count);
            _msgList.Add(recvStr);

            socket.BeginReceive(_readBuff, 0, 1024, 0, ReceiveCallback, socket);
        }
        catch (SocketException ex)
        {
            Debug.Log("Socket Receive fail" + ex.ToString());
        }
    }

    public static void Send(string sendStr)
    {
        if (_socket == null) return;
        if (!_socket.Connected) return;
        
        // Send
        byte[] sendBytes = System.Text.Encoding.Default.GetBytes(sendStr);
        _socket.BeginSend(sendBytes, 0, sendBytes.Length, 0, SendCallback, _socket);
    }

    public static void SendCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket) ar.AsyncState;
            int count = socket.EndSend(ar);
            //Debug.Log("Socket Send succ" + count);
        }
        catch (SocketException ex)
        {
            Debug.Log("Socket Send fail" + ex.ToString());
        }
    }


}
