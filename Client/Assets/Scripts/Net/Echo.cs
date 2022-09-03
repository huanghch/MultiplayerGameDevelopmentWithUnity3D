using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Net.Sockets;
using UnityEngine.UI;

public class Echo : MonoBehaviour
{
    private const string IP = "127.0.0.1";
    private const int Port = 8888;
    
    private Socket _socket;
    // 接收缓冲区
    private byte[] _readBuff = new byte[1024];
    // 接收缓冲区的数据长度
    private int _buffCount = 0;
    private string _recvStr = "";
    
    // 定义发送缓冲区
    // private byte[] _sendBytes = new byte[1024];
    // 缓冲区偏移值
    // private int _readIndex = 0;
    // 缓冲区剩余长度
    // private int _sendLength = 0;
    // 发送队列
    Queue<ByteArray> writeQueue = new Queue<ByteArray>();
    
    public Button btnConnect;
    public Button btnSend;
    public InputField inputField;
    public Text text;

    private void Start()
    {
        btnConnect.onClick.AddListener(Connection);
        btnSend.onClick.AddListener(Send);
    }

    private void Update()
    {
        text.text = _recvStr;
    }

    private void OnDestroy()
    {
        btnConnect.onClick.RemoveAllListeners();
        btnSend.onClick.RemoveAllListeners();
    }

    public void Connection()
    {
        // Socket
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        
        // Connect
        _socket.BeginConnect(IP, Port,ConnectCallback,_socket);
    }

    public void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket) ar.AsyncState;
            socket.EndConnect(ar);
            Debug.Log("Socket Connect Succ");
            socket.BeginReceive(_readBuff, _buffCount, 1024-_buffCount, 0, ReceiveCallback, socket);
        }
        catch (SocketException ex)
        {
            Debug.Log("Socket Connect fail " + ex.ToString());
        }
    }

    public void Send()
    {
        string sendStr = inputField.text;
        
        // 组装协议
        byte[] bodyBytes = System.Text.Encoding.Default.GetBytes(sendStr);
        Int16 len = (Int16) bodyBytes.Length;
        byte[] lenBytes = BitConverter.GetBytes(len);
        byte[] sendBytes = lenBytes.Concat(bodyBytes).ToArray();

        // _sendLength = sendBytes.Length;
        // _readIndex = 0;
        
        // ByteArray
        ByteArray ba = new ByteArray(sendBytes);
        int count = 0;
        lock(writeQueue){
            writeQueue.Enqueue(ba);
            count = writeQueue.Count;
        }

        // Send
        if (count == 1)
        {
            _socket.BeginSend(ba.bytes, ba.readIdx, ba.length, 0, SendCallback, _socket);
            Debug.Log("[Send]" + BitConverter.ToString(sendBytes));
        }

        
        
        // Recv
        // byte[] readBuff = new byte[1024];
        // int count = _socket.Receive(readBuff);
        // string recvStr = System.Text.Encoding.Default.GetString(readBuff, 0, count);
        //
        // text.text = recvStr;
        // _socket.Close();
    }

    public void SendCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket) ar.AsyncState;
            int count = socket.EndSend(ar);
            
            //判断是否发送完整  
            ByteArray ba;
            lock(writeQueue){
                ba = writeQueue.First();
            }
            ba.readIdx+=count;
            if(ba.length == 0){   //发送完整
                lock(writeQueue){
                    writeQueue.Dequeue();
                    ba = writeQueue.First();
                }
            }

            if (ba != null)
            {
                //发送不完整，或发送完整且存在第二条数据
                socket.BeginSend(ba.bytes, ba.readIdx, ba.length, 0, SendCallback, socket);
            }

            // _readIndex += count;
            // _sendLength -= count;

            // if (_sendLength > 0)
            // {
            //     socket.BeginSend(_sendBytes, _readIndex, _sendLength, 0, SendCallback, socket);
            // }
            
            //Debug.Log("Socket Send succ" + count);
        }
        catch (SocketException ex)
        {
            Debug.Log("Socket Send fail" + ex.ToString());
        }
    }

    public void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket) ar.AsyncState;
            int count = socket.EndReceive(ar);

            _buffCount += count;
            // 处理二进制消息
            OnReceiveData();
            
            // 手动模拟粘包
            System.Threading.Thread.Sleep(1000*10);
            
            // 继续接收数据
            socket.BeginReceive(_readBuff, _buffCount, 1024 - _buffCount, 0, ReceiveCallback, socket);
        }
        catch (SocketException ex)
        {
            Debug.Log("Socket Receive fail" + ex.ToString());
        }
    }

    public void OnReceiveData()
    {
        Debug.Log("[Recv 1] buffCount = " + _buffCount);
        Debug.Log("[Recv 2] readbuff = " + BitConverter.ToString(_readBuff));
        
        // 消息长度
        if (_buffCount <= 2) return;
        Int16 bodyLength = BitConverter.ToInt16(_readBuff, 0);
        Debug.Log("[Recv 3] bodyLength = " + bodyLength);
        
        // 消息体
        if (_buffCount < 2 + bodyLength) return;
        string s = System.Text.Encoding.Default.GetString(_readBuff, 2, bodyLength);
        Debug.Log("[Recv 4] s = " + s);
        
        // 更新缓冲区
        int start = 2 + bodyLength;
        int count = _buffCount - start;
        Array.Copy(_readBuff,start,_readBuff,0,count);
        _buffCount -= start;
        Debug.Log("[Recv 5] buffCount = " + _buffCount);
        
        // 消息处理
        _recvStr = string.Format("{0}\n{1}", s, _recvStr);
        
        // 继续读取消息
        OnReceiveData();
    }
}

