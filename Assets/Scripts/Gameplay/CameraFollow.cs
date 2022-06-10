using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{

    [SerializeField] Transform target;
    [SerializeField] float smoothness = 0.12f;
    [SerializeField] float yOffset = 0.8f;
    [SerializeField] float leftBorder = -16.5f;
    [SerializeField] float rightBorder = 16.5f;

    private void FixedUpdate() {
        Vector3 finalPosition = target.position + new Vector3(0, yOffset, 0);
        if (finalPosition.x < leftBorder) {
            finalPosition = new Vector3(leftBorder, finalPosition.y, finalPosition.z);
        } else if (finalPosition.x > rightBorder) {
            finalPosition = new Vector3(rightBorder, finalPosition.y, finalPosition.z);
        }
        transform.position = Vector3.Lerp(transform.position, finalPosition, smoothness * Time.deltaTime);
    }


}
