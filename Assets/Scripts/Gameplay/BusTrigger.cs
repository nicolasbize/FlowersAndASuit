using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BusTrigger : MonoBehaviour
{
    public enum TriggerType { BusStop, Ending }

    [SerializeField] CutScene finalCutscene;
    [SerializeField] CutScene endCutscene;
    [SerializeField] TriggerType type;
    private bool entered;

    void Update() {
        List<Collider2D> collidedWith = new List<Collider2D>();
        GetComponent<Collider2D>().OverlapCollider(new ContactFilter2D(), collidedWith);
        collidedWith = collidedWith.FindAll(c => c.gameObject.tag == "Player");
        if (collidedWith.Count == 1 && entered == false) {
            entered = true;
            if (type == TriggerType.BusStop) {
                CheckForBus(collidedWith[0].gameObject);
            } else {
                CutSceneManager.Instance.PlayCutscene(endCutscene);
            }
        } else if (collidedWith.Count == 0 && entered) {
            entered = false;
        }
    }

    private void CheckForBus(GameObject player) {
        if (InventoryManager.Instance.HasItemInInventory("Flowers") && InventoryManager.Instance.HasItemInInventory("Fancy Suit")) {
            GameObject.Find("Enzo").GetComponent<PlayerController>().SetIdle();
            CutSceneManager.Instance.PlayCutscene(finalCutscene);
            InventoryManager.Instance.ClearInventory();
            Camera.main.GetComponent<CameraFollow>().leftBorder = -9.65f;
            Camera.main.GetComponent<CameraFollow>().rightBorder = 35.5f;
        }
    }
}
