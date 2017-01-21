using System;
using System.Collections.Generic;

namespace GameKit
{
    public enum EnmEvn
    {
        NONE,
        EVN_ADD_HERO,            //增加英雄事件, data : dgame.Hero
        

        EVN_MAX                      //无实际意义，事件的最大值，用于new数组
    }

    /*
     * simple example
     *
           //define delegate function
            public void OnAddHero(EnmEvn evn, System.Object data);
     
            //regist care event and delegate function
            dispatcher.Regist(EnmEvn.EVN_ADD_HERO, OnAddHero);
            
            //dispatch event
            dgame.Hero hero = new dgame.Hero();
            dispatcher.Dispatch(EnmEvn.EVN_ADD_HERO, hero);
     */

    public class EventDispatcher
    {
        public delegate void DCEvnHandle(EnmEvn evn, System.Object data);

        class EventEntry
        {
            // 事件
            EnmEvn _evn;

            // 事件的观察者列表
            List<DCEvnHandle> _observers = new List<DCEvnHandle>();

            public EventEntry(EnmEvn evn)
            {
                _evn = evn;
            }

            public void Add(DCEvnHandle observer)
            {
                _observers.Add(observer);
            }

            public void Remove(DCEvnHandle observer)
            {
                _observers.Remove(observer);
            }

            public void RemoveByTarget(Object target)
            {
                if (target != null)
                {
                    for (int i = 0; i < _observers.Count;)
                    {
                        if (_observers[i] != null && _observers[i].Target == target)
                        {
                            _observers.RemoveAt(i);
                        }
                        else
                        {
                            i++;
                        }
                    }
                }
            }

            public void Call(EnmEvn evn, System.Object data)
            {
                for (int i = 0; i < _observers.Count; i++)
                {
                    if (_observers[i] != null)
                    {
                        _observers[i](evn, data);
                    }
                }
            }
        }

        Dictionary<EnmEvn, EventEntry> _evnEntries = new Dictionary<EnmEvn, EventEntry>();

        private static EventDispatcher sInstance = null;
        public static EventDispatcher Instance
        {
            get
            {
                if (sInstance == null)
                {
                    sInstance = new EventDispatcher();
                }
                return sInstance;
            }
        }

        //分发事件
        public void Dispatch(EnmEvn evn, System.Object data)
        {
            EventEntry entry = GetEventEntry(evn);
            if (entry != null)
            {
                entry.Call(evn, data);
            }
        }

        //注册关注的事件
        public void Regist(EnmEvn evn, DCEvnHandle handle)
        {
            if (evn >= EnmEvn.EVN_MAX)
            {
                return;
            }

            EventEntry entry = GetEventEntry(evn);
            if (entry == null)
            {
                entry = AddEventEntry(evn);
            }

            entry.Add(handle);
        }

        //取消关注的事件
        public void UnRegist(EnmEvn evn, DCEvnHandle handle)
        {
            if (evn >= EnmEvn.EVN_MAX)
            {
                return;
            }

            EventEntry entry = GetEventEntry(evn);
            if (entry != null)
            {
                entry.Remove(handle);
            }
        }

        //清除所有事件
        public void CleanAllEvn()
        {
            _evnEntries.Clear();
        }

        /// <summary>
        /// 移除所有由对象target所注册的事件监听器。
        /// 通常来说，可在事件监听器所属对象被销毁之前调用该方法。
        /// </summary>
        public void RemoveAllListenersOfTarget(Object target)
        {
            if (target == null)
            {
                return;
            }

            foreach (var item in _evnEntries)
            {
                EventEntry entry = item.Value;
                if (entry != null)
                {
                    entry.RemoveByTarget(target);
                }
            }
        }

        EventEntry GetEventEntry(EnmEvn evn)
        {
            EventEntry entry = null;
            _evnEntries.TryGetValue(evn, out entry);
            return entry;
        }

        EventEntry AddEventEntry(EnmEvn evn)
        {
            EventEntry entry = new EventEntry(evn);
            _evnEntries[evn] = entry;

            return entry;
        }
    }
}
