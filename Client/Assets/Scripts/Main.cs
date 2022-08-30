using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class Main : MonoBehaviour
{
    // Net
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
        NetManager.AddListener("Enter", OnEnter);
        NetManager.AddListener("Move", OnMove);
        NetManager.AddListener("Leave", OnLeave);
        NetManager.Connect(IP, Port);
        
        // 添加一个角色
        GameObject obj = (GameObject) Instantiate(humanPrefab);
        float x = Random.Range(-5, 5);
        float z = Random.Range(-5, 5);
        obj.transform.position = new Vector3(x, 0, z);
        myHuman = obj.AddComponent<CtrlHuman>();
        myHuman.desc = NetManager.GetDesc();
        
        // 发送协议
        Vector3 pos = myHuman.transform.position;
        Vector3 eul = myHuman.transform.eulerAngles;
        StringBuilder str = new StringBuilder("Enter|");
        str.Append(NetManager.GetDesc());
        str.Append(",");
        str.Append(pos.x);
        str.Append(",");
        str.Append(pos.y);
        str.Append(",");
        str.Append(pos.z);
        str.Append(",");
        str.Append(eul.y);
        NetManager.Send(str.ToString());

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
        if (desc == NetManager.GetDesc()) return;
        
        // 添加一个角色
        GameObject obj = (GameObject) Instantiate(humanPrefab);
        obj.transform.position = new Vector3(x, y, z);
        obj.transform.eulerAngles = new Vector3(0, eulY, 0);
        BaseHuman h = obj.AddComponent<SyncHuman>();
        h.desc = desc;
        otherHumans.Add(desc, h);
    }

    void OnMove(string msgArgs)
    {
        Debug.Log("OnMove|" + msgArgs);
    }

    void OnLeave(string msgArgs)
    {
        Debug.Log("OnLeave|" + msgArgs);
    }
}
