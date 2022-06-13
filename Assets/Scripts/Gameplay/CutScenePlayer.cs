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
    Queue<Step> remainingSteps = new Queue<Step>();
    Step currentStep = null;
    GameObject currentCharacter;
    Vector3 currentDestination;

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
                    currentCharacter.transform.position = Vector3.MoveTowards(currentCharacter.transform.position, currentStep.targetLocation, moveSpeed * Time.deltaTime);
                    if (Mathf.Abs(currentCharacter.transform.position.x - currentStep.targetLocation.x) < margin) {
                        float x = Mathf.Round(currentCharacter.transform.position.x);
                        float y = Mathf.Round(currentCharacter.transform.position.y);
                        float z = Mathf.Round(currentCharacter.transform.position.z);
                        currentCharacter.transform.position = new Vector3(x, y, z);
                        Advance();
                    }
                    break;
                case StepType.CameraPan:
                    Camera.main.transform.position = Vector3.MoveTowards(Camera.main.transform.position, currentStep.targetLocation, moveSpeed * Time.deltaTime);
                    if (Mathf.Abs(Camera.main.transform.position.x - currentStep.targetLocation.x) < margin) {
                        float x = Mathf.Round(Camera.main.transform.position.x);
                        float y = Mathf.Round(Camera.main.transform.position.y);
                        float z = Mathf.Round(Camera.main.transform.position.z);
                        Camera.main.transform.position = new Vector3(x, y, z);
                        Advance();
                    }
                    break;
            }
        }

    }

    private void Advance() {
        remainingSteps.Dequeue();
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
                break;
            case StepType.AnimateCharacter:
                currentCharacter = GameObject.Find(step.character);
                currentCharacter.GetComponent<Animator>().SetBool(step.animationProperty, step.animationValue);
                if (step.text != "") {
                    GetComponent<FloatingTextManager>().AddText(currentCharacter, step.text);
                }
                StartCoroutine(WaitFor(step.interactionDuration));
                break;
            case StepType.CameraPan:
                currentDestination = step.targetLocation;
                break;
            case StepType.Wait:
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
