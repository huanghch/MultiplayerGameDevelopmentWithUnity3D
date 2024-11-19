using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static string id = "";
    
    void Start()
    {
        //网络事件监听
        NetManager.AddEventListener(NetManager.NetEvent.Close, OnConnectClose);
        NetManager.AddMsgListener("MsgKick", OnMsgKick);

        //初始化
        PanelManager.Init();
        //打开登录面板
        PanelManager.Open<LoginPanel>();
    }
    
    void Update()
    {
        NetManager.Update();
    }
    
    
    //关闭连接
    private void OnConnectClose(string err){
        Debug.Log("断开连接");
    } 

    //被踢下线
    private void OnMsgKick(MsgBase msgBase){
        //PanelManager.Open<TipPanel>("被踢下线");
    }
}
