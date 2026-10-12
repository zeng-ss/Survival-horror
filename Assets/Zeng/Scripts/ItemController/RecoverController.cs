using TMPro;
using UnityEngine;

public class RecoverController : ItemController
{ 
    private TMP_Text text;
    public enum RecoverType
    {
        RecoverSanity,
        RecoverBody
    }

    public RecoverType recoverType = RecoverType.RecoverSanity;
    
    private new void Start()
    {
        base.Start();
        text = GetComponentInChildren<TMP_Text>();
        text.gameObject.SetActive(false);
    }
    private new void Update()
    {
        base.Update();
        if (player == null || player.cameraOne == null || ghost == null)
        {
            return;
        }
        if (transform.parent == player.cameraOne.transform)
        {
            text.gameObject.SetActive(true);
        }
        else
        {
            text.gameObject.SetActive(false);
        }
        if (Input.GetKeyDown(KeyCode.R) && transform.parent == player.cameraOne.transform)
        {
            switch (recoverType)
            {
                case RecoverType.RecoverSanity:
                    player.CurrentSanity += 30;
                    player.CurrentSanity = Mathf.Min(100, player.CurrentSanity);
                    // 使用过后处理
                    player.DropItem();
                    tag = "Untagged";
                    gameObject.transform.GetChild(0).gameObject.SetActive(false);
                    break;
                case RecoverType.RecoverBody:
                    player.CurrentBodyPower += 40;
                    player.CurrentBodyPower = Mathf.Min(100, player.CurrentBodyPower);
                    // 使用过后处理
                    player.DropItem();
                    tag = "Untagged";
                    gameObject.transform.GetChild(0).gameObject.SetActive(false);
                    break;
            }
        }
    }
}
