using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AudioUtils;

public class Warpable : MonoBehaviour
{
    [SerializeField] SpawnInformation destination;
    [SerializeField] Music warpMusic;
    [SerializeField] Transform transitionCanvas;
    [SerializeField] bool isDoorway;
    [field: SerializeField] public float LimitLeftX { get; private set; }
    [field: SerializeField] public float LimitRightX { get; private set; }

    PlayerController playerWarped;
    Action onWarpCallback;

    public void Warp(PlayerController player, Action callback) {
        playerWarped = player;
        onWarpCallback = callback;
        if (isDoorway) {
            Animator animator = player.GetComponent<Animator>();
            animator.SetTrigger("interact-background");
            AnimatorUtils.Watch(animator, "enzo-interacting-background", StartWarp);
        } else {
            StartWarp();
        }
    }

    private void StartWarp() {
        transitionCanvas.GetComponent<Animator>().SetTrigger("start");
        StartCoroutine(WarpTo(destination, warpMusic));
    }

    IEnumerator WarpTo(SpawnInformation spawn, AudioUtils.Music warpZoneMusic) {
        AudioUtils.PlayMusic(warpZoneMusic);
        yield return new WaitForSeconds(.5f);
        playerWarped.transform.position = spawn.transform.position;
        playerWarped.GetComponent<Movable>().StopMoving();
        playerWarped.GetComponent<Movable>().LimitLeftX = LimitLeftX;
        playerWarped.GetComponent<Movable>().LimitRightX = LimitRightX;
        Camera.main.GetComponent<CameraFollow>().leftBorder = spawn.limitCameraLeft;
        Camera.main.GetComponent<CameraFollow>().rightBorder = spawn.limitCameraRight;
        Camera.main.GetComponent<CameraFollow>().GoToFinalPosition();
        yield return new WaitForSeconds(.5f);
        transitionCanvas.GetComponent<Animator>().SetTrigger("end");
        //SetIdle(); // do we need an intermediary callback here?
        yield return new WaitForSeconds(1f);
        transitionCanvas.GetComponent<Animator>().Play("TransitionIdle");
        onWarpCallback();
    }
}
