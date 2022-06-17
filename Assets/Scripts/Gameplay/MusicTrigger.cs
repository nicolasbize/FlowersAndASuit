using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AudioUtils;

public class MusicTrigger : MonoBehaviour
{
    [SerializeField] SoundType soundPlayed;
    private bool entered;

    void Update()
    {
        List<Collider2D> collidedWith = new List<Collider2D>();
        GetComponent<Collider2D>().OverlapCollider(new ContactFilter2D(), collidedWith);
        collidedWith = collidedWith.FindAll(c => c.gameObject.tag == "Player");
        if (collidedWith.Count > 0 && entered == false) {
            entered = true;
            PlaySound();
        } else if (collidedWith.Count == 0 && entered) {
            entered = false;
        }
    }

    private void PlaySound() {
        if (soundPlayed == SoundType.CityOutdoor || soundPlayed == SoundType.ParkOutdoor) {
            AudioUtils.PlayAtmospheric(soundPlayed, Camera.main.transform.position);
        }
    }

}
