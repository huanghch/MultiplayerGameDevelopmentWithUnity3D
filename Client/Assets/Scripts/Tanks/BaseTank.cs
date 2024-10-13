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
    //物理
    protected Rigidbody rigidBody; 
    
    //炮塔旋转速度
    public float turretSpeed = 30f; 
    //炮塔
    public Transform turret;
    //炮管
    public Transform gun;
    //发射点
    public Transform firePoint; 
    
    //炮弹Cd时间
    public float fireCd = 0.5f; 
    //上一次发射炮弹的时间
    public float lastFireTime = 0; 
    
    //生命值
    public float hp = 100;

    //初始化
    public virtual void Init(string skinPath){ 
        GameObject skinRes = ResManager.LoadPrefab(skinPath);
        skin = (GameObject)Instantiate(skinRes);
        skin.transform.parent = this.transform;
        skin.transform.localPosition = Vector3.zero;
        skin.transform.localEulerAngles = Vector3.zero; 
        
        //物理
        rigidBody = gameObject.AddComponent<Rigidbody>();
        BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
        boxCollider.center = new Vector3(0, 2.5f, 1.47f);
        boxCollider.size = new Vector3(7, 5, 12);
        
        //炮塔炮管
        turret = skin.transform.Find("Turret");
        gun = turret.transform.Find("Gun");
        firePoint = gun.transform.Find("FirePoint");
    }

    public void Update()
    {
        
    }
    
    //是否死亡
    public bool IsDie(){
        return hp <= 0;
    }
    
    //发射炮弹
    public Bullet Fire(){
        //已经死亡
        if(IsDie()){
            return null;
        }
        //产生炮弹
        GameObject bulletObj = new GameObject("bullet");
        Bullet bullet = bulletObj.AddComponent<Bullet>();
        bullet.Init();
        bullet.tank = this;
        //位置
        bullet.transform.position = firePoint.position;
        bullet.transform.rotation = firePoint.rotation;
        //更新时间
        lastFireTime = Time.time;
        return bullet;
    } 
    
    //被攻击
    public void Attacked(float damage){
        //已经死亡
        if(IsDie()){
            return;
        }
        //扣血
        hp -= damage;
        //死亡
        if(IsDie()){
            //显示焚烧效果
            Debug.Log("explosion");
            // GameObject obj = ResManager.LoadPrefab("explosion");
            // GameObject explosion = Instantiate(obj, transform.position, transform.rotation);
            // explosion.transform.SetParent(transform);
        }
    } 
}
