using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GetStartedButton : MonoBehaviour
{

    [SerializeField] float speed = 2f;
    private bool clicked = false;
    private bool ready = false;
    
    void Start()
    {
        Invoke("Blink", speed);
    }

    private void Update() {
        ready = FMODUnity.RuntimeManager.IsInitialized && FMODUnity.RuntimeManager.HasBankLoaded("Master");
        if (ready) {
            GetComponent<TextMeshPro>().text = "Click to start!";
            if (Input.GetMouseButtonDown(0) && !clicked) {
                clicked = true;
                GetComponent<MeshRenderer>().enabled = false;
            }
        }
    }

    private void Blink() {
        if (ready && !clicked) {
            bool wasDisplayed = GetComponent<MeshRenderer>().enabled;
            GetComponent<MeshRenderer>().enabled = !wasDisplayed;
            float blinkSpeed = wasDisplayed ? 0.3f : speed;
            Invoke("Blink", blinkSpeed);
        }
    }

}
