using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CutScene;

public class CutScenePlayer : MonoBehaviour
{

    [SerializeField] CutScene currentCutscene;
    [SerializeField] float margin = 0.01f;
    [SerializeField] float moveSpeed = 2f;
    [SerializeField] Transform topMovieStrip;
    [SerializeField] Transform bottomMovieStrip;
    [SerializeField] Transform UI;
    Queue<Step> remainingSteps = new Queue<Step>();
    Step currentStep = null;
    GameObject currentCharacter;
    Vector3 currentDestination = Vector3.zero;

    public bool IsPlayingCutScene() {
        return remainingSteps.Count > 0;
    }


    private void Start() {
        if (currentCutscene != null) {
            PlayCutscene(currentCutscene);
        }
    }

    public void PlayCutscene(CutScene scene) {
        remainingSteps.Clear();
        foreach (Step step in scene.steps) {
            remainingSteps.Enqueue(step);
        }
        PlayStep(remainingSteps.Peek());
    }

    private void Update() {
        if (currentStep != null) {
            switch (currentStep.type) {
                case StepType.MoveCharacter:
                    float speed = moveSpeed;
                    if (currentStep.interactionDuration > 0) {
                        speed = currentStep.interactionDuration;
                    }
                    currentCharacter.transform.position = Vector3.MoveTowards(currentCharacter.transform.position, currentStep.targetLocation, speed * Time.deltaTime);
                    if (Mathf.Abs(currentCharacter.transform.position.x - currentStep.targetLocation.x) < margin) {
                        float x = Mathf.Round(currentCharacter.transform.position.x * 72) / 72;
                        float y = Mathf.Round(currentCharacter.transform.position.y * 72) / 72;
                        float z = Mathf.Round(currentCharacter.transform.position.z * 72) / 72;
                        currentCharacter.transform.position = new Vector3(x, y, z);
                        Advance();
                    }
                    break;
                case StepType.CameraPan:
                    Camera.main.transform.position = Vector3.MoveTowards(Camera.main.transform.position, currentStep.targetLocation, moveSpeed * Time.deltaTime);
                    if (Mathf.Abs(Camera.main.transform.position.x - currentStep.targetLocation.x) < margin &&
                        Mathf.Abs(Camera.main.transform.position.y - currentStep.targetLocation.y) < margin) {
                        float x = Mathf.Round(Camera.main.transform.position.x * 72) / 72;
                        float y = Mathf.Round(Camera.main.transform.position.y * 72) / 72;
                        float z = Mathf.Round(Camera.main.transform.position.z * 72) / 72;
                        Camera.main.transform.position = new Vector3(x, y, z);
                        Advance();
                    }
                    break;
                case StepType.WaitForClick:
                    if (Input.GetMouseButtonDown(0)) {
                        Advance();
                    }
                    break;
                case StepType.ActivateUI:
                    UI.gameObject.SetActive(currentStep.animationValue);
                    Advance();
                    break;
            }
        }

    }

    private void Advance() {
        if (currentStep.objectGained != null) {
            GameObject.Find("Enzo").GetComponent<PlayerController>().AddToInventory(currentStep.objectGained);
        }
        if (remainingSteps.Count > 0) {
            remainingSteps.Dequeue();
        }
        if (remainingSteps.Count > 0) {
            PlayStep(remainingSteps.Peek());
        }
    }

    private void PlayStep(Step step) {
        currentStep = step;
        switch (step.type) {
            case StepType.Intro:
                topMovieStrip.GetComponent<Animator>().SetTrigger("EnterMovie");
                bottomMovieStrip.GetComponent<Animator>().SetTrigger("EnterMovie");
                Camera.main.GetComponent<Animator>().SetTrigger("EnterMovie");
                StartCoroutine(WaitFor(step.interactionDuration));
                break;
            case StepType.Outro:
                topMovieStrip.GetComponent<Animator>().SetTrigger("ExitMovie");
                bottomMovieStrip.GetComponent<Animator>().SetTrigger("ExitMovie");
                Camera.main.GetComponent<Animator>().SetTrigger("ExitMovie");
                StartCoroutine(WaitFor(step.interactionDuration));
                break;
            case StepType.MoveCharacter:
                currentCharacter = GameObject.Find(step.character);
                if (currentCharacter.GetComponent<SpriteRenderer>() != null) {
                    currentCharacter.GetComponent<SpriteRenderer>().flipX = step.flipValue;
                }
                break;
            case StepType.AnimateCharacter:
                currentCharacter = GameObject.Find(step.character);
                if (step.animationTrigger != "") {
                    currentCharacter.GetComponent<Animator>().SetTrigger(step.animationTrigger);
                } else {
                    currentCharacter.GetComponent<Animator>().SetBool(step.animationProperty, step.animationValue);
                }
                if (step.text != "") {
                    GetComponent<FloatingTextManager>().AddText(currentCharacter, step.text);
                }
                StartCoroutine(WaitFor(step.interactionDuration));
                break;
            case StepType.CameraPan:
                currentDestination = step.targetLocation;
                break;
            case StepType.Teleport:
                currentCharacter = GameObject.Find(step.character);
                if (step.targetLocation != Vector3.zero) {
                    currentCharacter.transform.position = step.targetLocation;
                }
                if (currentCharacter.GetComponent<SpriteRenderer>() != null) {
                    currentCharacter.GetComponent<SpriteRenderer>().flipX = step.flipValue;
                }
                Advance();
                break;
            case StepType.Destroy:
                currentCharacter = GameObject.Find(step.character);
                Destroy(currentCharacter);
                Advance();
                break;
            case StepType.Create:
                Transform newObject = Instantiate(step.objectCreatedPrefab, GameObject.Find("Entities").transform);
                newObject.position = step.targetLocation;
                Advance();
                break;
            case StepType.RemoveFromInventory:
                GameObject.Find("Enzo").GetComponent<PlayerController>().RemoveFromInventory(step.character);
                Advance();
                break;
            case StepType.Wait:
                currentCharacter = GameObject.Find(step.character);
                if (step.text != "") {
                    GetComponent<FloatingTextManager>().AddText(currentCharacter, step.text);
                }
                StartCoroutine(WaitFor(step.interactionDuration));
                break;

        }
    }

    IEnumerator WaitFor(float duration) {
        yield return new WaitForSeconds(duration);
        if (currentStep?.type == StepType.AnimateCharacter && currentStep.text != "") {
            GetComponent<FloatingTextManager>().AddText(currentCharacter, "");
        }
        Advance();
    }

    IEnumerator PlayAnimation(GameObject character, string animation, float duration) {
        currentCharacter.GetComponent<Animator>().Play(animation);
        yield return new WaitForSeconds(duration);
        Advance();
    }


}
