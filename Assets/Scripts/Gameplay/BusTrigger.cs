using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BusTrigger : MonoBehaviour
{
    [SerializeField] Transform gameplayManager;
    [SerializeField] CutScene finalCutscene;
    private bool entered;

    void Update() {
        List<Collider2D> collidedWith = new List<Collider2D>();
        GetComponent<Collider2D>().OverlapCollider(new ContactFilter2D(), collidedWith);
        collidedWith = collidedWith.FindAll(c => c.gameObject.tag == "Player");
        if (collidedWith.Count == 1 && entered == false) {
            entered = true;
            CheckForBus(collidedWith[0].gameObject);
        } else if (collidedWith.Count == 0 && entered) {
            entered = false;
        }
    }

    private void CheckForBus(GameObject player) {
        if (player.GetComponent<PlayerController>().HasItemInInventory("Flowers") && player.GetComponent<PlayerController>().HasItemInInventory("Fancy Suit")) {
            GameObject.Find("Enzo").GetComponent<PlayerController>().SetIdle();
            gameplayManager.GetComponent<CutScenePlayer>().PlayCutscene(finalCutscene);
            Camera.main.GetComponent<CameraFollow>().leftBorder = -9.65f;
            Camera.main.GetComponent<CameraFollow>().rightBorder = 35.5f;
        }
    }
}
