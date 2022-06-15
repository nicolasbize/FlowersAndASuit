using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static Dialog;

public class FloatingTextManager : MonoBehaviour
{
    [SerializeField] float speedDisplay = 4f;
    [SerializeField] float distanceAboveHead = 3f;
    public Transform textElement;
    bool isShowing = false;
    bool isMouthOpen = false;
    GameObject currentTarget = null;
    Queue<SingleDialogText> messageQueue = new Queue<SingleDialogText>();

    public delegate void OnQueueEmpty();
    public event OnQueueEmpty onEmptyQueue;

    public void Start() {
        Invoke("MoveMouth", 0f);
    }

    public void AddText(GameObject target, string text, AudioUtils.DialogConversation fmodEvent = AudioUtils.DialogConversation.None, int fmodId = 0) {
        string[] phrases = text.Split(new[] { "   " }, StringSplitOptions.None);
        for (int i=0; i<phrases.Length; i++) {
            if (phrases[i].Trim().Length > 0) {
                SingleDialogText sentence = new SingleDialogText();
                sentence.speaker = target;
                sentence.text = phrases[i];
                if (i == 0) { // even though we break down long sentences into several displayed strings, we only have a single audio event
                    sentence.fmodEvent = fmodEvent;
                    sentence.fmodId = fmodId;
                }
                messageQueue.Enqueue(sentence);
            }
        }
    }

    public bool HasEnquedMessagesForOtherThan(GameObject target) {
        return new List<SingleDialogText>(messageQueue.ToArray()).Find(t => t.speaker.name != target.name) != null;
    }

    public bool HasMessagesInQueue() {
        return messageQueue.Count > 0;
    }

    public int NbMessagesInQueue() {
        return messageQueue.Count;
    }

    public void RemoveMessagesFor(GameObject target) {
        Queue<SingleDialogText> newQueue = new Queue<SingleDialogText>();
        while (messageQueue.Count > 0) {
            SingleDialogText message = messageQueue.Dequeue();
            if (message.speaker.name != target.name) {
                newQueue.Enqueue(message);
            }
        }
        messageQueue = newQueue;
    }


    private void Update() {
        if (!isShowing && messageQueue.Count > 0) {
            StartCoroutine(ShowMessage(messageQueue.Dequeue()));
        }
        // FIXME: this forces every animator to have a mouth_open property, even animated objects in the game.
        if (isShowing) {
            currentTarget.GetComponent<Animator>().SetBool("mouth_open", isMouthOpen);
        }
    }

    IEnumerator ShowMessage(SingleDialogText message) {
        textElement.GetComponent<TextMeshPro>().text = message.text;
        float overrideDistAbove = 0f;
        if (message.speaker.GetComponent<Interactive>() != null) {
            overrideDistAbove = message.speaker.GetComponent<Interactive>().overrideTextDistanceAboveHead;
        }
        float distAbove = message.speaker.GetComponent<SpriteRenderer>().sprite.bounds.extents.y + (overrideDistAbove == 0 ? distanceAboveHead : overrideDistAbove);
        textElement.position = message.speaker.transform.position + Vector3.up * distAbove;
        textElement.GetComponent<MeshRenderer>().enabled = true;
        isShowing = true;
        currentTarget = message.speaker;
        if (message.fmodEvent != AudioUtils.DialogConversation.None) {
            AudioUtils.PlaySound(message.fmodEvent, Camera.main.transform.position, message.fmodId);
        }
        float waitTime = Mathf.Min(3, message.text.Split(' ').Length / 1.5f);
        yield return new WaitForSeconds(waitTime);
        isShowing = false;
        textElement.GetComponent<MeshRenderer>().enabled = false;
        if (messageQueue.Count == 0 && onEmptyQueue != null) {
            onEmptyQueue();
            currentTarget = null;
        }
    }

    void MoveMouth() {
        isMouthOpen = !isMouthOpen;
        float randomTime = UnityEngine.Random.Range(0.1f, 0.3f);
        Invoke("MoveMouth", randomTime);
    }

}
