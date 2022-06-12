using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{

    [SerializeField] Transform target;
    [SerializeField] float smoothness = 0.12f;
    [SerializeField] float yOffset = 0.8f;
    public float leftBorder = -16.5f;
    public float rightBorder = 16.5f;

    private void FixedUpdate() {
        transform.position = Vector3.Lerp(transform.position, GetFinalPosition(), smoothness * Time.deltaTime);
    }

    public void GoToFinalPosition() {
        transform.position = GetFinalPosition();
    }

    private Vector3 GetFinalPosition() {
        Vector3 finalPosition = target.position + new Vector3(0, yOffset, 0);
        if (finalPosition.x < leftBorder) {
            finalPosition = new Vector3(leftBorder, finalPosition.y, finalPosition.z);
        } else if (finalPosition.x > rightBorder) {
            finalPosition = new Vector3(rightBorder, finalPosition.y, finalPosition.z);
        }
        return finalPosition;
    }


}
