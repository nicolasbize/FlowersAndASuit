using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogContainer : MonoBehaviour
{
    bool isShowing = false;
    bool isHiding = false;
    bool shown = false;
    RectTransform rect = null;
    [SerializeField] float speedAnimation = 10f;
    [SerializeField] Transform dialogOptionPrefab;

    private void Start() {
        rect = GetComponent<RectTransform>();
    }

    public void Activate(Dialog dialog) {
        if (!shown && !isShowing) {
            isShowing = true;
            ClearContents();
            AddContents(dialog);
        }
    }

    private void AddContents(Dialog dialog) {
        foreach(var branch in dialog.branches) {
            var option = Instantiate(dialogOptionPrefab, transform);
            option.GetComponent<RectTransform>().localScale = Vector3.one;
            option.GetComponent<TextMeshProUGUI>().text = branch.question;
        }
    }

    private void ClearContents() {
        foreach (Transform child in transform) {
            GameObject.Destroy(child.gameObject);
        }
    }

    private void Update() {
        if (isShowing && rect.anchoredPosition.y < 0) {
            rect.anchoredPosition = Vector3.Lerp(rect.anchoredPosition, Vector2.zero, Time.deltaTime * speedAnimation);
            if (rect.anchoredPosition.y > -0.05) {
                shown = true;
                isShowing = false;
            }
        }
    }


}
