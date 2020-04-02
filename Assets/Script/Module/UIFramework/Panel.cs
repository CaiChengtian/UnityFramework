/*
	Copyright (c) 2017 Tiantian. All rights reserved.
	Description:UI界面基类,所有UI界面类都必须继承它.
*/
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityFramework
{
    /// <summary>
    /// UI界面基类
    /// </summary>
    public abstract class Panel : MonoBehaviour
    {
        private PanelID panelID;
        //private PanelID prePanelID;
        private PanelData panelData;
        //private bool IsShown = false;
        //private bool IsLock = false;
        private int depth;
        private List<PanelID> fixedPanels;


        /// <summary>
        /// 界面ID
        /// </summary>
        public PanelID PanelID
        {
            get
            {
                UnityEngine.Assertions.Assert.AreNotEqual<PanelID>(PanelID.Invalid, panelID);
                return panelID;
            }
            protected set { panelID = value; }
        }

        /// <summary>
        /// 默认的上级界面
        /// </summary>
        public PanelID PrePanelID
        {
            get;
            protected set;
        }

        /// <summary>
        /// 界面数据
        /// </summary>
        public PanelData PanelData
        {
            get { return panelData; }
            protected set { panelData = value; }
        }

        /// <summary>
        /// 该界面是否正在显示中
        /// </summary>
        public bool IsShown
        {
            get;
            private set;
        }

        /// <summary>
        /// (Normal界面)附属的Fixed界面列表
        /// </summary>
        /// <returns></returns>
        public List<PanelID> FixedPanels
        {
            get { return fixedPanels; }
            protected set
            {
                if (PanelData.Type == PanelType.Normal)
                {
                    fixedPanels = value;
                }
                else
                {
                    Log.Warn(String.Format("Failed to set fixed panel list : only normal panel can be set."));
                    return;
                }
            }
        }


        protected virtual void Awake()
        {
            gameObject.SetActive(false);
            InitData();
            InitUI();
        }

        /// <summary>
        /// 获取界面上下文
        /// </summary>
        /// <param name="popupNavigation">Popup导航栈</param>
        public abstract Context GetContext(Stack<Context> popupNavigation = null);

        /// <summary>
        /// 初始化核心数据(PanelID,PrePanelID,PanelData...)
        /// </summary>
        protected abstract void InitData();

        /// <summary>
        /// 初始化UI
        /// </summary>
        protected abstract void InitUI();


        /// <summary>
        /// 打开
        /// </summary>
        /// <param name="context"></param>
        public virtual void Show(Context context = null)
        {
            Debug.Log("Show:"+ gameObject.ToString());
            IsShown = true;
            gameObject.SetActive(true);
        }

        /// <summary>
        /// 关闭
        /// </summary>
        /// <param name="action">回调</param>
        public virtual void Hide(Action action = null)
        {
            IsShown = false;
            gameObject.SetActive(false);
            if (action != null)
            {
                action();
            }
        }

        /// <summary>
        /// 强制关闭
        /// </summary>
        public void HideDirectly()
        {
            gameObject.SetActive(false);
            IsShown = false;
        }

        /// <summary>
        /// 销毁
        /// </summary>
        public virtual void Destroy()
        {
            BeforeDestroyPanel();
            Destroy(this.gameObject);
        }

        /// <summary>
        /// 销毁前的处理
        /// </summary>
        protected virtual void BeforeDestroyPanel()
        {
        }

        /// <summary>
        /// 重置
        /// </summary>
        public virtual void ResetPanel()
        {
        }


        /// <summary>
        /// 添加背景阻挡层
        /// </summary>
        private void AddColliderBG()
        {
            PanelColliderMode colliderMode = panelData.ColliderMode;
            if (colliderMode == PanelColliderMode.None)
            {
                return;
            }

            GameObject bg = new GameObject("bg");
            RectTransform rt = bg.AddComponent<RectTransform>();
            rt.SetParent(transform);
            rt.SetSiblingIndex(0);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = Vector2.zero;

            Image image = bg.AddComponent<Image>();
            Button button = bg.AddComponent<Button>();
            button.onClick.AddListener(OnClickCollider);

            //透明
            if (colliderMode == PanelColliderMode.Transparent)
            {
                //BUG
                image.color = new Color(0, 0, 0, 0);
            }
            else if (colliderMode == PanelColliderMode.translucent)
            {
                image.color = new Color(0, 0, 0, 0.25f);
            }
            else if (colliderMode == PanelColliderMode.Image)
            {
                image.sprite = UIConfig.ColliderSprite;
            }

        }


        /// <summary>
        /// 点击背景阻挡层
        /// </summary>
        protected virtual void OnClickCollider()
        {

        }


    }
}
