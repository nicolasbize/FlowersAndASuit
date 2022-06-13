using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PixelPerfectUtils : MonoBehaviour
{
    // Iterate through all objects in the scene and snap them to the pixel grid
    [MenuItem("Pixel Perfect/Snap Objects")]
    public static void SnapObjects() {
        foreach (SpriteRenderer sr in FindObjectsOfType<SpriteRenderer>()) {
            sr.transform.position = SnapVector(sr.transform.position);
        }
        FindObjectOfType<CameraFollow>().transform.position = FindObjectOfType<PlayerController>().gameObject.transform.position;
    }

    // Return a new vector snapped to the pixel grid
    public static Vector3 SnapVector(Vector3 v) {
        return new Vector3(
            Mathf.Round(v.x * 72) / 72,
            Mathf.Round(v.y * 72) / 72,
            v.z);
    }
}