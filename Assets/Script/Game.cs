/*
	Copyright (c) 2017 Tiantian. All rights reserved.
	Description:游戏入口
*/
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityFramework
{
    /// <summary>
    /// 游戏入口
    /// </summary>
    public class Game : MonoBehaviour
    {
        private static GameObject coreObject;

        /// <summary>
        /// 核心GameObject,不会被销毁
        /// (游戏入口及各Manager均挂载于此)
        /// </summary>
        public static GameObject CoreObject
        {
            get
            {
                return coreObject;
            }
        }

        void Awake()
        {
            if (coreObject == null)
            {
                coreObject = this.gameObject;
            }
            DontDestroyOnLoad(this);
            coreObject.AddComponent<Server>();
            coreObject.AddComponent<ServerLogic>();            
            UIConfig.InitControllers();
            AddManagers();
        }

        void Start()
        {
            Server.Instance.StartServer();  
            NetManager.Instance.Connect();
            UIManager.Instance.ShowPanel(PanelID.Login);
        }

        void OnApplicationQuit()
        {

        }

        /// <summary>
        /// 添加各个管理器
        /// </summary>
        private void AddManagers()
        {
            coreObject.AddComponent<UIManager>();
            coreObject.AddComponent<ResManager>();
            coreObject.AddComponent<TableManager>();
            coreObject.AddComponent<EventManager>();
            coreObject.AddComponent<NetManager>();
        }

    }
}
