using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseTank : MonoBehaviour
{
    //坦克模型
    private GameObject skin;
    
    //转向速度
    public float steer = 20;
    //移动速度
    public float speed = 3f;

    //初始化
    public virtual void Init(string skinPath){ 
        GameObject skinRes = ResManager.LoadPrefab(skinPath);
        skin = (GameObject)Instantiate(skinRes);
        skin.transform.parent = this.transform;
        skin.transform.localPosition = Vector3.zero;
        skin.transform.localEulerAngles = Vector3.zero; 
    }

    public void Update()
    {
        
    }
}
