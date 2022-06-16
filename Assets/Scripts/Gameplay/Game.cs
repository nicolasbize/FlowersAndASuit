using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{

    [SerializeField] bool startNewGame;
    [SerializeField] CutScene startGameCutscene;
    [SerializeField] Transform toolbar;

    private void Start() {
        CutScenePlayer player = GetComponent<CutScenePlayer>();
        foreach (SpriteRenderer sr in FindObjectsOfType<SpriteRenderer>()) {
            sr.transform.position = SnapVector(sr.transform.position);
        }
        FindObjectOfType<CameraFollow>().transform.position = FindObjectOfType<PlayerController>().gameObject.transform.position;
        if (player.currentCutscene != null) {
            player.PlayCutscene(player.currentCutscene);
        } else if (startNewGame) {
            toolbar.GetComponent<UIInventoryManager>().ClearInventory();
            player.PlayCutscene(startGameCutscene);
        }
    }

    public static Vector3 SnapVector(Vector3 v) {
        return new Vector3(
            Mathf.Round(v.x * 72) / 72,
            Mathf.Round(v.y * 72) / 72,
            v.z);
    }

}