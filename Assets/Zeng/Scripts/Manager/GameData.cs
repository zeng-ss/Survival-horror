using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// 道具接口
/// </summary>
public interface I_Item
{
    /// <summary>
    /// 当物品被拾起时调用
    /// </summary>
    void OnPickUp(Transform parentTransform);
    
    /// <summary>
    /// 当物品被丢弃时调用
    /// </summary>
    void OnDrop();
    
    /// <summary>
    /// 当物品被切换到手中时调用
    /// </summary>
    void OnEquip(Transform handTransform);
    
    /// <summary>
    /// 当物品从手中切换出去时调用
    /// </summary>
    void OnUnequip();

    /// <summary>
    /// 书（日记）的查看方法
    /// </summary>
    void OnLookDiary(Transform targetTransform);
    
    /// <summary>
    /// 书（日记）的放回方法
    /// </summary>
    void OnPutBackDiary();

}

/// <summary>
/// 加载消息信息
/// </summary>
public class LoadTipsData
{
    public int id;
    public string Tip;
}

/// <summary>
/// 道具的信息对象
/// </summary>
public class ItemData
{
    public int id;
    public string name;
    public string icon;
    public string des;
    //[JsonConverter(typeof(ArrayToDictionaryConverter<int, int>))]
    public string function;
    public string model;
    public int price;
}


/// <summary>
/// 日记的信息对象
/// </summary>
public class DiaryData
{
    public int id;
    public string diaryContent;
    public string ghostName;
}

/// <summary>
/// 证据信息对象
/// </summary>
public class EvidenceData
{
    public int id;
    public string evidence;
}

public class GhostData
{
    public int id;
    public string des;
    public string name;
    public string icon;
    public List<int> evidenceList;
    public string model;
    public int coin;
}

// 游戏操作枚举（根据游戏需求扩展）
public enum GameAction
{
    None,
    Drop,      // 丢弃道具
    Jump,      // 跳跃
    Attack,    // 攻击
    Crouch,    // 下蹲
    PickUp,    // 捡起
    Interact,  // 交互
    UsingItem, // 使用道具映射
}

// 按键绑定数据
public class KeyBinding
{
    public GameAction action;       // 对应操作
    public KeyCode bindedKey;       // 仅绑定单个按键
    public string actionName;

    // 构造函数（默认绑定单个按键）
    public KeyBinding(GameAction action, KeyCode defaultKeys,string actionName)
    {
        this.action = action;
        bindedKey = defaultKeys;
        this.actionName = actionName;
    }
}


// 存档的数据
public class MainSaveData
{
    public string name; // 存档名字
    public int coin;    // 玩家金币数量数据
    public long saveRealTicks; // 存档时的真实系统时间戳
    public float GlobalSoundVolumeData; // 全局音量
    public bool isFirstPlayGameData;    // 判断是否为第一次玩游戏数据
    public Dictionary<GameAction,KeyBinding> KeyBindingsDictData;
    public Dictionary<int, int> ItemHasNumDictData;
    public Dictionary<int, bool> ItemLockDictData;  // 所有道具的解锁情况
    public Dictionary<int, bool> GhostLockDictData; // 判断是否解锁鬼魂字典
}




