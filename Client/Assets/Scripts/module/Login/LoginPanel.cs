using System.Collections;
using System.Collections.Generic;
using Net.Proto;
using UnityEngine;
using UnityEngine.UI;

public class LoginPanel : BasePanel
{
    // 连接地址，一般不写在这里
    private const string IP = "127.0.0.1";
    private const int Port = 8888;
    
    // 组件
    private InputField idInput;
    private InputField pwInput;
    private Button loginBtn;
    private Button regBtn;
    private Button connectBtn;
    private Button closeBtn;
    
    // 初始化
    public override void OnInit()
    {
        skinPath = "LoginPanel";
        layer = PanelManager.Layer.Panel;
    }
    // 显示
    public override void OnShow(params object[] args)
    {
        // 绑定
        idInput = skin.transform.Find("Layout/ID/IdInput").GetComponent<InputField>();
        pwInput = skin.transform.Find("Layout/PW/PwInput").GetComponent<InputField>();
        loginBtn = skin.transform.Find("Layout/Btns/LoginBtn").GetComponent<Button>();
        regBtn = skin.transform.Find("Layout/Btns/RegisterBtn").GetComponent<Button>();
        connectBtn = skin.transform.Find("Layout/Btns/ConnectBtn").GetComponent<Button>();
        closeBtn = skin.transform.Find("Layout/Btns/CloseBtn").GetComponent<Button>();
        // 监听
        loginBtn.onClick.AddListener(OnLoginClick);
        regBtn.onClick.AddListener(OnRegClick);
        connectBtn.onClick.AddListener(OnConnectClick);
        closeBtn.onClick.AddListener(OnCloseClick);
        // 网络事件监听
        NetManager.AddEventListener(NetManager.NetEvent.ConnectSucc, OnConnectSucc);
        NetManager.AddEventListener(NetManager.NetEvent.ConnectFail, OnConnectFail);
        NetManager.AddEventListener(NetManager.NetEvent.Close, OnCloseSucc);
        // 网络协议监听
        NetManager.AddMsgListener("MsgLogin", OnMsgLogin);
    }

    public override void OnClose()
    {
        NetManager.RemoveEventListener(NetManager.NetEvent.ConnectSucc, OnConnectSucc);
        NetManager.RemoveEventListener(NetManager.NetEvent.ConnectFail, OnConnectFail);
        NetManager.RemoveMsgListener("MsgLogin", OnMsgLogin);
    }

    // Buttons
    private void OnLoginClick()
    {
        if (idInput.text == "" || pwInput.text == "")
        {
            PanelManager.Open<TipPanel>("用户名和密码不能为空");
            return;
        }
        // 发送登录协议
        MsgLogin msgLogin = new MsgLogin();
        msgLogin.id = idInput.text;
        msgLogin.pw = pwInput.text;
        NetManager.Send(msgLogin);
    }

    private void OnRegClick()
    {
        PanelManager.Open<RegisterPanel>();
    }

    private void OnConnectClick()
    {
        NetManager.Connect(IP, Port);
    }
    
    private void OnCloseClick()
    {
        NetManager.Close();
    }

    // Listener
    // 连接成功
    private void OnConnectSucc(string err)
    {
        Debug.Log("连接成功");
        PanelManager.Open<TipPanel>("连接成功");
    }

    // 连接失败
    private void OnConnectFail(string err)
    {
        Debug.Log("连接失败");
        PanelManager.Open<TipPanel>(err);
    }
    // 关闭连接
    private void OnCloseSucc(string err)
    {
        Debug.Log("已断开连接");
        PanelManager.Open<TipPanel>("已断开连接");
    }

    // 收到登录协议
    private void OnMsgLogin(MsgBase msgBase)
    {
        MsgLogin msg = (MsgLogin) msgBase;
        if (msg.result == 0)
        {
            Debug.Log("登录成功");
            // 进入游戏
            // 添加tank
            GameObject tankObj = new GameObject("myTank");
            CtrlTank ctrlTank = tankObj.AddComponent<CtrlTank>();
            ctrlTank.Init("tankPrefab");
            // 设置相机
            tankObj.AddComponent<CameraFollow>();
            // 关闭LoginPanel
            Close();
        }
        else
        {
            Debug.Log("登录失败");
            PanelManager.Open<TipPanel>("登录失败");
        }
    }
}
