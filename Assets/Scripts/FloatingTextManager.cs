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
    bool isMouthOpen = false;
    GameObject currentTarget = null;
    Queue<Tuple<GameObject, string>> messageQueue = new Queue<Tuple<GameObject, string>>();

    public delegate void OnQueueEmpty();
    public event OnQueueEmpty onEmptyQueue;

    public void Start() {
        Invoke("MoveMouth", 0f);
    }

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
        if (isShowing) {
            currentTarget.GetComponent<Animator>().SetBool("mouth_open", isMouthOpen);
        }
    }

    IEnumerator ShowMessage(GameObject target, string text) {
        textElement.GetComponent<TextMeshPro>().text = text;
        float overrideDistAbove = 0f;
        if (target.GetComponent<Interactive>() != null) {
            overrideDistAbove = target.GetComponent<Interactive>().overrideTextDistanceAboveHead;
        }
        float distAbove = target.GetComponent<SpriteRenderer>().sprite.bounds.extents.y + (overrideDistAbove == 0 ? distanceAboveHead : overrideDistAbove);
        textElement.position = target.transform.position + Vector3.up * distAbove;
        textElement.GetComponent<MeshRenderer>().enabled = true;
        isShowing = true;
        currentTarget = target;

        yield return new WaitForSeconds(speedDisplay);
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
