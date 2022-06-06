using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine;
using static Dialog;

public class DialogContainer : MonoBehaviour, IPointerClickHandler
{
    //bool isShowing = false;
    //bool isHiding = false;
    //bool shown = false;
    RectTransform rect = null;
    [SerializeField] float speedAnimation = 10f;
    [SerializeField] Transform dialogOptionPrefab;
    [SerializeField] Color newTextColor;
    [SerializeField] Color visitedColor;
    [SerializeField] string backText = "(Back)";

    Action<string> onClickCallback = null;
    Branch[] currentBranches = null;
    Dialog currentDialog = null;

    enum State { Appearing, Hiding, Hidden, Displayed }
    State state = State.Hidden;

    private void Start() {
        rect = GetComponent<RectTransform>();
    }

    public void Activate(Dialog dialog, Branch[] branches, Action<string> callback) {
        if (state != State.Displayed && state != State.Appearing) {
            currentBranches = branches;
            currentDialog = dialog;
            state = State.Appearing;
            ClearContents();
            AddContents(branches);
            onClickCallback = callback;
        }
    }

    private void AddContents(Branch[] branches) {
        bool hasFinalOption = false;
        foreach(var branch in branches) {
            var option = Instantiate(dialogOptionPrefab, transform);
            option.GetComponent<RectTransform>().localScale = Vector3.one;
            option.GetComponent<TextMeshProUGUI>().text = branch.question;
            option.GetComponent<TextMeshProUGUI>().color = branch.visited ? visitedColor : newTextColor;
            if (branch.final) {
                hasFinalOption = true;
            }
        }
        if (!hasFinalOption) { // add a (Back) option
            var option = Instantiate(dialogOptionPrefab, transform);
            option.GetComponent<RectTransform>().localScale = Vector3.one;
            option.GetComponent<TextMeshProUGUI>().text = backText;
            option.GetComponent<TextMeshProUGUI>().color = visitedColor;
        }
    }

    private void ClearContents() {
        foreach (Transform child in transform) {
            GameObject.Destroy(child.gameObject);
        }
    }

    private void Update() {
        float targetY = -200 + transform.childCount * 50;
        if (state == State.Appearing) {
            rect.anchoredPosition = Vector3.Lerp(rect.anchoredPosition, Vector2.zero, Time.deltaTime * speedAnimation);
            if (rect.anchoredPosition.y > targetY - 0.01f) {
                state = State.Displayed;
            }
        } else if (state == State.Hiding) {
            rect.anchoredPosition = Vector3.Lerp(rect.anchoredPosition, new Vector2(0, -201), Time.deltaTime * speedAnimation * 2);
            if (rect.anchoredPosition.y < -200) {
                state = State.Hidden;
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData) {
        if (state == State.Displayed) {
            var objectClicked = eventData.pointerCurrentRaycast.gameObject;
            var optionSelected = objectClicked.GetComponent<TextMeshProUGUI>().text;
            if (optionSelected == backText) {

                ClearContents();
                if (currentBranches[0].parent.parent != null) {
                    currentBranches = currentBranches[0].parent.parent.branches;
                } else {
                    currentBranches = currentDialog.branches;
                }
                AddContents(currentBranches);
                state = State.Appearing; // reset size
            } else {
                state = State.Hiding;
                onClickCallback(optionSelected);
            }
        }
    }

}
