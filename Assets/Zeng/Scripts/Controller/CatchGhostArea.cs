using UnityEngine;

public class CatchGhostArea : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ghost"))
        {
            TipPanel tipPanel = UIManager.Instance.openPanel<TipPanel>();
            tipPanel.transform.localPosition = new Vector2(0, 430);
            tipPanel.ShowTip("任务成功，出口已出现，请前往逃离");
            var player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
            player.exitDoor.gameObject.SetActive(true);
            player.guideLine.endTarget = player.exitDoor.transform;
            player.guideLine.isStart = true;
            Destroy(gameObject);
            other.gameObject.GetComponent<GhostController>().ChangeState(Ghost_StateType.Appear);
        }
    }
}
