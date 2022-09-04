using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Net.Sockets;
using Net.Proto;

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
    // 每一次Update处理的消息量
    private readonly static int MAX_MESSAGE_FIRE = 10;
    
    // 消息列表
    private static List<MsgBase> _msgList = new List<MsgBase>();
    private static int _msgCount = 0;
    
    // 是否启用心跳
    public static bool isUsingPing = true;
    // 心跳间隔事件
    public static int pingInterval = 30;
    // 上一次发送PING的时间
    private static float _lastPingTime = 0;
    // 上一次收到PONG的时间
    private static float _lastPongTime = 0;
    
    
    // Update
    public static void Update()
    {
        MsgUpdate();
        PingUpdate();
    }

    private static void MsgUpdate()
    {
        // 初步判断，提升效率
        if (_msgCount == 0) return;
        
        // 处理消息
        for (int i = 0; i < MAX_MESSAGE_FIRE; i++)
        {
            MsgBase msgBase = null;
            lock (_msgList)
            {
                if (_msgList.Count > 0)
                {
                    msgBase = _msgList[0];
                    _msgList.RemoveAt(0);
                    _msgCount--;
                }
            }

            if (msgBase != null)
            {
                FireMsg(msgBase.protoName, msgBase);
            }
            else
            {
                break;
            }
        }
    }
    
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
        
        // 消息列表
        _msgList = new List<MsgBase>();
        _msgCount = 0;
        
        // PING & PONG
        _lastPingTime = Time.time;
        _lastPongTime = Time.time;
        if (!_msgListeners.ContainsKey("MsgPong"))
        {
            AddMsgListener("MsgPong", OnMsgPong);
        }
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
            socket.BeginReceive(_readBuff.bytes, _readBuff.writeIdx, _readBuff.remain, 0, ReceiveCallback, socket);
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
    
    public static void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket) ar.AsyncState;
            int count = socket.EndReceive(ar);
            // 接收到Fin信号，count == 0
            if (count == 0)
            {
                Close();
                return;
            }

            _readBuff.writeIdx += count;
            OnReceiveData();

            if (_readBuff.remain < 8)
            {
                _readBuff.MoveBytes();
                _readBuff.ReSize(_readBuff.length * 2);
            }
            socket.BeginReceive(_readBuff.bytes, _readBuff.writeIdx, _readBuff.remain, 0, ReceiveCallback, socket);
        }
        catch (SocketException ex)
        {
            Debug.Log("Socket Receive fail" + ex.ToString());
        }
    }
    
    public static void OnReceiveData()
    {
        // 消息长度
        if (_readBuff.length <= 2) return;
        // 获取消息体长度
        int readIdx = _readBuff.readIdx;
        byte[] bytes = _readBuff.bytes;
        Int16 bodyLength = (Int16) ((bytes[readIdx + 1] << 8) | bytes[readIdx]);
        if (_readBuff.length < bodyLength + 2)
        {
            return;
        }
        _readBuff.readIdx += 2;             // 读完消息体长度

        // 解析协议名
        int nameCount = 0;
        string protoName = MsgBase.DecodeName(_readBuff.bytes, _readBuff.readIdx, out nameCount);
        if (protoName == "")
        {
            Debug.Log("OnReceiveData MsgBase.DecodeName fail");
            return;
        }
        _readBuff.readIdx += nameCount;     // 读完协议名
        
        // 解析协议体
        int bodyCount = bodyLength - nameCount;
        MsgBase msgBase = MsgBase.Decode(protoName, _readBuff.bytes, _readBuff.readIdx, bodyCount);
        _readBuff.readIdx += bodyCount;     // 读完协议体
        _readBuff.CheckAndMoveBytes();
        
        // 添加到消息队列
        lock (_msgList)
        {
            _msgList.Add(msgBase);
        }
        _msgCount++;
        
        // 继续读取消息
        if (_readBuff.length > 2)
        {
            OnReceiveData();
        }
    }
    
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
    
    // 心跳，发送Ping协议
    private static void PingUpdate()
    {
        // 是否启用
        if (!isUsingPing)
        {
            return;
        }
        // 发送PING
        if (Time.time - _lastPingTime > pingInterval)
        {
            MsgPing msgPing = new MsgPing();
            Send(msgPing);
            _lastPingTime = Time.time;
        }
        // 检查Pong时间
        if (Time.time - _lastPongTime > pingInterval * 4)
        {
            Close();
        }
    }
    
    // 监听Pong协议
    private static void OnMsgPong(MsgBase msgBase)
    {
        _lastPingTime = Time.time;
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
