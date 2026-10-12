using UnityEngine;

public class SwitchController : MonoBehaviour
{
    private CircuitbreakerController circuitBreaker; // 拿到电闸引用
    private bool isOpen;
    [Header("这个开关控制的灯泡")]
    public LightController[] lights;
    private PlayerController player;
    private BoxCollider boxCollider;

    void Start()
    {
        circuitBreaker = GameObject.Find("Circuit breaker").GetComponent<CircuitbreakerController>();
        if (!gameObject.GetComponent<Collider>())
        {
            gameObject.AddComponent<BoxCollider>();
            gameObject.GetComponent<BoxCollider>().isTrigger = true;
            gameObject.GetComponent<BoxCollider>().size = new Vector3(0.14f,0.15f,0.02f);
            gameObject.GetComponent<BoxCollider>().center = new Vector3(0,0,0.02f);
        }
        boxCollider = GetComponent<BoxCollider>();
        player = GameObject.Find("Player").GetComponent<PlayerController>();
        Light[] allLights = FindObjectsOfType<Light>();
        foreach (Light light in allLights) { light.enabled = false; }
    }
    

    /// <summary>
    /// 关灯的方法
    /// </summary>
    private void OnCloseSwitch()
    {
        if(!isOpen) return;
        isOpen = false;
        SoundAudioPool.Instance.PlaySound(SoundManager.Instance.switchClip,transform.position);
        foreach (LightController currentLight in lights) { currentLight.CloseLight(); }
    }

    /// <summary>
    /// 开灯的方法
    /// </summary>
    private void OnOpenSwitch()
    {
        if (isOpen) { return; }
        isOpen = true;
        SoundAudioPool.Instance.PlaySound(SoundManager.Instance.switchClip,transform.position);
        foreach (LightController currentLight in lights) { currentLight.OpenLight(); }
    }

    private void HandleOpenAndCloseSwitch()
    {
        // 获取屏幕中心位置
        Vector3 screenCenter = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
        // 从屏幕中心发射射线
        Ray ray = player.currentCamera.ScreenPointToRay(screenCenter);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit,1.5f))
        {
            if (hit.collider.gameObject == gameObject && KeyBindingManager.Instance.GetActionDown(GameAction.Interact))
            {
                if (!isOpen) { OnOpenSwitch(); }
                else { OnCloseSwitch(); }
            }
        }
    }

    void Update()
    {
        if (!circuitBreaker.isOpen) { boxCollider.enabled = false; }
        else
        {
            boxCollider.enabled = true;
            HandleOpenAndCloseSwitch();
        }
    }
    
}
