using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.PlayerLoop;

/// <summary>
/// 池子数据 
/// </summary>
public class PoolData //小池子 
{
    //池子中的父容器   
    public GameObject fatherObj;
    //池子中放置容器的列表 
    public List<GameObject> poolList ;

    public PoolData(GameObject obj,GameObject grandFatherObj)
    {
        fatherObj = new GameObject(obj.name);
        fatherObj.transform.parent = grandFatherObj.transform;  
        poolList = new List<GameObject>();
        //添加新对象到池子容器列表中 并添加到fatherObj对象下面
        PushObj(obj);
    }
/// <summary>
/// 往池子中的放东西  游戏对象用完了
/// </summary>
/// <param name="obj"></param>
    public void PushObj(GameObject obj)
    {
        obj.SetActive(false);
        poolList.Add(obj);  
        obj.transform.parent = fatherObj.transform;
    }
/// <summary>
/// 从池子中拿对象 
/// </summary>
/// <returns></returns>
    public GameObject PopObj()
    {
        GameObject obj=poolList[0];
        poolList.RemoveAt(0);
        obj.transform.parent =null;
        obj.SetActive(true);
        return obj;
    }
}

/// <summary>
/// 缓存池管理器
/// </summary>
public class PoolMgr : SingleTon<PoolMgr>
{
   //缓存池容器 
   Dictionary<string, PoolData> poolDic = new Dictionary<string, PoolData>();
   private GameObject grandFatherObj;

   public GameObject getObj(string name)
   {
       //判断字典中是否有该对象对应的池子 并且池子列表长度大于0 这时候才可以从池子列表中获取池子对象
       if (poolDic.ContainsKey(name)&&poolDic[name].poolList.Count > 0)
       {
           return poolDic[name].PopObj();  
       }
       else
       {
           //如果池子中没有对象则自己从资源中加载生成一个对象 
           GameObject obj=ResMgr.Instance.load<GameObject>(name);
           if (obj == null) obj = new GameObject();
           obj.name = name;
           return obj;
       }
   }
/// <summary>
/// 将不用的对象还给池子  
/// </summary>
/// <param name="name"></param>
/// <param name="obj"></param>
   public void pushObj(string name, GameObject obj)
   {
       
       if(grandFatherObj==null)
           grandFatherObj=new GameObject("Pool");
       //判断是否有池子 
       if (poolDic.ContainsKey(name))
       {
           poolDic[name].PushObj(obj);  
       }
       else
       {
           poolDic.Add(name,new PoolData(obj,grandFatherObj));
       }
   }

/// <summary>
/// 清空池子容器
/// </summary>
   public void clear()
   {
       poolDic.Clear(); 
       grandFatherObj = null;   
   }
   
}
