using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteUtils : MonoBehaviour
{

    public static SpriteUtils spriteUtils;
    private void Awake() {
        if (spriteUtils != null) {
            GameObject.Destroy(spriteUtils);
        } else {
            spriteUtils = this;
        }
        DontDestroyOnLoad(this);
    }

    public static void AddOutline(GameObject target) {
        // this requires a sprite renderer to work properly
        SpriteRenderer spriteRenderer = target.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            return;

        float thickness = 0f;
        Interactive interactive = target.GetComponent<Interactive>();
        if (interactive != null) {
            thickness = interactive.OutlineThickness;
        }
        // if we haven't overriden the thickness, try our best to find the right size
        if (thickness == 0) { // TODO: fix this, right now I can't properly get the right outline on spritesheets
            thickness = 1f / (18 * spriteRenderer.bounds.size.x);
        }
        spriteRenderer.material.SetFloat("_Thickness", thickness);
        spriteRenderer.material.SetColor("_OutlineColor", new Color(230, 230, 230, 1));
    }

    public static void RemoveOutline (GameObject target) {
        // this requires a sprite renderer to work properly
        SpriteRenderer spriteRenderer = target.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            return;

        spriteRenderer.material.SetFloat("_Thickness", 0f);
        spriteRenderer.material.SetColor("_OutlineColor", new Color(0, 0, 0, 1));
    }

    // camera is zoomed at 5. Total camera height is 5*2 = 10 for a height of 720 px, aka 72 PPU
    public static Vector3 PixelAlign(Vector3 position) {
        float x = Mathf.Round(position.x * 72) / 72;
        float y = Mathf.Round(position.y * 72) / 72;
        float z = Mathf.Round(position.z * 72) / 72;
        return new Vector3(x, y, z);
    }

    public static void RemoveOutlines() {
        foreach (SpriteRenderer sr in FindObjectsOfType<SpriteRenderer>()) {
            sr.material.SetFloat("_Thickness", 0f);
            sr.material.SetColor("_OutlineColor", new Color(0, 0, 0, 1));
        }
    }

}
