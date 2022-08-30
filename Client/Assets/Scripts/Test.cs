using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{

    public delegate void DelegateStr(string str);

    public static void PrintStr(string str)
    {
        Debug.Log(str);
    }
    
    public static void PrintStr2(string str)
    {
        Debug.Log(str);
    }
    
    // Start is called before the first frame update
    void Start()
    {
        DelegateStr fun = new DelegateStr(PrintStr);
        fun += PrintStr2;
        fun -= PrintStr;
        fun -= PrintStr2;
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(Time.deltaTime);
    } 
}
