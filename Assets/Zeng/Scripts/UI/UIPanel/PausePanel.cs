using System;
using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using UnityEngine.EventSystems; 

// 按键设置项数据结构（用于绑定 UI）
[Serializable]
public class KeyBindingItem
{
    public TMP_InputField keyCodeInput; // 按键修改输入
}

public class PausePanel : BasePanel
{
    // 设置音量和确认面板位置
    private readonly Vector2 originalConfirmPos = new (0, 660);
    private readonly Vector2 targetConfirmPos = new (0, 414);
    
    [Header("返回菜单确认面板相关")]
    #region 返回菜单确认面板相关

    public GameObject confirmPanel;
    public Button confirmBtn;          // 确认返回菜单按钮
    public Button confirmCloseButton;  //关闭确认面板按钮

    #endregion
    
    [Header("音量设置面板相关")]
    #region 音量设置面板相关

    public GameObject audioEscPanel;
    public Slider audioSlider;
    public TMP_InputField audioInput;
    public Button audioEscBtn;
    public Button audioCloseBtn;
    public Sprite noAudioSprite; // 给按钮指定选中态的 Sprite（在Inspector赋值）
    public Sprite hasAudioSprite;
    private const int VolumeDecimalDigits = 2; // 音量精度（保留2位小数，避免输入框显示过多小数）

    #endregion
    
    [Header("主面板相关")]
    #region 主面板相关

    public Button backBtn;    // 返回菜单按钮
    public Button closeBtn;   // 关闭按钮
    public Button audioBtn;   // 音量设置按钮
    public Button keyCodeBtn; // 按键设置按钮
    public GameObject mask;
    private Image backgroundImage;   // 过渡效果面板

    #endregion
    
    [Header("按键设置面板相关")]
    #region 按键设置面板相关

    public GameObject keyCodeEscPanel;    // 按键设置面板
    public Button keyCodeCloseBtn;        // 关闭按键设置面板按钮
    public Button resetKeyBtn;            // 重置默认按键按钮
    // 按键设置项
    public KeyBindingItem useItem;        // 使用道具
    public KeyBindingItem dropItem;       // 丢弃
    public KeyBindingItem jumpItem;       // 跳跃
    public KeyBindingItem crouchItem;     // 下蹲
    public KeyBindingItem pickUpItem;     // 捡起
    public KeyBindingItem attackItem;     // 攻击
    public KeyBindingItem interactItem;   // 交互
    // 编辑状态标记（防止同时编辑多个按键）
    private bool isEditingKey;  
    private GameAction currentEditAction;
    private KeyBindingItem currentEditItem; // 当前编辑的输入框项
    private EventSystem eventSystem;
    // 新增一个标记：忽略点击输入框的鼠标左键
    private bool ignoreNextMouse0;

    #endregion
    
    private void OnEnable()
    {
        eventSystem = EventSystem.current; // 获取事件系统
        // 设置滑块范围（0~1）
        audioSlider.minValue = 0;
        audioSlider.maxValue = 1;
        audioSlider.wholeNumbers = false; // 允许小数
        // 初始化数值为当前全局音量
        ChangeValue(SoundAudioPool.Instance.GlobalSoundVolume);
        mask.SetActive(false);
        confirmPanel.transform.localPosition = originalConfirmPos;
        backgroundImage = GameObject.Find("background").GetComponent<Image>();
        confirmPanelOnclick();
        pausePanelOnclick();
        audioPanelOnclick();
        keyCodePanelOnclick();
    }

    #region 返回菜单面板相关

    private void confirmPanelOnclick()
    {
        SoundAudioPool.Instance.PlaySound(SoundManager.Instance.baseBtnClick, transform.position);
        confirmCloseButton.onClick.AddListener(() =>
        {
            confirmPanel.transform.DOLocalMove(originalConfirmPos, 0.5f);
            mask.SetActive(false);
        });
        confirmBtn.onClick.AddListener(ConfirmBack);
    }
    /// <summary>
    /// 确认返回方法 
    /// </summary>
    private void ConfirmBack()
    {
        SoundAudioPool.Instance.PlaySound(SoundManager.Instance.baseBtnClick, transform.position);
        UIManager.Instance.closePanel<PausePanel>();
        backgroundImage.GetComponent<FaderBackGround>().isNeedCreatText = false;
        backgroundImage.GetComponent<FaderBackGround>().StartFade();
        DOVirtual.DelayedCall(3.5f, () =>
        {
            GameObject.FindGameObjectWithTag("Ghost").GetComponent<GhostController>().ChangeState(Ghost_StateType.Disappear);
            SceneMgr.Instance.LoadSceneAsync("StartScene", () =>
            {
                GameManager.Instance.isFirstPlayGame = false;
                UIManager.Instance.closePanel<PlayerProPanel>();
                UIManager.Instance.closePanel<LoadPanel>();
                UIManager.Instance.openPanel<StartPanel>();
                SoundManager.Instance.audioSource.enabled = true; // 打开背景音乐
                SoundManager.Instance.audioSource.Play();
            });
        });
    }

    #endregion

    #region 主面板相关

    private void pausePanelOnclick()
    {
        backBtn.onClick.AddListener(() =>
        {
            SoundAudioPool.Instance.PlaySound(SoundManager.Instance.menuBtnClick, transform.position);
            confirmPanel.transform.DOLocalMove(targetConfirmPos, 0.5f);
            mask.SetActive(true);
        });
        audioBtn.onClick.AddListener(() =>
        {
            SoundAudioPool.Instance.PlaySound(SoundManager.Instance.menuBtnClick, transform.position);
            audioEscPanel.transform.DOLocalMove(targetConfirmPos, 0.5f);
            mask.SetActive(true);
            ChangeValue(SoundAudioPool.Instance.GlobalSoundVolume);
        });
        keyCodeBtn.onClick.AddListener(() =>
        {
            SoundAudioPool.Instance.PlaySound(SoundManager.Instance.menuBtnClick, transform.position);
            keyCodeEscPanel.transform.DOLocalMove(new Vector2(143, -22), 0.5f);
            InitKeyCodePanel();
            mask.SetActive(true);
            KeyBindingManager.Instance.isEscKey = true;
        });
        closeBtn.onClick.AddListener(() =>
        {
            SoundAudioPool.Instance.PlaySound(SoundManager.Instance.menuBtnClick, transform.position);
            UIManager.Instance.closePanel<PausePanel>();
            EventCenter.Instance.EventTrigger(GameEvent.光标消失);
        });
    }

    #endregion

    #region 音量面板相关

    private void audioPanelOnclick()
    {
        audioCloseBtn.onClick.AddListener(() =>
        {
            SoundAudioPool.Instance.PlaySound(SoundManager.Instance.baseBtnClick, transform.position);
            audioEscPanel.transform.DOLocalMove(originalConfirmPos, 0.5f);
            mask.SetActive(false);
        });
        audioEscBtn.onClick.AddListener(() =>
        {
            SoundAudioPool.Instance.PlaySound(SoundManager.Instance.baseBtnClick, transform.position);
            // 获取按钮当前的SpriteState
            SpriteState spriteState = audioEscBtn.spriteState;
            if (audioEscBtn.spriteState.selectedSprite == noAudioSprite)
            {
                ChangeValue(SoundAudioPool.Instance.previousVolume);
                // 应用先前保存的音量
                SoundAudioPool.Instance.GlobalSoundVolume = SoundAudioPool.Instance.previousVolume;
                // 设置按下的交换的精灵
                spriteState.selectedSprite = hasAudioSprite;
            }
            else if (audioEscBtn.spriteState.selectedSprite == hasAudioSprite)
            {
                // 先保存音量
                SoundAudioPool.Instance.previousVolume = SoundAudioPool.Instance.GlobalSoundVolume;
                SoundAudioPool.Instance.GlobalSoundVolume = 0;
                ChangeValue(0);
                // 设置按下的交换的精灵
                spriteState.selectedSprite = noAudioSprite;
            }
            // 应用修改后的SpriteState
            audioEscBtn.spriteState = spriteState;
        });
        audioSlider.onValueChanged.AddListener((value) =>
        {
            // 限制范围 0到 1
            float clampedValue = Mathf.Clamp01(value);
            SoundAudioPool.Instance.GlobalSoundVolume = clampedValue;
            // 设置输入的值
            // 格式化为指定小数位数，避免显示如 0.5000001 这类冗余小数
            string formattedValue = clampedValue.ToString($"F{VolumeDecimalDigits}", CultureInfo.InvariantCulture);
            audioInput.text = formattedValue;
            if (MusicManager.Instance != null) { MusicManager.Instance.SetMusicVolume(clampedValue); }
        });
        // 监听输入框值变化：输入框改 → 同步滑块 + 更新全局音量
        audioInput.onEndEdit.AddListener(inputText =>
        {
            // 解析输入的字符串为浮点数
            if (float.TryParse(inputText, NumberStyles.Float, CultureInfo.InvariantCulture, out float inputValue))
            {
                // 限制值范围在 0~1
                float clampedValue = Mathf.Clamp01(inputValue);
                // 同步到滑块
                audioSlider.value = clampedValue;
                // 更新全局音量
                SoundAudioPool.Instance.GlobalSoundVolume = clampedValue;
                if (MusicManager.Instance != null) { MusicManager.Instance.SetMusicVolume(clampedValue); }
            }
            else
            {
                // 输入无效（非数字），还原为当前滑块值
                audioSlider.value = audioSlider.value;
            }
        });
        // 输入框失去焦点时强制格式化（避免输入不合法字符）
        audioInput.onDeselect.AddListener(inputText =>
        {
            // 格式化为指定小数位数，避免显示如 0.5000001 这类冗余小数
            string formattedValue = audioSlider.value.ToString($"F{VolumeDecimalDigits}", CultureInfo.InvariantCulture);
            audioInput.text = formattedValue;
        });
    }
    private void ChangeValue(float currentValue)
    {
        if (MusicManager.Instance != null) { MusicManager.Instance.SetMusicVolume(currentValue); }
        audioSlider.value = currentValue;
        // 格式化为指定小数位数，避免显示如 0.5000001 这类冗余小数
        string formattedValue = currentValue.ToString($"F{VolumeDecimalDigits}", CultureInfo.InvariantCulture);
        audioInput.text = formattedValue;
        // 设置全局音量
        SoundAudioPool.Instance.GlobalSoundVolume = currentValue;
    }

    #endregion

    #region 按键设置面板相关
    private void keyCodePanelOnclick()
    {
        keyCodeCloseBtn.onClick.AddListener(() =>
        {
            SoundAudioPool.Instance.PlaySound(SoundManager.Instance.baseBtnClick, transform.position);
            keyCodeEscPanel.transform.DOLocalMove(new Vector2(143, 750), 0.5f);
            CancelAllSelect();
            isEditingKey = false;
            mask.SetActive(false);
            KeyBindingManager.Instance.isEscKey = false;
        });
        // 重置默认按键
        resetKeyBtn.onClick.AddListener(() =>
        {
            SoundAudioPool.Instance.PlaySound(SoundManager.Instance.baseBtnClick, transform.position);
            KeyBindingManager.Instance.ResetBindings();
            InitKeyCodePanel();
        });
        // 绑定每个输入框的事件（点击输入框进入编辑）
        BindKeyInputItem(jumpItem, GameAction.Jump);
        BindKeyInputItem(dropItem, GameAction.Drop);
        BindKeyInputItem(useItem, GameAction.UsingItem);
        BindKeyInputItem(pickUpItem, GameAction.PickUp);
        BindKeyInputItem(attackItem, GameAction.Attack);
        BindKeyInputItem(crouchItem, GameAction.Crouch);
        BindKeyInputItem(interactItem, GameAction.Interact);
    }

    /// <summary>
    /// 绑定单个输入框的编辑事件
    /// </summary>
    private void BindKeyInputItem(KeyBindingItem item, GameAction action)
    {
        // 使用协程延迟处理，避免在onSelect中直接修改interactable
        item.keyCodeInput.onSelect.AddListener(input =>
        {
            StartCoroutine(DelayedStartEdit(item, action));
        });
        // 输入框失去焦点时取消编辑
        item.keyCodeInput.onDeselect.AddListener(input =>
        {
            if (isEditingKey && currentEditAction == action) { CancelKeyEdit(); }
        });
    }

    /// <summary>
    /// 延迟开始编辑
    /// </summary>
    private IEnumerator DelayedStartEdit(KeyBindingItem item, GameAction action)
    {
        // 等待一帧，让 Unity 完成选择操作
        yield return null;
        // 强制取消其他选中（避免冲突）
        CancelAllSelect();
        // 如果已经在编辑其他按键，先取消
        if (isEditingKey && currentEditAction != action) { CancelKeyEdit(); }
        // 进入编辑状态
        if (!isEditingKey)
        {
            isEditingKey = true;
            currentEditAction = action;
            currentEditItem = item;
            // 显示提示文本（让玩家知道可以按按键了）
            item.keyCodeInput.text = "new key";
            // 现在安全地修改 interactable
            item.keyCodeInput.interactable = false;
            // 标记：忽略接下来的鼠标左键（点击输入框的误触）
            ignoreNextMouse0 = true;
        }
    }
    
    /// <summary>
    /// 初始化按键面板显示
    /// </summary>
    private void InitKeyCodePanel()
    {
        UpdateKeyItem(dropItem, GameAction.Drop);
        UpdateKeyItem(jumpItem, GameAction.Jump);
        UpdateKeyItem(useItem, GameAction.UsingItem);
        UpdateKeyItem(pickUpItem, GameAction.PickUp);
        UpdateKeyItem(attackItem, GameAction.Attack);
        UpdateKeyItem(crouchItem, GameAction.Crouch);
        UpdateKeyItem(interactItem, GameAction.Interact);
        isEditingKey = false;
    }

    /// <summary>
    /// 更新单个按键项的显示（输入框）
    /// </summary>
    private void UpdateKeyItem(KeyBindingItem item, GameAction action)
    {
        KeyCode bindedKey = KeyBindingManager.Instance.GetBindedKey(action);
        string keyText = GetFriendlyKeyText(bindedKey);
        item.keyCodeInput.text = keyText;
        // 恢复输入框可交互状态
        item.keyCodeInput.interactable = true;
    }

    /// <summary>
    /// 按键文本友好显示（适配鼠标按键）
    /// </summary>
    private string GetFriendlyKeyText(KeyCode key)
    {
        if (key == KeyCode.None) return "未绑定";
        switch (key)
        {
            case KeyCode.Alpha0: return "0";
            case KeyCode.Alpha1: return "1";
            case KeyCode.Alpha2: return "2";
            case KeyCode.Alpha3: return "3";
            case KeyCode.Alpha4: return "4";
            case KeyCode.Alpha5: return "5";
            case KeyCode.Alpha6: return "6";
            case KeyCode.Alpha7: return "7";
            case KeyCode.Alpha8: return "8";
            case KeyCode.Alpha9: return "9";
            case KeyCode.Space: return "空格";
            case KeyCode.Mouse0: return "左键";
            case KeyCode.Mouse1: return "右键";
            case KeyCode.Mouse2: return "中键";
            case KeyCode.LeftShift: return "左Shift";
            case KeyCode.RightShift: return "右Shift";
            default: return key.ToString();
        }
    }
    
    /// <summary>
    /// 验证按键有效性（允许鼠标/键盘按键，排除无效键）
    /// </summary>
    private bool IsValidKey(KeyCode key)
    {
        // 排除系统按键和无效按键
        KeyCode[] invalidKeys = {
            KeyCode.None,
            KeyCode.Backspace,
            KeyCode.Return,
            KeyCode.KeypadEnter,
            KeyCode.Tab,
            KeyCode.CapsLock,
            KeyCode.LeftApple,
            KeyCode.RightApple,
            KeyCode.LeftWindows,
            KeyCode.RightWindows,
            KeyCode.AltGr,
            KeyCode.Help,
            KeyCode.Print,
            KeyCode.SysReq,
            KeyCode.Break,
            KeyCode.Menu
        };
        return Array.IndexOf(invalidKeys, key) == -1;
    }

    /// <summary>
    /// 取消按键编辑
    /// </summary>
    private void CancelKeyEdit()
    {
        if (!isEditingKey || currentEditItem == null) return;
        // 恢复显示
        UpdateKeyItem(currentEditItem, currentEditAction);
        // 重置状态
        isEditingKey = false;
        currentEditItem = null;
        currentEditAction = GameAction.None;
    }

    /// <summary>
    /// 强制取消所有输入框选中
    /// </summary>
    private void CancelAllSelect()
    {
        if (eventSystem != null) { eventSystem.SetSelectedGameObject(null); }
        ignoreNextMouse0 = false;
        CancelKeyEdit();
    }

    #endregion

    private void Update()
    {
        if (isEditingKey && currentEditItem != null)
        {
            KeyCode pressedKey = KeyCode.None;
            // 遍历所有按键，只记录第一个按下的键（避免重复触发）
            foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(key))
                {
                    pressedKey = key;
                    break; // 只取第一个按下的键，避免遍历全部
                }
            }
            // 没有按键按下，直接返回
            if (pressedKey == KeyCode.None) return;

            // 按 ESC取消编辑
            if (pressedKey == KeyCode.Escape) { CancelAllSelect(); return; }
            // 忽略点击输入框的鼠标左键（避免误触）
            if (ignoreNextMouse0 && pressedKey == KeyCode.Mouse0) { ignoreNextMouse0 = false; return; }

            // 过滤无效按键
            if (IsValidKey(pressedKey))
            {
                // 更新绑定（覆盖原有按键）
                KeyBindingManager.Instance.UpdateBinding(currentEditAction, pressedKey);
                // 刷新显示并退出编辑状态
                CancelKeyEdit();
                // 取消 UI选择
                CancelAllSelect();
                // 重置忽略标记
                ignoreNextMouse0 = false;
            }
        }
    }
    
    void OnDisable()
    {
        // 移除所有监听（防止内存泄漏）
        // 移除主面板相关监听
        backBtn.onClick.RemoveAllListeners();
        audioBtn.onClick.RemoveAllListeners();
        confirmBtn.onClick.RemoveAllListeners();
        closeBtn.onClick.RemoveAllListeners();
        // 移除音量相关监听
        audioCloseBtn.onClick.RemoveAllListeners();
        audioEscBtn.onClick.RemoveAllListeners();
        audioSlider.onValueChanged.RemoveAllListeners();
        audioInput.onEndEdit.RemoveAllListeners();
        audioInput.onDeselect.RemoveAllListeners();
        // 移除按键相关监听
        keyCodeCloseBtn.onClick.RemoveAllListeners();
        keyCodeBtn.onClick.RemoveAllListeners();
        resetKeyBtn.onClick.RemoveAllListeners();
        // 移除按键输入框的监听
        RemoveKeyInputListeners(useItem);
        RemoveKeyInputListeners(jumpItem);
        RemoveKeyInputListeners(dropItem);
        RemoveKeyInputListeners(pickUpItem);
        RemoveKeyInputListeners(attackItem);
        RemoveKeyInputListeners(interactItem);
    }
    /// <summary>
    /// 移除监听方法
    /// </summary>
    /// <param name="item"></param>
    private void RemoveKeyInputListeners(KeyBindingItem item)
    {
        item.keyCodeInput.onSelect.RemoveAllListeners();
        item.keyCodeInput.onDeselect.RemoveAllListeners();
    }
}
