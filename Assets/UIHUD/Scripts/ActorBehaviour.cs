/********************************************
-	    File Name: ActorBehaviour
-	  Description: 
-	 	   Author: lijing,<979477187@qq.com>
-     Create Date: Created by lijing on #CREATIONDATE#.
-Revision History: 
********************************************/

using UnityEngine;
using System.Collections;
using System;
using GameKit;

public class ActorBehaviour : MonoBehaviour 
{

    public Action<ActorBehaviour, ActorBehaviour> OnHitEvent;   // 受击事件
    public Action<ActorBehaviour, ActorBehaviour> OnCritEvent;  // 暴击事件
    public Action<ActorBehaviour> OnDeadEvent;                  // 死亡事件
    public Action<long, long> OnHPChangeEvent;                  // 更新血量时间
    public Action<uint> OnPlaySkill;                            // 释放技能

	// Use this for initialization
	void Start () 
    {
        TestHUD();
	}

    void TestHUD()
    {
        //准备数据
        StructUIHUD data = new StructUIHUD();
        data.ActorName = "骷髅弓箭手";
        data.FollowObject = gameObject;
        data.CurHP = 10;
        data.MaxHP = 10;
     
        UIHUD uihud = UIHUD.Create(data, transform);
        //监听事件
        OnHPChangeEvent += uihud.OnEventHPChange;
        OnDeadEvent += uihud.OnEventActorDie;
        OnPlaySkill += uihud.OnEventActorPlaySkill;
    }
	
}
