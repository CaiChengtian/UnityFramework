using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityFramework;


public class MainPanel : Panel
{



    void Start()
    {

    }

    void Update()
    {

    }

    public override void Show(Context context = null)
    {
        base.Show(context);
    }

    public override Context GetContext(Stack<Context> popupNavigation = null)
    {
        throw new System.NotImplementedException();
    }

    protected override void InitData()
    {
        PanelID = PanelID.Main;
        PrePanelID = PanelID.Invalid;
        PanelData = new PanelData(
            PanelType.Normal,
            PanelColliderMode.Image,
            PanelNavigationMode.Navigation,
            PanelPopupMode.HideOther,
            //进入主界面后,清空导航栈
            true
        );
    }

    protected override void InitUI()
    {

    }
}

