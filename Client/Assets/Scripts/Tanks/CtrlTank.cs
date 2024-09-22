using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CtrlTank : BaseTank
{
    new void Update(){
        base.Update();
        //移动控制
        MoveUpdate();
        //炮塔控制
        TurretUpdate();
    }

    //移动控制
    public void MoveUpdate()
    {
        //旋转
        float x = Input.GetAxis("Horizontal");
        transform.Rotate(0, x * steer * Time.deltaTime, 0);
        //前进后退
        float y = Input.GetAxis("Vertical");
        Vector3 s = y * transform.forward * speed * Time.deltaTime;
        transform.transform.position += s;
    }
    
    //炮塔控制
    public void TurretUpdate(){
        //或者轴向
        float axis = 0;
        if(Input.GetKey(KeyCode.Q)){
            axis = -1;
        }
        else if(Input.GetKey(KeyCode.E)){
            axis = 1;
        }
        //旋转角度
        Vector3 le = turret.localEulerAngles;
        le.y += axis * Time.deltaTime * turretSpeed;
        turret.localEulerAngles = le;
    }
}
