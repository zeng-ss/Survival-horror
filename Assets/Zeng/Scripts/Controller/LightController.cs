using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LightController : MonoBehaviour
{
    private new Light light; // light组件
    void Start()
    {
        if (!GetComponent<Light>()) { light = GetComponentInChildren<Light>(); }
        if (GetComponent<Light>()) { light = GetComponent<Light>(); }
    }
    
   /// <summary>
    /// 开灯 
    /// </summary>
    public void OpenLight()
    {
        light.enabled = true;
    }
   
    /// <summary>
    /// 关灯 
    /// </summary>
    public void CloseLight()
    {
        light.enabled = false;
    }
    
}
