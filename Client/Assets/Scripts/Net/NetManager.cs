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

    // 消息委托类型
    public delegate void MsgListener(MsgBase str);
    // 消息监听列表
    private static Dictionary<string, MsgListener> _msgListeners = new Dictionary<string, MsgListener>();
    
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
    public static void Send(MsgBase msg)
    {
        // 状态判断
        if (_socket == null || !_socket.Connected) return;
        if ( _isConnecting) return;
        if (_isClosing) return;

        // 数据编码
        byte[] nameBytes = MsgBase.EncodeName(msg);
        byte[] bodyBytes = MsgBase.Encode(msg);
        int len = nameBytes.Length + bodyBytes.Length;
        byte[] sendBytes = new byte[2 + len];
        
        // 组装长度
        sendBytes[0] = (byte) (len % 256);
        sendBytes[1] = (byte) (len / 256);
        // 组装名字
        Array.Copy(nameBytes, 0, sendBytes, 2, nameBytes.Length);
        // 组装消息体
        Array.Copy(bodyBytes,0,sendBytes,2+nameBytes.Length,bodyBytes.Length);
        // 写入队列
        ByteArray ba = new ByteArray(sendBytes);
        int count = 0;
        lock (_writeQueue)
        {
            _writeQueue.Enqueue(ba);
            count = _writeQueue.Count;
        }

        if (count == 1)
        {
            _socket.BeginSend(sendBytes, 0, sendBytes.Length, 0, SendCallback, _socket);
        }
    }
    
    public static void SendCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket) ar.AsyncState;
            if (socket == null || !socket.Connected)
            {
                return;
            }
            int count = socket.EndSend(ar);

            ByteArray ba;
            lock (_writeQueue)
            {
                ba = _writeQueue.First();
            }
            // 完整发送
            ba.readIdx += count;
            if (ba.length == 0)
            {
                lock (_writeQueue)
                {
                    _writeQueue.Dequeue();
                    ba = _writeQueue.First();
                }
            }

            if (ba != null)
            {
                socket.BeginSend(ba.bytes, ba.readIdx, ba.length, 0, SendCallback, socket);
            }
            else if (_isClosing)
            {
                socket.Close();
            }
            //Debug.Log("Socket Send succ" + count);
        }
        catch (SocketException ex)
        {
            Debug.Log("Socket Send fail" + ex.ToString());
        }
    }
    
    // 添加消息监听
    public static void AddMsgListener(string msgName, MsgListener listener)
    {
        //添加
        if (_msgListeners.ContainsKey(msgName)){
            _msgListeners[msgName] += listener;
        }
        //新增
        else{
            _msgListeners[msgName] = listener;
        }
    }
    // 删除消息监听
    public static void RemoveMsgListener(string msgName, MsgListener listener)
    {
        if (_msgListeners.ContainsKey(msgName)){
            _msgListeners[msgName] -= listener;
            //删除
            if(_msgListeners[msgName] == null){
                _msgListeners.Remove(msgName);
            } 
        }
    }
    // 分发消息
    private static void FireMsg(string msgName, MsgBase msgBase)
    {
        if(_msgListeners.ContainsKey(msgName)){
            _msgListeners[msgName](msgBase);
        }
    } 

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
