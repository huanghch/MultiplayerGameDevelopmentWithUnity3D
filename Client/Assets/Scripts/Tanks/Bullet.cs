using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {
    //移动速度
    public float speed = 100f;
    // 伤害
    public float damage = 35f;
    //发射者
    public BaseTank tank;
    //炮弹模型
    private GameObject skin;
    //物理
    Rigidbody rigidBody;

    //初始化
    public void Init(){
        //皮肤
        GameObject skinRes = ResManager.LoadPrefab("Prefabs/bulletPrefab");
        skin = (GameObject)Instantiate(skinRes);
        skin.transform.parent = this.transform;
        skin.transform.localPosition = Vector3.zero;
        skin.transform.localEulerAngles = Vector3.zero;
        //物理
        rigidBody = gameObject.AddComponent<Rigidbody>();
        rigidBody.useGravity = false;
    }
    
    // Update is called once per frame
    void Update () {
        //向前移动
        transform.position += transform.forward * speed * Time.deltaTime;
    }
    
    //碰撞
    void OnCollisionEnter(Collision collisionInfo) {
        //打到的坦克
        GameObject collObj = collisionInfo.gameObject;
        BaseTank hitTank = collObj.GetComponent<BaseTank>();
        //不能打自己
        if(hitTank == tank){
            return;
        }
        //打到其他坦克
        if(hitTank != null){
            hitTank.Attacked(damage);
        }
        //显示爆炸效果
        Debug.Log("Hit");  
        // GameObject explode = ResManager.LoadPrefab("Prefabs/Particles/fire");
        // Instantiate(explode, transform.position, transform.rotation);
        //摧毁自身
        Destroy(gameObject);
    }
}