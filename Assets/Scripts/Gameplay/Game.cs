using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AudioUtils;

public class Game : MonoBehaviour
{

    [SerializeField] bool startNewGame;
    [SerializeField] CutScene startGameCutscene;
    [SerializeField] Transform toolbar;
    [SerializeField] Texture2D cursorTexture;

    private void Start() {
        Cursor.SetCursor(cursorTexture, Vector2.one * 32, CursorMode.Auto);
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
        AudioUtils.PlayMusic(Music.IntroCredits, Camera.main.transform.position);
    }

    public static Vector3 SnapVector(Vector3 v) {
        return new Vector3(
            Mathf.Round(v.x * 72) / 72,
            Mathf.Round(v.y * 72) / 72,
            v.z);
    }

}
