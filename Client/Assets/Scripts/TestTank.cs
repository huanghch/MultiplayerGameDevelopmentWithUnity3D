using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestTank : MonoBehaviour
{
    // Start is called before the first frame update
    // void Start()
    // {
    //     GameObject skinRes = ResManager.LoadPrefab("Prefabs/TankPrefab/tankPrefab");
    //     GameObject skin = (GameObject) Instantiate(skinRes);
    // }
    
    public void Start()
    {
        string skinPath = "Prefabs/TankPrefab/tankPrefab";
        GameObject tankObj = new GameObject("myTank");
        CtrlTank ctrlTank = tankObj.AddComponent<CtrlTank>();
        ctrlTank.Init(skinPath);

        tankObj.AddComponent<CameraFollow>();
    }
}
