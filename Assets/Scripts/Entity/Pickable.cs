using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AnimatorUtils;

public class Pickable : MonoBehaviour
{
    [field: SerializeField] public InventoryItem Item { get; private set; }
    [field: SerializeField] public bool OnGround { get; private set; }
    [field: SerializeField] public SpokenLine TextBeforePickup { get; private set; }
    [field: SerializeField] public bool DestroyAfterPickup { get; private set; }
    [SerializeField] private UIInventoryManager inventory;

    Animator animator;
    Action callbackAfterPicked;

    public void PickUp(PlayerController player, Action callback) {
        callbackAfterPicked = callback;
        animator = player.GetComponent<Animator>();
        if (OnGround) {
            animator.SetTrigger("crouch-start");
            AnimatorUtils.Watch(animator, "enzo-pickup-crouch", AddItemToInventory);
        } else {
            animator.SetTrigger("interact-background");
            AnimatorUtils.Watch(animator, "enzo-interacting-background", AddItemToInventory);
        }
    }

    private void AddItemToInventory() {
        inventory.AddToInventory(Item);
        if (OnGround) {
            animator.SetTrigger("crouch-end");
            AnimatorUtils.Watch(animator, "enzo-pickup-rise", CompletePickup);
        } else {
            CompletePickup();
        }
    }

    private void CompletePickup() {
        if (DestroyAfterPickup) {
            GameObject.Destroy(gameObject);
        }
        callbackAfterPicked();
    }
}
