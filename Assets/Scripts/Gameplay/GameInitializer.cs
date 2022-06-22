using UnityEngine;
using static AudioUtils;

public class GameInitializer : MonoBehaviour
{
    public static GameInitializer Instance { get; private set; }
    void Awake() {
        if (Instance != null) {
            GameObject.Destroy(Instance);
        } else {
            Instance = this;
        }
        DontDestroyOnLoad(this);
    }

    [SerializeField] bool startNewGame;
    [SerializeField] CutScene startGameCutscene;
    [SerializeField] Texture2D cursorTexture;
    public bool IsReady { get; private set; }
    private bool assetsLoaded = false;
    private int attempted = 0;

    private void Start() {
        Cursor.SetCursor(cursorTexture, Vector2.one * 32, CursorMode.Auto);
        foreach (SpriteRenderer sr in FindObjectsOfType<SpriteRenderer>()) {
            sr.transform.position = SnapVector(sr.transform.position);
        }
        if (startNewGame) {
            Camera.main.transform.position = new Vector3(-52.4900017f, 20, 0);
        } else {
            assetsLoaded = true;
            Camera.main.transform.position = FindObjectOfType<PlayerController>().gameObject.transform.position;
        }

    }


    private void Update() {
        if (!assetsLoaded) {
            assetsLoaded = FMODUnity.RuntimeManager.IsInitialized && FMODUnity.RuntimeManager.HasBankLoaded("Master");
        } else if (!IsReady) {
            // assets loaded, either start a new game or play test cutscene
            IsReady = true;
            if (startNewGame) {
                InventoryManager.Instance.ClearInventory();
                CutSceneManager.Instance.PlayCutscene(startGameCutscene);
                // sometimes the web version doesn't play on start for some reason
                // try every second for the first 10 secs
                Invoke("TryPlayMusic", 1);
            } else if (CutSceneManager.Instance.CurrentCutScene != null) {
                CutSceneManager.Instance.PlayCutscene(CutSceneManager.Instance.CurrentCutScene);
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
