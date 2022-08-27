using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using UnityEngine.UI;

public class Echo : MonoBehaviour
{
    private const string IP = "127.0.0.1";
    private const int Port = 8888;
    
    private Socket _socket;
    
    public Button btnConnect;
    public Button btnSend;
    public InputField inputField;
    public Text text;

    private void Start()
    {
        btnConnect.onClick.AddListener(Connection);
        btnSend.onClick.AddListener(Send);
    }

    public void Connection()
    {
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
        _socket.Send(sendBytes);
        
        //Recv
        byte[] readBuff = new byte[1024];
        int count = _socket.Receive(readBuff);
        string recvStr = System.Text.Encoding.Default.GetString(readBuff, 0, count);

        text.text = recvStr;
        _socket.Close();
    }

    private void OnDestroy()
    {
        btnConnect.onClick.RemoveAllListeners();
        btnSend.onClick.RemoveAllListeners();
    }
}
