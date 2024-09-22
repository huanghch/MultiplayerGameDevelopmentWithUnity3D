using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    //距离矢量
    public Vector3 distance = new Vector3(0, 8, -18);
    //相机
    new public Camera camera;
    //偏移值
    public Vector3 offset = new Vector3(0, 5f, 0);
    //相机移动速度
    public float speed = 3f;
    // Use this for initialization
    void Start () {
        //默认为主相机
        camera = Camera.main;
        //相机初始位置
        camera.transform.position = transform.position - 30 * transform.forward + Vector3.up * 10; 
    }

    //所有组件update之后发生
    void LateUpdate () {
        //坦克位置&方向
        Vector3 tankPos = transform.position;
        Vector3 tankForward = transform.forward;
        //相机目标位置
        Vector3 cameraNextPos = tankPos;
        cameraNextPos = tankPos + tankForward*distance.z;
        cameraNextPos.y += distance.y;
        //相机当前位置
        Vector3 cameraCurrentPos = camera.transform.position;
        cameraCurrentPos = Vector3.MoveTowards(cameraCurrentPos, cameraNextPos,Time.deltaTime*speed);
        camera.transform.position = cameraCurrentPos;
        //对准坦克
        camera.transform.LookAt(tankPos + offset);
    }
}
