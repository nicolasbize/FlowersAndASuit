using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static AudioUtils;

public class Speakable : MonoBehaviour
{

    [SerializeField] private float distAboveHead;
    [SerializeField] Transform textPrefab;
    [SerializeField] bool facePlayer;
    [SerializeField] PlayerController player;
    [field: SerializeField] public bool Busy { get; set; }
    [field: SerializeField] public Dialog Dialog { get; set; }
    private Transform textMesh;
    private Animator animator;

    private void Start() {
        animator = GetComponent<Animator>();
    }

    public bool IsSpeaking() {
        return textMesh != null;
    }

    public void Speak(SpokenLine line) {
        // if fmod id < 0, we don't have a vocal line
        Speak(line, line.FmodId < 0 ? DialogConversation.None : Dialog.fmodEvent);
    }

    public void Speak(SpokenLine line, Action onDoneSpeaking) {
        Speak(line, line.FmodId < 0 ? DialogConversation.None : Dialog.fmodEvent, onDoneSpeaking);
    }

    public void Speak(SpokenLine voiceLine, DialogConversation audioClip = DialogConversation.None, Action onDoneSpeaking = null) {
        textMesh = Instantiate(textPrefab, transform);
        float distAbove = GetComponent<SpriteRenderer>().sprite.bounds.extents.y * 2 + distAboveHead;
        textMesh.position = transform.position + Vector3.up * distAbove;
        textMesh.GetComponent<TextMeshPro>().text = voiceLine.Text;
        Action onCompleteSound = () => {
            Destroy(textMesh.gameObject);
            textMesh = null;
            if (onDoneSpeaking != null) onDoneSpeaking();
        };

        if (voiceLine.FmodId < 0) { // we don't have the voice line, just display the text for some time
            StartCoroutine(WaitForAndExecute(voiceLine.ManualDuration, onCompleteSound));
        }  else {
            DialogConversation clip = audioClip != DialogConversation.None ? audioClip : Dialog.fmodEvent;
            if (clip == DialogConversation.None) {
                Debug.Log("Speakable trying to speak but doesn't have a clip to check");
                return;
            }
            if (voiceLine.ManualDuration > 0) {
                AudioUtils.PlayDialog(clip, voiceLine.FmodId);
                StartCoroutine(WaitForAndExecute(voiceLine.ManualDuration, () => {
                    AudioUtils.StopDialog(clip, voiceLine.FmodId);
                    onCompleteSound();
                }));
            } else {
                AudioUtils.PlayDialog(clip, voiceLine.FmodId, onCompleteSound);
            }
        }
    }


    IEnumerator WaitForAndExecute(float duration, Action callback) {
        yield return new WaitForSeconds(duration);
        callback();
    }

    private void Update() {
        animator.SetBool("is-speaking", IsSpeaking());
        Movable movable = GetComponent<Movable>();
        bool moving = movable == null ? true : movable.IsMoving();
        if (facePlayer && !moving && !CutSceneManager.Instance.IsPlayingCutScene()) {
            GetComponent<SpriteRenderer>().flipX = player.transform.position.x < transform.position.x;
        }
    }

}
