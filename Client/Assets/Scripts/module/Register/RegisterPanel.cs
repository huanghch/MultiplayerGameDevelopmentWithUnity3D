using System.Collections;
using System.Collections.Generic;
using Net.Proto;
using UnityEngine;
using UnityEngine.UI;

public class RegisterPanel : BasePanel
{
    // 组件
    private InputField idInput;
    private InputField pwInput;
    private InputField repInput;
    private Button regBtn;
    private Button closeBtn;

    // 初始化
    public override void OnInit()
    {
        skinPath = "RegisterPanel";
        layer = PanelManager.Layer.Panel;
    }

    // 显示
    public override void OnShow(params object[] args)
    {
        // 绑定
        idInput = skin.transform.Find("Layout/ID/IdInput").GetComponent<InputField>();
        pwInput = skin.transform.Find("Layout/PW/PwInput").GetComponent<InputField>();
        repInput = skin.transform.Find("Layout/RepPW/RepInput").GetComponent<InputField>();
        regBtn = skin.transform.Find("Layout/Register/RegisterBtn").GetComponent<Button>();
        closeBtn = skin.transform.Find("CloseBtn").GetComponent<Button>();
        // 监听
        regBtn.onClick.AddListener(OnRegClicke);
        closeBtn.onClick.AddListener(OnCloseClick);
        // 网络协议监听
        NetManager.AddMsgListener("MsgRegister", OnMsgRegister);
    }

    // 关闭
    public override void OnClose()
    {
        NetManager.RemoveMsgListener("MsgRegister", OnMsgRegister);
    }
    
    // Buttons
    public void OnRegClicke()
    {
        if (idInput.text == "" || pwInput.text == "")
        {
            PanelManager.Open<TipPanel>("用户名和密码不能为空");
            return;
        }

        if (repInput.text != pwInput.text)
        {
            PanelManager.Open<TipPanel>("两次输入的密码不同");
            return;
        }
        // 发送
        MsgRegister msgReg = new MsgRegister();
        msgReg.id = idInput.text;
        msgReg.pw = pwInput.text;
        NetManager.Send(msgReg);
    }

    public void OnCloseClick()
    {
        Close();
    }
    
    // 收到注册协议
    public void OnMsgRegister(MsgBase msgBase)
    {
        MsgRegister msg = (MsgRegister) msgBase;
        if (msg.result == 0)
        {
            Debug.Log("注册成功");
            // 提示
            PanelManager.Open<TipPanel>("注册成功");
            // 关闭界面
            Close();
        }
        else
        {
            Debug.Log("注册失败");
            PanelManager.Open<TipPanel>("注册失败");
        }
    }
}
