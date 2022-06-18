using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BusTrigger : MonoBehaviour
{
    public enum TriggerType { BusStop, Ending }

    [SerializeField] Transform gameplayManager;
    [SerializeField] Transform inventory;
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
                gameplayManager.GetComponent<CutScenePlayer>().PlayCutscene(endCutscene);
            }
        } else if (collidedWith.Count == 0 && entered) {
            entered = false;
        }
    }

    private void CheckForBus(GameObject player) {
        if (player.GetComponent<PlayerController>().HasItemInInventory("Flowers") && player.GetComponent<PlayerController>().HasItemInInventory("Fancy Suit")) {
            GameObject.Find("Enzo").GetComponent<PlayerController>().SetIdle();
            gameplayManager.GetComponent<CutScenePlayer>().PlayCutscene(finalCutscene);
            inventory.GetComponent<UIInventoryManager>().ClearInventory();
            Camera.main.GetComponent<CameraFollow>().leftBorder = -9.65f;
            Camera.main.GetComponent<CameraFollow>().rightBorder = 35.5f;
        }
    }
}
