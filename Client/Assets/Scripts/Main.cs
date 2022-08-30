using System.Collections;
using System.Collections.Generic;
using UnityEditor.VersionControl;
using UnityEngine;

public class Main : MonoBehaviour
{
    private const string IP = "127.0.0.1";
    private const int Port = 8888;
    
    // Start is called before the first frame update
    void Start()
    {
        NetManager.AddListener("Enter", OnEnter);
        NetManager.AddListener("Move", OnMove);
        NetManager.AddListener("Leave", OnLeave);
        NetManager.Connect(IP, Port);
    }

    // Update is called once per frame
    void Update()
    {
        NetManager.Update();
    }

    void OnEnter(string msg)
    {
        Debug.Log("OnEnter" + msg);
    }

    void OnMove(string msg)
    {
        Debug.Log("OnMove" + msg);
    }

    void OnLeave(string msg)
    {
        Debug.Log("OnLeave" + msg);
    }
}
