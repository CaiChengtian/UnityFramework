/*
	Copyright (c) 2017 Tiantian. All rights reserved.
	Description:
*/
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

namespace UnityFramework
{
    /// <summary>
    /// 登陆界面
    /// </summary>
    public class LoginPanel : Panel
    {
		Button playButton;


        public override Context GetContext(Stack<Context> popupNavigation = null)
        {
			return new LoginContext();
        }

        protected override void InitData()
        {
            PanelID = PanelID.Login;
			PrePanelID = PanelID.Invalid;
            PanelData = new PanelData(
                PanelType.Normal,
                PanelColliderMode.Image,
                PanelNavigationMode.NoNavigation,
                PanelPopupMode.DoNothing
            );
        }

        protected override void InitUI()
        {
			playButton = transform.Find("Button_Login").GetComponent<Button>();
			playButton.onClick.AddListener(OnClickPlay);
        }

		private void OnClickPlay()
		{
			//SceneManager.LoadScene("Main");
            //UIManager.Instance.ShowPanel(PanelID.Main);
            LoginSend loginSend = new LoginSend();
            loginSend.Account = "cct";
            loginSend.Password = "123";
            NetManager.Instance.Send(loginSend);
		}
    }
}
