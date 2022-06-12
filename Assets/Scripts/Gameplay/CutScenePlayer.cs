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
            case StepType.MoveCharacter:
                currentCharacter = GameObject.Find(step.character);
                break;
            case StepType.AnimateCharacter:
                currentCharacter = GameObject.Find(step.character);
                currentCharacter.GetComponent<Animator>().SetBool(step.animationProperty, step.animationValue);
                StartCoroutine(WaitFor(step.interactionDuration));
                break;
            case StepType.CameraPan:
                currentDestination = step.targetLocation;
                break;
        }
    }

    IEnumerator WaitFor(float duration) {
        yield return new WaitForSeconds(duration);
        Advance();
    }

    IEnumerator PlayAnimation(GameObject character, string animation, float duration) {
        currentCharacter.GetComponent<Animator>().Play(animation);
        yield return new WaitForSeconds(duration);
        Advance();
    }


}
