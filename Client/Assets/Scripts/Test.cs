using System.Collections;
using System.Collections.Generic;
using Net.Proto;
using UnityEngine;
using UnityEngine.UI;

public class Test : MonoBehaviour
{
    private const string IP = "127.0.0.1";
    private const int Port = 8888;
    
    public Button btnConnect;
    public Button btnClose;

    public Button btnLogin;
    public Button btnRegister;
    public Button btnSave;

    public InputField idInput;
    public InputField pwInput;
    public InputField textInput;
    //开始
    void Start(){
        btnConnect.onClick.AddListener(OnConnectClick);
        btnClose.onClick.AddListener(OnCloseClick);
        
        btnLogin.onClick.AddListener(OnLoginClick);
        btnRegister.onClick.AddListener(OnRegisterClick);
        btnSave.onClick.AddListener(OnSaveClick);

        NetManager.AddEventListener(NetManager.NetEvent.ConnectSucc, OnConnectSucc);
        NetManager.AddEventListener(NetManager.NetEvent.ConnectFail, OnConnectFail);
        NetManager.AddEventListener(NetManager.NetEvent.Close, OnConnectClose);

        NetManager.AddMsgListener("MsgRegister", OnMsgRegister);
        NetManager.AddMsgListener("MsgLogin", OnMsgLogin);
        NetManager.AddMsgListener("MsgKick", OnMsgKick);
        NetManager.AddMsgListener("MsgGetText", OnMsgGetText);
        NetManager.AddMsgListener("MsgSaveText", OnMsgSaveText);
    }

    //玩家点击连接按钮OnConnectClick
    public void OnConnectClick()
    {
        NetManager.Connect(IP,Port);
    }

    //主动关闭OnCloseClick
    public void OnCloseClick()
    {
        NetManager.Close();
    }

    //连接成功回调 OnConnectSucc
    public void OnConnectSucc(string s)
    {
        Debug.Log("OnConnectSucc");
    }
    
    //连接成功回调 OnConnectSucc
    public void OnConnectFail(string s)
    {
        Debug.Log("OnConnectFail");
    }
    
    //关闭连接 OnConnectClose
    public void OnConnectClose(string s)
    {
        Debug.Log("OnConnectClose");
    }
    
    //Update
    public void Update()
    {
        NetManager.Update();
    }

    //发送注册协议
    public void OnRegisterClick () {
        MsgRegister msg = new MsgRegister();
        msg.id = idInput.text;
        msg.pw = pwInput.text;
        NetManager.Send(msg);
    }

    //收到注册协议
    public void OnMsgRegister (MsgBase msgBase) {
        MsgRegister msg = (MsgRegister)msgBase;
        if(msg.result == 0){
            Debug.Log("注册成功");
        }
        else{
            Debug.Log("注册失败");
        }
    } 
    
    //发送登录协议
    public void OnLoginClick () {
        MsgLogin msg = new MsgLogin();
        msg.id = idInput.text;
        msg.pw = pwInput.text;
        NetManager.Send(msg);
    }

    //收到登录协议
    public void OnMsgLogin (MsgBase msgBase) {
        MsgLogin msg = (MsgLogin)msgBase;
        if(msg.result == 0){
            Debug.Log("登录成功");
            //请求记事本文本
            MsgGetText msgGetText = new MsgGetText();
            NetManager.Send(msgGetText);
        }
        else{
            Debug.Log("登录失败");
        }
    } 

    //被踢下线
    void OnMsgKick(MsgBase msgBase){
        Debug.Log("被踢下线");
    } 
    
    //收到记事本文本协议
    public void OnMsgGetText (MsgBase msgBase) {
        MsgGetText msg = (MsgGetText)msgBase;
        textInput.text = msg.text;
    }
    
    //发送保存协议
    public void OnSaveClick () {
        MsgSaveText msg = new MsgSaveText();
        msg.text = textInput.text;
        NetManager.Send(msg);
    }

    //收到保存协议
    public void OnMsgSaveText (MsgBase msgBase) {
        MsgSaveText msg = (MsgSaveText)msgBase;
        if(msg.result == 0){
            Debug.Log("保存成功");
        }
        else{
            Debug.Log("保存失败");
        }
    } 
}
