using System;
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
    private string _recvStr = "";
    
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
            socket.BeginReceive(_readBuff, 0, 1024, 0, ReceiveCallback, socket);
        }
        catch (SocketException ex)
        {
            Debug.Log("Socket Connect fail " + ex.ToString());
        }
    }

    public void Send()
    {
        // Send
        string sendStr = inputField.text;
        byte[] sendBytes = System.Text.Encoding.Default.GetBytes(sendStr);
        _socket.BeginSend(sendBytes, 0, sendBytes.Length, 0, SendCallback, _socket);
        
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
            Debug.Log("Socket Send succ" + count);
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
            string s = System.Text.Encoding.Default.GetString(_readBuff, 0, count);
            _recvStr = string.Format("{0}\n{1}", s, _recvStr);

            socket.BeginReceive(_readBuff, 0, 1024, 0, ReceiveCallback, socket);
        }
        catch (SocketException ex)
        {
            Debug.Log("Socket Receive fail" + ex.ToString());
        }
    }
}
