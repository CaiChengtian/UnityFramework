/*
	Copyright (c) 2017 Tiantian. All rights reserved.
	Description:
*/
using UnityEngine;
using UnityEngine.Assertions;
using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityFramework
{
    /// <summary>
    /// 事件管理器
    /// </summary>
    public class EventManager : Manager<EventManager>
    {
        private Dictionary<int, Delegate> events;

        void Awake()
        {
            events = new Dictionary<int, Delegate>();
        }

        /// <summary>
        /// 触发
        /// </summary>
        /// <returns></returns>
        public void Trigger(EventID eventID)
        {
            Delegate targetAction;
            if (events.TryGetValue((int)eventID, out targetAction))
            {
                Assert.IsNotNull(targetAction);
                Delegate[] list = targetAction.GetInvocationList();
                for (int i = 0; i < list.Length; i++)
                {
                    if (list[i].GetType() != typeof(Action))
                    {
                        Log.Warn(String.Format("Unexpected event type : {0}", eventID.ToString()));
                        continue;
                    }
                    Action action = (Action)list[i];
                    action();
                }
            }
        }

        public void Trigger<T>(EventID eventID, T t)
        {
            Delegate targetAction;
            if (events.TryGetValue((int)eventID, out targetAction))
            {
                Assert.IsNotNull(targetAction);
                Delegate[] list = targetAction.GetInvocationList();
                for (int i = 0; i < list.Length; i++)
                {
                    if (list[i].GetType() != typeof(Action<T>))
                    {
                        Log.Warn(String.Format("Unexpected event type : {0}", eventID.ToString()));
                        continue;
                    }
                    Action<T> action = (Action<T>)list[i];
                    action(t);
                }
            }
        }

        public void Trigger<T, U>(EventID eventID, T t, U u)
        {
            Delegate targetAction;
            if (events.TryGetValue((int)eventID, out targetAction))
            {
                Assert.IsNotNull(targetAction);
                Delegate[] list = targetAction.GetInvocationList();
                for (int i = 0; i < list.Length; i++)
                {
                    if (list[i].GetType() != typeof(Action<T, U>))
                    {
                        Log.Warn(String.Format("Unexpected event type : {0}", eventID.ToString()));
                        continue;
                    }
                    Action<T, U> action = (Action<T, U>)list[i];
                    action(t, u);
                }
            }
        }

        public void Trigger<T, U, V>(EventID eventID, T t, U u, V v)
        {
            Delegate targetAction;
            if (events.TryGetValue((int)eventID, out targetAction))
            {
                Assert.IsNotNull(targetAction);
                Delegate[] list = targetAction.GetInvocationList();
                for (int i = 0; i < list.Length; i++)
                {
                    if (list[i].GetType() != typeof(Action<T, U, V>))
                    {
                        Log.Warn(String.Format("Unexpected event type : {0}", eventID.ToString()));
                        continue;
                    }
                    Action<T, U, V> action = (Action<T, U, V>)list[i];
                    action(t, u, v);
                }
            }
        }


        /// <summary>
        /// 订阅
        /// </summary>
        public void AddListener(EventID eventID, Action listener)
        {
            CheckAdd(eventID, listener);
            events[(int)eventID] = (Action)Delegate.Combine((Action)events[(int)eventID], listener);
        }

        public void AddListener<T>(EventID eventID, Action<T> listener)
        {
            CheckAdd(eventID, listener);
            events[(int)eventID] = (Action<T>)Delegate.Combine((Action<T>)events[(int)eventID], listener);
        }

        public void AddListener<T, U>(EventID eventID, Action<T, U> listener)
        {
            CheckAdd(eventID, listener);
            events[(int)eventID] = (Action<T, U>)Delegate.Combine((Action<T, U>)events[(int)eventID], listener);
        }

        public void AddListener<T, U, V>(EventID eventID, Action<T, U, V> listener)
        {
            CheckAdd(eventID, listener);
            events[(int)eventID] = (Action<T, U, V>)Delegate.Combine((Action<T, U, V>)events[(int)eventID], listener);
        }

        /// <summary>
        /// 取消订阅
        /// </summary>
        public void RemoveListener(EventID eventID, Action listener)
        {
            if (CheckRemove(eventID, listener))
            {
                events[(int)eventID] = (Action)Delegate.Remove((Action)events[(int)eventID], listener);
            }
        }

        public void RemoveListener<T>(EventID eventID, Action<T> listener)
        {
            if (CheckRemove(eventID, listener))
            {
                events[(int)eventID] = (Action<T>)Delegate.Remove((Action<T>)events[(int)eventID], listener);
            }
        }

        public void RemoveListener<T, U>(EventID eventID, Action<T, U> listener)
        {
            if (CheckRemove(eventID, listener))
            {
                events[(int)eventID] = (Action<T, U>)Delegate.Remove((Action<T, U>)events[(int)eventID], listener);
            }
        }

        public void RemoveListener<T, U, K>(EventID eventID, Action<T, U, K> listener)
        {
            if (CheckRemove(eventID, listener))
            {
                events[(int)eventID] = (Action<T, U, K>)Delegate.Remove((Action<T, U, K>)events[(int)eventID], listener);
            }
        }

        /// <summary>
        /// 清除所有事件
        /// </summary>
        public void ClearAllEvents()
        {
            events.Clear();
        }

        private bool CheckAdd(EventID eventID, Delegate del)
        {
            if (!events.ContainsKey((int)eventID))
            {
                events.Add((int)eventID, null);
                return true;
            }
            return CheckType(eventID, del);
        }

        private bool CheckRemove(EventID eventID, Delegate del)
        {
            if (!events.ContainsKey((int)eventID))
            {
                Log.Warn(String.Format("Fialed to remove event listener {0}:Target listener is non-existent.", eventID.ToString()));
                return false;
            }
            return CheckType(eventID, del);
        }

        private bool CheckType(EventID eventID, Delegate del)
        {
            Delegate d = events[(int)eventID];
            if (d != null && d.GetType() != del.GetType())
            {
                Log.Error(
                    String.Format("Event type doesnot match : {0}", eventID.ToString())
                );
                return false;
            }
            else
            {
                return true;
            }
        }

    }
}