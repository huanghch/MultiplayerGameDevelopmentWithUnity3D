using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TipPanel : BasePanel
{
    // 组件
    private Text text;
    private Button okBtn;
    
    public override void OnInit()
    {
        skinPath = "TipPanel";
        layer = PanelManager.Layer.Tip;
    }
    // 显示
    public override void OnShow(params object[] args)
    {
        // 绑定
        text = skin.transform.Find("TipText").GetComponent<Text>();
        okBtn = skin.transform.Find("OkBtn").GetComponent<Button>();
        // 监听
        okBtn.onClick.AddListener(OnOkClick);
        // 提示语
        if (args.Length == 1)
        {
            text.text = (string) args[0];
        }
    }

    public override void OnClose(){}

    public void OnOkClick()
    {
        Close();
    }
}
