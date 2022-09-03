using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Net.Sockets;

public enum NetEvent
{
    ConnectSucc = 1,
    ConnectFail = 2,
    Close = 3,
}

public static class NetManager
{
    // private const string IP = "127.0.0.1";
    // private const int Port = 8888;
    
    // 定义套接字
    private static Socket _socket;
    // 接收缓冲区
    private static ByteArray _readBuff;
    // 写入队列
    private static Queue<ByteArray> _writeQueue;
    // 事件委托列表
    public delegate void EventListener(string err);
    // 事件监听列表
    private static Dictionary<NetEvent, EventListener> _eventListeners = new Dictionary<NetEvent, EventListener>();

    // 是否正在连接
    private static bool _isConnecting = false;
    // 是否正在关闭连接
    private static bool _isClosing = false;

    // // 委托类型
    // public delegate void MsgListener(String str);
    // // 监听列表
    // private static Dictionary<string, MsgListener> _listeners = new Dictionary<string, MsgListener>();
    // // 消息列表
    // private static List<String> _msgList = new List<string>();
    //
    // // Update
    // public static void Update()
    // {
    //     if (_msgList.Count <= 0) return;
    //
    //     String msgStr = _msgList[0];
    //     _msgList.RemoveAt(0);
    //
    //     string[] split = msgStr.Split('|');
    //     string msgName = split[0];
    //     string msgArgs = split[1];
    //     
    //     // 监听回调
    //     if (_listeners.ContainsKey(msgName))
    //     {
    //         _listeners[msgName](msgArgs);
    //     }
    // }
    //
    // // 获取描述
    // public static string GetDesc()
    // {
    //     if (_socket == null) return "";
    //     if (!_socket.Connected) return "";
    //     return _socket.LocalEndPoint.ToString();
    // }

    public static void Connect(string ip, int port)
    {
        // 状态判断
        if (_socket != null && _socket.Connected)
        {
            Debug.Log("Connect fail, already connected!");
            return;
        }
        if (_isConnecting)
        {
            Debug.Log("Connect fail, isConnecting");
            return;
        }
        
        // 初始化成员
        InitState();
        // 参数设置
        _socket.NoDelay = true;
        // Connect
        _isConnecting = true;
        // Connect
        _socket.BeginConnect(ip, port, ConnectCallback, _socket);
    }

    public static void InitState()
    {
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _readBuff = new ByteArray();
        _writeQueue = new Queue<ByteArray>();
        _isConnecting = false;
        _isClosing = false;
    }

    public static void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket) ar.AsyncState;
            socket.EndConnect(ar);
            Debug.Log("Socket Connect Succ");
            FireEvent(NetEvent.ConnectSucc,"");
            _isConnecting = false;
            //socket.BeginReceive(_readBuff, 0, 1024, 0, ReceiveCallback, socket);
        }
        catch (SocketException ex)
        {
            Debug.Log("Socket Connect fail " + ex.ToString());
            FireEvent(NetEvent.ConnectFail, ex.ToString());
            _isConnecting = false;
        }
    }

    public static void Close()
    {
        // 状态判断
        if (_socket == null || !_socket.Connected) return;
        if (_isConnecting) return;
        // 还有数据在发送
        if (_writeQueue.Count > 0)
        {
            _isClosing = true;
        }
        // 没有数据在发送
        else
        {
            _socket.Close();
            FireEvent(NetEvent.Close, "");
        }
    }
    
    // public static void ReceiveCallback(IAsyncResult ar)
    // {
    //     try
    //     {
    //         Socket socket = (Socket) ar.AsyncState;
    //         int count = socket.EndReceive(ar);
    //         string recvStr = System.Text.Encoding.Default.GetString(_readBuff, 0, count);
    //         _msgList.Add(recvStr);
    //
    //         socket.BeginReceive(_readBuff, 0, 1024, 0, ReceiveCallback, socket);
    //     }
    //     catch (SocketException ex)
    //     {
    //         Debug.Log("Socket Receive fail" + ex.ToString());
    //     }
    // }
    //
    // public static void Send(string sendStr)
    // {
    //     if (_socket == null) return;
    //     if (!_socket.Connected) return;
    //     
    //     // Send
    //     byte[] bodyBytes = System.Text.Encoding.Default.GetBytes(sendStr);
    //     Int16 len = (Int16) bodyBytes.Length;
    //     byte[] lenBytes = BitConverter.GetBytes(len);
    //     byte[] sendBytes = lenBytes.Concat(bodyBytes).ToArray();
    //     _socket.BeginSend(sendBytes, 0, sendBytes.Length, 0, SendCallback, _socket);
    // }
    //
    // public static void SendCallback(IAsyncResult ar)
    // {
    //     try
    //     {
    //         Socket socket = (Socket) ar.AsyncState;
    //         int count = socket.EndSend(ar);
    //         //Debug.Log("Socket Send succ" + count);
    //     }
    //     catch (SocketException ex)
    //     {
    //         Debug.Log("Socket Send fail" + ex.ToString());
    //     }
    // }
    //
    // // 添加监听
    // public static void AddListener(string msgName, MsgListener listener)
    // {
    //     _listeners[msgName] = listener;
    // }

    //添加事件监听
    public static void AddEventListener(NetEvent netEvent, EventListener listener)
    {
        //添加事件
        if (_eventListeners.ContainsKey(netEvent))
        {
            _eventListeners[netEvent] += listener;
        }
        //新增事件
        else
        {
            _eventListeners[netEvent] = listener;
        }
    }
    //删除事件监听
    public static void RemoveEventListener(NetEvent netEvent, EventListener listener)
    {
        if (_eventListeners.ContainsKey(netEvent))
        {
            _eventListeners[netEvent] -= listener;
            //删除
            if(_eventListeners[netEvent] == null)
            {
                _eventListeners.Remove(netEvent);
            } 
        }
    } 
    //分发事件
    private static void FireEvent(NetEvent netEvent, String err){
        if(_eventListeners.ContainsKey(netEvent)){
            _eventListeners[netEvent](err);
        }
    }
}
