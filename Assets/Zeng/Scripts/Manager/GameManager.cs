using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
public class ModelImportSettingsFixer : EditorWindow
{
    [MenuItem("Tools/Fix Model ReadWrite Settings")]
    static void FixModelSettings()
    {
        string[] modelGuids = AssetDatabase.FindAssets("t:Model");
        
        foreach (string guid in modelGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ModelImporter modelImporter = AssetImporter.GetAtPath(path) as ModelImporter;
            
            if (modelImporter != null && !modelImporter.isReadable)
            {
                modelImporter.isReadable = true;
                modelImporter.SaveAndReimport();
                Debug.Log($"Fixed: {path}");
            }
        }
        
        Debug.Log("All model import settings fixed!");
    }
}
#endif
public class GameManager : UnitySingleTonMono<GameManager>
{
    private int coin;
    public int Coin
    {
        get => coin;
        set => coin = value;
    }
    // 记录每个道具的购买数量字典 , 只有温度计和罗盘开始就有（免费送），需要存档
    public Dictionary<int, int> ItemHasNumDict = new();
    // 判断是否解锁鬼魂字典，需要存档
    public Dictionary<int,bool> GhostLockDict = new ();
    // 所有道具的解锁情况，需要存档
    public Dictionary<int, bool> ItemLockDict = new();
    // 是否是第一次进入玩游戏（进入到抓鬼的游戏场景），需要存档
    [HideInInspector] public bool isFirstPlayGame;
    // 记录每个道具的选择的数量字典
    public Dictionary<int, int> ItemSelectNumDict = new()
    {
        { 1, 0 },
        { 2, 0 },
        { 3, 0 },
        { 4, 0 },
        { 5, 0 },
        { 6, 0 },
        { 7, 0 },
        { 8, 0 },
        { 9, 0 }
    };
    // 当前保存的证据列表，用于定鬼的时候打勾的证据
    public List<EvidenceData> SaveEvidenceList = new ();
    [HideInInspector] public int currentChooseGhostId = 1;
    
    // readonly => 任何代码都可以读取，但任何代码都不能修改（除了构造函数）
    // 加载信息字典
    public Dictionary<int, LoadTipsData> LoadTipsDataDict = new();
    // 道具信息字典
    public Dictionary<int, ItemData> ItemDataDict = new();
    // 日记信息字典
    public Dictionary<int, DiaryData> DiaryDataDict = new();
    // 鬼信息字典
    public Dictionary<int, GhostData> GhostDataDict = new();
    // 证据信息字典
    public Dictionary<int, EvidenceData> EvidenceDataDict = new();
    
    // 存档
    // 存档信息 
    public MainSaveData mainSaveData;
    // 记录当前正在玩的存档路径（ null 表示新游戏 /未加载存档 ）
    [HideInInspector] public string currentArchivePath;

    private void Start()
    {
        UIManager.Instance.openPanel<StartPanel>();
        GameDataInit();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            Time.timeScale = 1;
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            if (GhostDataDict != null)
            {
                GhostLockDict.Clear();
                ItemLockDict.Clear();
                foreach (GhostData ghostData in GhostDataDict.Values)
                {
                    GhostLockDict.Add(ghostData.id, false);
                }

                foreach (ItemData itemData in ItemDataDict.Values)
                {
                    ItemLockDict.Add(itemData.id, false);
                }
            }
        }
        if (!Input.GetKeyDown(KeyCode.M)) return;
        GhostLockDict[4] = !GhostLockDict[4];
        print(GhostLockDict[4]);
        ItemLockDict[4] = !ItemLockDict[4];
        print(ItemLockDict[4]);
        isFirstPlayGame = !isFirstPlayGame;
        print(isFirstPlayGame);
    }

    public void GameDataInit()
    {
        GameLoadTipInit();
        GameItemDataInit();
        GameDiaryDataInit();
        GameGhostDataInit();
        GameEvidenceDataInit();
        
        //受存档影响的数据
        CoinDataInit();
        JumpDataInit();
        ItemLockDataInit();
        GhostLockDataInit();
        ItemHasNumDataInit();
    }

    #region 数据初始化
    
//*********************************************** 音量和按键的读档 ******************************************************\\
    
    // 音量的读档方法在 SoundAudioPool的 public float GlobalSoundVolume 属性里面
    // 按键的读档方法在 KeyBindingManager的 InitDefaultBindings() 方法里面
    // ！音量和按键读档失败，现在在 ArchiveFileItem 的 LoadFileGame（）方法中读档
    
//********************************************************************************************************************\\

    /// <summary>
    /// 初始化玩家金币数据
    /// </summary>
    private void CoinDataInit()
    {
        if (mainSaveData != null)
        {
            Coin = mainSaveData.coin;
            return;
        }
        Coin = 10000;
    }

    /// <summary>
    /// 初始化道具解锁数据
    /// </summary>
    private void ItemLockDataInit()
    {
        if (mainSaveData != null)
        {
            ItemLockDict = new Dictionary<int, bool>(mainSaveData.ItemLockDictData);
            return;
        }
        ItemLockDict = new Dictionary<int, bool>
        {
            { 1, false }, { 2, false }, { 3, false }, { 4, true },
            { 5, true }, { 6, true }, { 7, false }, { 8, false }, { 9, true }
        };
    }

    /// <summary>
    /// 初始化拥有道具数量数据
    /// </summary>
    private void ItemHasNumDataInit()
    {
        if (mainSaveData != null)
        {
            ItemHasNumDict = new Dictionary<int, int>(mainSaveData.ItemHasNumDictData);
            return;
        }
        ItemHasNumDict = new Dictionary<int, int>
        {
            { 1, 1 }, { 2, 1 }, { 3, 0 }, { 4, 0 }, { 5, 0 }, { 6, 0 }, { 7, 0 }, { 8, 0 }, { 9, 0 }
        };
    }
    
    /// <summary>
    /// 初始化鬼魂解锁数据
    /// </summary>
    private void GhostLockDataInit()
    {
        if (mainSaveData != null)
        {
            GhostLockDict = new Dictionary<int, bool>(mainSaveData.GhostLockDictData);
            return;
        }
        // 默认初始化
        GhostLockDict = new Dictionary<int, bool>();
        // 注意：这里必须先确保 GhostDataDict 已经被初始化
        if (GhostDataDict != null)
        {
            foreach (GhostData ghostData in GhostDataDict.Values) { GhostLockDict.Add(ghostData.id, true); }
        }
    }
    
    /// <summary>
    /// 初始化是否有跳过数据
    /// </summary>
    private void JumpDataInit()
    {
        if (mainSaveData != null)
        {
            isFirstPlayGame = mainSaveData.isFirstPlayGameData;
            return;
        }
        isFirstPlayGame = true;
    }
    
    private void GameEvidenceDataInit()
    {
        EvidenceDataDict = new Dictionary<int, EvidenceData>();
        List<EvidenceData> itemDataList = JsonMgr.Instance.LoadData<List<EvidenceData>>("tbevidencedata");
        foreach (EvidenceData item in itemDataList) { EvidenceDataDict.Add(item.id, item); }
    }

    private void GameGhostDataInit()
    {
        GhostDataDict = new Dictionary<int, GhostData>();
        List<GhostData> itemDataList = JsonMgr.Instance.LoadData<List<GhostData>>("tbghostdata");
        foreach (GhostData item in itemDataList) { GhostDataDict.Add(item.id, item); }
    }
    
    private void GameItemDataInit()
    {
        ItemDataDict = new Dictionary<int, ItemData>();
        List<ItemData> itemDataList = JsonMgr.Instance.LoadData<List<ItemData>>("tbitemdata");
        foreach (ItemData item in itemDataList) { ItemDataDict.Add(item.id, item); }
    }

    private void GameLoadTipInit()
    {
        LoadTipsDataDict = new Dictionary<int, LoadTipsData>();
        List<LoadTipsData> loadTipsDataList = JsonMgr.Instance.LoadData<List<LoadTipsData>>("tbloadtips");
        foreach (LoadTipsData item in loadTipsDataList) { LoadTipsDataDict.Add(item.id, item); }
    }
    
    private void GameDiaryDataInit()
    {
        DiaryDataDict = new Dictionary<int, DiaryData>();
        List<DiaryData> diaryDatasList = JsonMgr.Instance.LoadData<List<DiaryData>>("tbdiarydata");
        foreach (DiaryData diary in diaryDatasList) { DiaryDataDict.Add(diary.id, diary); }
    }

    #endregion

    /// <summary>
    /// 保存存档信息
    /// 保存对应游戏存档到archive文件夹下面   Archive/名称/Main.json  
    /// </summary>
    public void SaveArchive(string archiveName = "游戏存档")
    {
        mainSaveData = new MainSaveData
        {
            coin = Coin,
            name = archiveName,
            ItemLockDictData = ItemLockDict,
            GhostLockDictData = GhostLockDict,
            ItemHasNumDictData = ItemHasNumDict,
            isFirstPlayGameData = isFirstPlayGame,
            KeyBindingsDictData = KeyBindingManager.Instance.keyBindingsDict,
            GlobalSoundVolumeData = SoundAudioPool.Instance.GlobalSoundVolume,
            saveRealTicks = DateTimeOffset.Now.ToUnixTimeSeconds() // 存档时的真实系统时间
        };

        if (currentArchivePath != null && File.Exists(currentArchivePath))
        {
            // 存在当前存档路径：直接覆盖原文件
            string json = JsonConvert.SerializeObject(mainSaveData, Formatting.Indented, 
                new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
            File.WriteAllText(currentArchivePath, json); // 覆盖写入
            Debug.Log("已覆盖存档：" + currentArchivePath);
            return;
        }
        
        // 保存的地址
        JsonMgr.Instance.SaveData(mainSaveData, "main", "Archive/EveryGame/" + mainSaveData.name + "/");
    }
    
}
