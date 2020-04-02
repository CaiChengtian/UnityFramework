/*
	Copyright (c) 2017 Tiantian. All rights reserved.
	Description:
*/
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityFramework;

public class LoginController : Controller<LoginController>
{
    private LoginController()
    {
    }

    public override void Init()
    {
        NetManager.Instance.Register(Protocol.System_Login, ReceiveLogin);
    }

    private void ReceiveLogin(byte[] bytes)
    {
        Log.TT("Receive Login");
        LoginReceive receive = Utility.FromBytes<LoginReceive>(bytes);
        Log.TT(receive.Name);
        SceneManager.LoadScene("Main");
        UIManager.Instance.ShowPanel(PanelID.Main);
    }
}
