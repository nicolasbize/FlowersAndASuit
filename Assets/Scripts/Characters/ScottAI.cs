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
    [SerializeField] public bool drugsPlanted = false;


    private void Start() {
        originalPosition = transform.position;
    }

    public void PlantDrugs() {
        // maybe wait for a few seconds before?
        drugsPlanted = true;
        destination = originalPosition;
    }

    public bool IsDrugsPlanted() {
        return drugsPlanted;
    }

    public bool IsPlanted() {
        return drugsPlanted;
    }

    public bool IsOnPhone() {
        return GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("scott-idle-phone");
    }

    // Update is called once per frame
    void Update()
    {
        bool isOnPhone = IsOnPhone();
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
                float x = Mathf.Round(transform.position.x * 72) / 72;
                float y = Mathf.Round(transform.position.y * 72) / 72;
                float z = Mathf.Round(transform.position.z * 72) / 72;
                transform.position = new Vector3(x, y, z);
                destination = Vector3.zero;
            }
        }
    }
}
