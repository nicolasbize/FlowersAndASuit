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
    private bool readyToStart = false;
    private bool started = false;
    private int attempted = 0;

    private void Start() {
        Cursor.SetCursor(cursorTexture, Vector2.one * 32, CursorMode.Auto);
        foreach (SpriteRenderer sr in FindObjectsOfType<SpriteRenderer>()) {
            sr.transform.position = SnapVector(sr.transform.position);
        }
        if (startNewGame) {
            Camera.main.transform.position = new Vector3(-52.4900017f, 20, 0);
        } else {
            readyToStart = true;
            Camera.main.transform.position = FindObjectOfType<PlayerController>().gameObject.transform.position;
            CutScenePlayer player = GetComponent<CutScenePlayer>();
            if (player.currentCutscene != null) {
                player.PlayCutscene(player.currentCutscene);
            }
        }

    }

    public bool IsBusy() {
        return !readyToStart || GetComponent<CutScenePlayer>().IsPlayingCutScene();
    }

    private void Update() {
        CutScenePlayer player = GetComponent<CutScenePlayer>();
        if (startNewGame) {
            if (!readyToStart) {
                readyToStart = FMODUnity.RuntimeManager.IsInitialized && FMODUnity.RuntimeManager.HasBankLoaded("Master");
            } else if (!started) {
                started = true;
                toolbar.GetComponent<UIInventoryManager>().ClearInventory();
                player.PlayCutscene(startGameCutscene);
                Invoke("TryPlayMusic", 1);
            }
        }
    }

    private void TryPlayMusic() {
        AudioUtils.PlayMusic(Music.IntroCredits);
        attempted += 1;
        if (attempted < 10) {
            Invoke("TryPlayMusic", 1);
        }
    }

    public static Vector3 SnapVector(Vector3 v) {
        return new Vector3(
            Mathf.Round(v.x * 72) / 72,
            Mathf.Round(v.y * 72) / 72,
            v.z);
    }

}
