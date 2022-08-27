using System;
using UnityEngine;
using System.Net.Sockets;
using UnityEngine.UI;

public class Poll : MonoBehaviour
{
    private const string IP = "127.0.0.1";
    private const int Port = 8888;
    
    private Socket _socket;

    public Button btnConnect;
    public Button btnSend;
    public InputField inputField;
    public Text text;
    // Start is called before the first frame update
    void Start()
    {
        btnConnect.onClick.AddListener(Connection);
        btnSend.onClick.AddListener(Send);
    }

    // Update is called once per frame
    void Update()
    {
        if (_socket == null) return;

        if (_socket.Poll(0, SelectMode.SelectRead))
        {
            byte[] readBuff = new byte[1024];
            int count = _socket.Receive(readBuff);
            string recvStr = System.Text.Encoding.Default.GetString(readBuff, 0, count);
            text.text = recvStr;
        }
    }

    public void Connection()
    {
        // Socket
        // Socket
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        
        // Connect
        _socket.Connect(IP, Port);
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
}
