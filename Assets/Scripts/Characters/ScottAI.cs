using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScottAI : MonoBehaviour
{

    [SerializeField] public Transform gamelogicManager;
    [SerializeField] public float speed = 3f;
    [SerializeField] public float margin = 0.1f;
    private Vector3 originalPosition;
    private Vector3 destination = Vector3.zero;
    private bool drugsPlanted = false;


    private void Start() {
        originalPosition = transform.position;
    }

    public bool CanPlantDrugs() {
        return GetComponent<Animator>().GetBool("on_phone");
    }

    public void PlantDrugs() {
        // maybe wait for a few seconds before?
        drugsPlanted = true;
        GetComponent<Animator>().SetBool("on_phone", false);
        destination = originalPosition;
    }

    public bool IsPlanted() {
        return drugsPlanted;
    }

    // Update is called once per frame
    void Update()
    {
        bool isOnPhone = GetComponent<Animator>().GetBool("on_phone");
        GetComponent<Interactive>().busy = isOnPhone;
        if (isOnPhone) {
            bool playerStartedOtherInteraction = gamelogicManager.GetComponent<FloatingTextManager>().HasEnquedMessagesForOtherThan(gameObject);
            // check if he should get off the phone, for ex if the player starts a new interaction
            if (playerStartedOtherInteraction) {
                // stop showing what he's saying, dedicate the text to the player's new interactions
                gamelogicManager.GetComponent<FloatingTextManager>().RemoveMessagesFor(gameObject);
            }
        }
        if (destination != Vector3.zero) {
            transform.position = Vector3.MoveTowards(transform.position, destination, speed * Time.deltaTime);
            if (Mathf.Abs(transform.position.x - destination.x) < margin) {
                float x = Mathf.Round(transform.position.x);
                float y = Mathf.Round(transform.position.y);
                float z = Mathf.Round(transform.position.z);
                transform.position = new Vector3(x, y, z);
                destination = Vector3.zero;
            }
        }
    }
}
