using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Net.Proto;
using UnityEngine;
using UnityEngine.UI;

public class Main : MonoBehaviour
{
    private const string IP = "127.0.0.1";
    private const int Port = 8888;
    
    public Button btnConnect;
    public Button btnSend;
    public Button btnClose;
    
    private void Start()
    {
        btnConnect.onClick.AddListener(OnConnectClick);
        btnSend.onClick.AddListener(OnSendClick);
        btnClose.onClick.AddListener(OnCloseClick);
        
        NetManager.AddEventListener(NetManager.NetEvent.ConnectSucc, OnConnectSucc);
        NetManager.AddEventListener(NetManager.NetEvent.ConnectFail, OnConnectFail);
        NetManager.AddEventListener(NetManager.NetEvent.Close, OnConnectClose);
        NetManager.AddMsgListener("MsgMove", OnMsgMove);
    }
    
    public void Update()
    {
        NetManager.Update();
    }

    public void OnConnectClick()
    {
        NetManager.Connect(IP,Port);
    }

    public void OnSendClick()
    {
        MsgMove msgMove = new MsgMove();
        msgMove.x = 1;
        msgMove.y = 1;
        msgMove.z = 1;
        NetManager.Send(msgMove);
    }

    public void OnCloseClick()
    {
        NetManager.Close();
    }

    public void OnMsgMove(MsgBase msgBase)
    {
        MsgMove msg = (MsgMove) msgBase;
        // 消息处理
        Debug.Log("OnMsgMove msg.x = " + msg.x);
        Debug.Log("OnMsgMove msg.y = " + msg.y);
        Debug.Log("OnMsgMove msg.z = " + msg.z);
    }
    
    public void OnConnectSucc(string s)
    {
        Debug.Log("OnConnectSucc");
    }
    public void OnConnectFail(string s)
    {
        Debug.Log("OnConnectFail");
    }
    public void OnConnectClose(string s)
    {
        Debug.Log("OnConnectClose");
    }




    /*// Net
    private const string IP = "127.0.0.1";
    private const int Port = 8888;
    
    // 人物模型预设
    public GameObject humanPrefab;
    // 人物列表
    public BaseHuman myHuman;
    public Dictionary<string, BaseHuman> otherHumans;

    // Start is called before the first frame update
    void Start()
    {
        // 网络模块
        // NetManager.AddListener("Enter", OnEnter);
        // NetManager.AddListener("List", OnList);
        // NetManager.AddListener("Move", OnMove);
        // NetManager.AddListener("Leave", OnLeave);
        NetManager.Connect(IP, Port);
        
        // 添加一个角色
        GameObject obj = (GameObject) Instantiate(humanPrefab);
        float x = Random.Range(-5, 5);
        float z = Random.Range(-5, 5);
        obj.transform.position = new Vector3(x, 0, z);
        myHuman = obj.AddComponent<CtrlHuman>();
        //myHuman.desc = NetManager.GetDesc();
        
        // 发送Enter协议
        Vector3 pos = myHuman.transform.position;
        Vector3 eul = myHuman.transform.eulerAngles;
        StringBuilder str = new StringBuilder("Enter|");
        //str.Append(NetManager.GetDesc());
        str.Append(",");
        str.Append(pos.x);
        str.Append(",");
        str.Append(pos.y);
        str.Append(",");
        str.Append(pos.z);
        str.Append(",");
        str.Append(eul.y);
        //NetManager.Send(str.ToString());
        
        // 请求玩家列表
        //NetManager.Send("List|");

    }

    // Update is called once per frame
    void Update()
    {
        NetManager.Update();
    }

    void OnEnter(string msgArgs)
    {
        Debug.Log("OnEnter|" + msgArgs);
        
        // 解析参数
        string[] split = msgArgs.Split(',');
        string desc = split[0];
        float x = float.Parse(split[1]);
        float y = float.Parse(split[2]);
        float z = float.Parse(split[3]);
        float eulY = float.Parse(split[4]);
        
        // 是自己
        //if (desc == NetManager.GetDesc()) return;
        
        // 添加一个角色
        GameObject obj = (GameObject) Instantiate(humanPrefab);
        obj.transform.position = new Vector3(x, y, z);
        obj.transform.eulerAngles = new Vector3(0, eulY, 0);
        BaseHuman h = obj.AddComponent<SyncHuman>();
        h.desc = desc;
        otherHumans.Add(desc, h);
    }

    void OnList(string msgArgs)
    {
        Debug.Log("OnList|" + msgArgs);
        string[] split = msgArgs.Split(',');
        int count = (split.Length - 1) / 6;
        for (int i = 0; i < count; i++)
        {
            string desc = split[i * 6];
            float x = float.Parse(split[i * 6 + 1]);
            float y = float.Parse(split[i * 6 + 2]);
            float z = float.Parse(split[i * 6 + 3]);
            float eulY = float.Parse(split[i * 6 + 4]);
            int hp = int.Parse(split[i * 6 + 5]);
            // 是自己
            //if (desc == NetManager.GetDesc()) return;
            // 添加一个角色
            GameObject obj = (GameObject) Instantiate(humanPrefab);
            obj.transform.position = new Vector3(x, y, z);
            obj.transform.eulerAngles = new Vector3(0, eulY, z);
            BaseHuman h = obj.AddComponent<SyncHuman>();
            h.desc = desc;
            otherHumans.Add(desc, h);
        }
    }

    void OnMove(string msgArgs)
    {
        Debug.Log("OnMove|" + msgArgs);
    }

    void OnLeave(string msgArgs)
    {
        Debug.Log("OnLeave|" + msgArgs);
    }*/
}
