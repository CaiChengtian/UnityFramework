/*
	Copyright (c) 2017 Tiantian. All rights reserved.
	Description:UI界面管理器,负责各个UI界面的跳转和显示,打开/关闭UI界面都必须通过它来控制.
    核心规则:同一时间必须且只能显示一个Normal界面,可以显示多个Popup或者Fixed界面;
            UIManager主要导航Normal界面,此外还导航Normal界面上的Popup界面;
            关闭(或打开新的)Normal界面时,同时清除该界面的Popup导航(并关闭所有的Popup界面).

            关闭一个Normal界面时,Popup导航栈会转成Context栈,存储在该界面的Context中;
            不太准确的解释:
                关闭一个Normal界面时,其上打开的所有参与导航的Popup界面的Context,
                会被放在该Normal界面的Context中,作为其上下文的一部分;
                返回一个Normal界面时,取出该Normal界面的Context,依次打开记录的Popup界面,
                该Normal界面就回到了之前关闭时的状态.

            Fixed界面之间相互独立,也无需导航;
            通常不会通过UIManager直接打开(或关闭)一个Fixed界面,而是在每个Normal界面中声明
                其附带的Fixed界面,打开Normal界面时同时打开其附带的Fixed界面;
            跳转到另一个Normal界面之间时,匹配两个界面的Fixed声明:
                保留双方都有的,关闭不再需要的,然后打开新增的.
*/
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace UnityFramework
{
    /// <summary>
    /// UI界面管理器
    /// </summary>
    public class UIManager : Manager<UIManager>
    {
        //----------------------------------------Fields

        //所有还存在的界面(包括隐藏的,但不包括已被销毁的)
        private Dictionary<PanelID, Panel> allPanels;
        //Normal导航栈
        private Stack<Context> normalNavigation;
        //当前打开的Normal界面
        private Panel curNormalPanel;
        //Popup导航栈
        private Stack<Context> popupNavigation;
        //最近一个打开的导航Popup界面
        private Panel curPopupPanel;
        //(除了curPopupPanel外)其它打开的Popup界面
        private Dictionary<PanelID, Panel> shownPopupPanels;
        //打开的Fixed界面
        private Dictionary<PanelID, Panel> shownFixedPanels;
        //
        private GameObject eventSystem;
        //UI根节点
        private RectTransform canvas;
        private RectTransform normalCanvas;
        private RectTransform popupCanvas;
        private RectTransform fixedCanvas;


        //----------------------------------------Properties

        /// <summary>
        /// 当前正在显示的Normal界面
        /// </summary>
        public PanelID CurNormalPanelID
        {
            get { return curNormalPanel.PanelID; }
        }

        /// <summary>
        /// UI根节点
        /// </summary>
        public RectTransform Canvas
        {
            get { return canvas; }
        }
        public RectTransform NormalCanvas
        {
            get { return normalCanvas; }
        }
        public RectTransform PopupCanvas
        {
            get { return popupCanvas; }
        }
        public RectTransform FixedCanvas
        {
            get { return fixedCanvas; }
        }


        //----------------------------------------Mono Methods

        void Awake()
        {
            Init();
            eventSystem = GameObject.Find("EventSystem");
            MonoBehaviour.DontDestroyOnLoad(eventSystem);
            MonoBehaviour.DontDestroyOnLoad(canvas);
        }


        //----------------------------------------Public Methods

        /// <summary>
        /// 打开界面
        /// </summary>
        /// <param name="panelID">界面ID</param>
        /// <param name="context">上下文</param>
        public bool ShowPanel(PanelID panelID, Context context = null)
        {
            Panel panel = BeforeShowPanel(panelID);
            //Assert.IsNotNull<Panel>(panel);
            Debug.Log("ShowPanel:" + panelID.ToString());
            switch (panel.PanelData.Type)
            {
                //打开Normal界面
                case PanelType.Normal:
                    if (curNormalPanel != null)
                    {
                        //当前Normal界面压入导航栈
                        if (curNormalPanel.PanelData.NavigationMode == PanelNavigationMode.Navigation)
                        {
                            Context curContext = curNormalPanel.GetContext(popupNavigation);
                            normalNavigation.Push(curContext);
                        }
                        DestroyAllPopupPanels();
                        RefreshFixedPanels(shownFixedPanels, panel.FixedPanels);
                        curNormalPanel.Hide();
                    }
                    curNormalPanel = panel;
                    panel.Show(context);
                    if (context != null && context.PopupNavigation != null)
                    {
                        ShowPopupPanels(context.PopupNavigation);
                    }
                    return true;
                //打开Popup界面
                case PanelType.PopUp:
                    if (panel.PanelData.PopupMode == PanelPopupMode.HideOther)
                    {
                        foreach (KeyValuePair<PanelID, Panel> item in shownPopupPanels)
                        {
                            item.Value.Hide();
                        }
                        shownPopupPanels.Clear();
                        //压入导航栈,然后关闭
                        if (curPopupPanel != null)
                        {
                            Context curContext = curPopupPanel.GetContext();
                            popupNavigation.Push(curContext);
                            curPopupPanel.Hide();
                            curPopupPanel = null;
                        }
                    }
                    if (panel.PanelData.NavigationMode == PanelNavigationMode.Navigation)
                    {
                        //压入导航栈但不关闭
                        if (curPopupPanel != null)
                        {
                            Context curContext = curPopupPanel.GetContext();
                            popupNavigation.Push(curContext);
                            curPopupPanel = panel;
                        }
                    }
                    panel.Show(context);
                    return true;
                //打开Fixed界面(不常用)
                case PanelType.Fixed:
                    panel.Show(context);
                    shownFixedPanels.Add(panelID, panel);
                    return true;
                //
                default:
                    return false;
            }
        }

        /// <summary>
        /// 关闭界面
        /// </summary>
        /// <param name="panelID">界面ID</param>
        /// <param name="onComplete">回调</param>
        public bool HidePanel(PanelID panelID, Action onComplete = null)
        {
            if (!allPanels.ContainsKey(panelID))
            {
                Log.Warn(String.Format("Failed to hide {0} panel : cannot find panel!", panelID.ToString()));
                return false;
            }

            Panel panel = allPanels[panelID];
            Panel returnPanel = null;
            switch (panel.PanelData.Type)
            {
                //Normal界面
                case PanelType.Normal:
                    DestroyAllPopupPanels();
                    allPanels.Remove(panelID);
                    panel.Destroy();

                    //无导航,返回默认上级Normal界面
                    if (normalNavigation.Count == 0)
                    {
                        returnPanel = BeforeShowPanel(panel.PrePanelID);
                        if (returnPanel == null)
                        {
                            return false;
                        }
                        Assert.AreEqual<PanelType>(PanelType.Normal, returnPanel.PanelData.Type);
                        returnPanel.Show();
                        curNormalPanel = returnPanel;
                    }
                    //返回导航Normal界面
                    else
                    {
                        Context lastContext = normalNavigation.Pop();
                        returnPanel = BeforeShowPanel(lastContext.PanelID);
                        if (returnPanel == null)
                        {
                            return false;
                        }
                        returnPanel.Show(lastContext);
                        curNormalPanel = returnPanel;
                        if (lastContext.PopupNavigation != null)
                        {
                            ShowPopupPanels(lastContext.PopupNavigation);
                        }
                    }
                    RefreshFixedPanels(shownFixedPanels, returnPanel.FixedPanels);
                    return true;
                //Popup界面
                case PanelType.PopUp:
                    if (panel.PanelData.NavigationMode == PanelNavigationMode.Navigation)
                    {
                        if (panel != curPopupPanel)
                        {
                            //只能关闭最近打开的导航Popup界面
                            return false;
                        }
                        panel.Hide();
                        if (shownPopupPanels.Count == 0)
                        {
                            curPopupPanel = null;
                        }
                        else
                        {
                            Context lastContext = popupNavigation.Pop();
                            returnPanel = BeforeShowPanel(lastContext.PanelID);
                            curPopupPanel = returnPanel;
                            returnPanel.Show(lastContext);
                        }
                    }
                    else if (panel.PanelData.NavigationMode == PanelNavigationMode.NoNavigation)
                    {
                        shownPopupPanels.Remove(panelID);
                        panel.Hide();
                    }
                    return true;
                //Fixed界面
                case PanelType.Fixed:
                    shownFixedPanels.Remove(panelID);
                    panel.Hide();
                    return true;
                //
                default:
                    return false;
            }
        }

        /// <summary>
        /// 判断指定界面是否正在显示
        /// </summary>
        public bool IsPanelShown(PanelID panelID)
        {
            if (shownPopupPanels.ContainsKey(panelID) || shownFixedPanels.ContainsKey(panelID))
            {
                return true;
            }
            if (curNormalPanel != null && curNormalPanel.PanelID == panelID)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 销毁所有Popup界面
        /// </summary>
        public void DestroyAllPopupPanels()
        {
            foreach (KeyValuePair<PanelID, Panel> item in shownPopupPanels)
            {
                allPanels.Remove(item.Key);
                shownPopupPanels.Remove(item.Key);
                item.Value.Destroy();
            }
            if (curPopupPanel != null)
            {
                allPanels.Remove(curPopupPanel.PanelID);
                curPopupPanel = null;
                curPopupPanel.Destroy();
            }
        }

        /// <summary>
        /// 销毁所有界面
        /// (重新登陆时会用到)
        /// </summary>
        public void DestroyAllPanels()
        {
            if (allPanels != null)
            {
                foreach (KeyValuePair<PanelID, Panel> panel in allPanels)
                {
                    Panel basePanel = panel.Value;
                    basePanel.Destroy();
                }
                allPanels.Clear();
            }
            /*             shownFixedPanels.Clear();
                        shownPopupPanels.Clear();
                        normalNavigation.Clear();
                        popupNavigation.Clear(); */
            curNormalPanel = null;
            curPopupPanel = null;
        }


        //----------------------------------------Private Methods

        /// <summary>
        /// 初始化数据
        /// </summary>
        private void Init()
        {
            if (normalNavigation == null)
            {
                normalNavigation = new Stack<Context>();
            }
            else
            {
                normalNavigation.Clear();
            }

            if (allPanels == null)
            {
                allPanels = new Dictionary<PanelID, Panel>();
            }
            else
            {
                allPanels.Clear();
            }

            if (shownFixedPanels == null)
            {
                shownFixedPanels = new Dictionary<PanelID, Panel>();
            }
            else
            {
                shownFixedPanels.Clear();
            }

            if (shownPopupPanels == null)
            {
                shownPopupPanels = new Dictionary<PanelID, Panel>();
            }
            else
            {
                shownPopupPanels.Clear();
            }

            canvas = GameObject.Find("Canvas").GetComponent<RectTransform>();

            if (NormalCanvas == null)
            {
                normalCanvas = canvas.Find("NormalCanvas") as RectTransform;
                if (normalCanvas == null)
                {
                    normalCanvas = AddUI(canvas, "NormalCanvas");
                }
                normalCanvas.SetSiblingIndex(0);
            }
            if (FixedCanvas == null)
            {
                fixedCanvas = canvas.Find("FixedCanvas") as RectTransform;
                if (fixedCanvas == null)
                {
                    fixedCanvas = AddUI(canvas, "FixedCanvas");
                }
                fixedCanvas.SetSiblingIndex(1);
            }
            if (PopupCanvas == null)
            {
                popupCanvas = canvas.Find("PopupCanvas") as RectTransform;
                if (popupCanvas == null)
                {
                    popupCanvas = AddUI(canvas, "PopupCanvas");
                }
                popupCanvas.SetSiblingIndex(2);
            }
        }


        /// <summary>
        /// 创建一个UI物件并添加到指定节点
        /// </summary>
        private RectTransform AddUI(RectTransform target, string name)
        {
            GameObject go = new GameObject(name);
            RectTransform rt = go.AddComponent<RectTransform>();
            go.AddComponent<CanvasRenderer>();
            rt.SetParent(target);
            go.layer = LayerMask.NameToLayer("UI");
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.localScale = Vector3.one;
            return rt;
        }

        /// <summary>
        /// (打开Normal界面时)关闭不需要的旧Fixed界面,并打开新的Fixed界面
        /// </summary>
        /// <param name="shownFixedPanels"></param>
        /// <param name="list"></param>
        private void RefreshFixedPanels(Dictionary<PanelID, Panel> shownFixedPanels, List<PanelID> list = null)
        {
            if (list == null)
            {
                foreach (KeyValuePair<PanelID, Panel> item in shownFixedPanels)
                {
                    item.Value.Destroy();
                    shownFixedPanels.Remove(item.Key);
                    allPanels.Remove(item.Key);
                }
                return;
            }

            foreach (KeyValuePair<PanelID, Panel> item in shownFixedPanels)
            {
                if (list.Contains(item.Key))
                {
                    continue;
                }
                else
                {
                    item.Value.Destroy();
                    shownFixedPanels.Remove(item.Key);
                    allPanels.Remove(item.Key);
                }
            }
            foreach (PanelID id in list)
            {
                if (shownFixedPanels.ContainsKey(id))
                {
                    continue;
                }
                else
                {
                    //这不是个好的设计,会给断点调试带来麻烦
                    ShowPanel(id);
                }
            }
        }

        /// <summary>
        /// 显示界面的预处理
        /// </summary>
        /// <param name="panelID"></param>
        /// <returns></returns>
        private Panel BeforeShowPanel(PanelID panelID)
        {
            if (IsPanelShown(panelID))
            {
                Log.Warn(String.Format("{0} panel have been shown!", panelID.ToString()));
                return null;
            }

            Panel basePanel = GetGamePanel(panelID);

            if (basePanel == null)
            {
                Assert.IsTrue(UIConfig.BundleName.ContainsKey(panelID));
                GameObject prefab = ResManager.Instance.Load<GameObject>(
                    UIConfig.BundleName[panelID],
                    panelID.ToString() + UIConfig.Suffix
                    );
                Assert.IsNotNull<GameObject>(prefab);
                GameObject goPanel = GameObject.Instantiate(prefab, normalCanvas);
                goPanel.SetActive(false);
                basePanel = goPanel.GetComponent<Panel>();
                Assert.AreEqual<PanelID>(panelID, basePanel.PanelID);
                RectTransform targetRoot = GetCanvasRoot(basePanel.PanelData.Type);
                goPanel.transform.SetParent(targetRoot);
                goPanel.name = panelID.ToString();
                allPanels[panelID] = basePanel;
            }
            return basePanel;
        }

        private Panel GetGamePanel(PanelID panelID)
        {
            if (allPanels.ContainsKey(panelID))
                return allPanels[panelID];
            else
                return null;
        }

        /// <summary>
        /// 根据上下文加载Popup界面
        /// </summary>
        private void ShowPopupPanels(Stack<Context> popupNavigation)
        {
            Context[] popupArray = popupNavigation.ToArray();
            for (int i = popupArray.Length - 1; i >= 0; i--)
            {
                ShowPanel(popupArray[i].PanelID, popupArray[i]);
            }
        }

        private RectTransform GetCanvasRoot(PanelType type)
        {
            if (type == PanelType.Fixed)
                return FixedCanvas;
            if (type == PanelType.Normal)
                return NormalCanvas;
            if (type == PanelType.PopUp)
                return PopupCanvas;
            return Canvas;
        }

    }
}