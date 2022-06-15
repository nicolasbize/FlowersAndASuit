using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetStartedButton : MonoBehaviour
{

    [SerializeField] float speed = 2f;
    private bool enabled = true;
    
    void Start()
    {
        Invoke("Blink", speed);
    }

    private void Update() {
        if (Input.GetMouseButtonDown(0)) {
            enabled = false;
            GetComponent<MeshRenderer>().enabled = false;
        }
    }

    private void Blink() {
        if (enabled) {
            bool wasDisplayed = GetComponent<MeshRenderer>().enabled;
            GetComponent<MeshRenderer>().enabled = !wasDisplayed;
            float blinkSpeed = wasDisplayed ? 0.3f : speed;
            Invoke("Blink", blinkSpeed);
        }
    }

}
