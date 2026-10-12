using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeController
{
    public long ticks = 654864000;
    public float timer;//计时器，表示当前过了多久，如果过了一个daytime就要重新计时
    public float daytime = 0.02f;//表示一s更新一天
    
    
    /// <summary>
    /// 更新时间
    /// </summary>
    public void UpdateTime()
    {
        timer += Time.deltaTime;
        if (timer >= daytime) //说明过了一秒
        {
            ticks += 86400;
            timer = 0;
        }
    }
}
