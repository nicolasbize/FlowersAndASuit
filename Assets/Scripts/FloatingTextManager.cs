using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FloatingTextManager : MonoBehaviour
{
    [SerializeField] float speedDisplay = 4f;
    [SerializeField] float distanceAboveHead = 3f;
    public Transform textElement;
    bool isShowing = false;
    Queue<Tuple<GameObject, string>> messageQueue = new Queue<Tuple<GameObject, string>>();

    public delegate void OnQueueEmpty();
    public event OnQueueEmpty onEmptyQueue;

    public void AddText(GameObject target, string text) {
        string[] phrases = text.Split(new[] { "   " }, StringSplitOptions.None);
        foreach (string phrase in phrases) {
            messageQueue.Enqueue(new Tuple<GameObject, string>(target, phrase));
        }
    }

    private void Update() {
        if (!isShowing && messageQueue.Count > 0) {
            Tuple<GameObject, string> message = messageQueue.Dequeue();
            StartCoroutine(ShowMessage(message.Item1, message.Item2));
        }
    }

    IEnumerator ShowMessage(GameObject target, string text) {
        textElement.GetComponent<TextMeshPro>().text = text;
        float distAbove = target.GetComponent<SpriteRenderer>().sprite.bounds.extents.y + distanceAboveHead;
        textElement.position = target.transform.position + Vector3.up * distAbove;
        textElement.GetComponent<MeshRenderer>().enabled = true;
        isShowing = true;
        yield return new WaitForSeconds(speedDisplay);
        isShowing = false;
        textElement.GetComponent<MeshRenderer>().enabled = false;
        if (messageQueue.Count == 0 && onEmptyQueue != null) {
            onEmptyQueue();
        }
    }

}
