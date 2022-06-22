using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AudioUtils;
using static CutScene;

public class CutSceneManager : MonoBehaviour
{

    public static CutSceneManager Instance { get; private set; }
    void Awake() {
        if (Instance != null) {
            GameObject.Destroy(Instance);
        } else {
            Instance = this;
        }
        DontDestroyOnLoad(this);
    }

    [field: SerializeField] public CutScene CurrentCutScene { get; private set; }
    [SerializeField] float margin = 0.01f;
    [SerializeField] float moveSpeed = 2f;
    [SerializeField] Transform topMovieStrip;
    [SerializeField] Transform bottomMovieStrip;
    [SerializeField] Transform UI;

    Queue<Step> remainingSteps = new Queue<Step>();
    Step currentStep = null;
    GameObject currentCharacter;

    public bool IsPlayingCutScene() {
        return remainingSteps.Count > 0;
    }

    public void PlayCutscene(CutScene scene) {
        CurrentCutScene = scene;
        remainingSteps.Clear();
        SpriteUtils.RemoveOutlines();
        foreach (Step step in scene.steps) {
            remainingSteps.Enqueue(step);
        }
        PlayStep(remainingSteps.Peek());
    }

    private void Update() {
        if (currentStep != null) {
            switch (currentStep.type) {
                case StepType.CameraPan:
                    Camera.main.transform.position = Vector3.MoveTowards(Camera.main.transform.position, currentStep.targetLocation, moveSpeed * Time.deltaTime);
                    if (Mathf.Abs(Camera.main.transform.position.x - currentStep.targetLocation.x) < margin &&
                        Mathf.Abs(Camera.main.transform.position.y - currentStep.targetLocation.y) < margin) {
                        Camera.main.transform.position = SpriteUtils.PixelAlign(Camera.main.transform.position);
                        Advance();
                    }
                    break;
                case StepType.WaitForClick:
                    if (Input.GetMouseButtonDown(0)) {
                        AudioUtils.PlaySound(AudioUtils.SoundType.UIClick);
                        Advance();
                    }
                    break;
            }
        }

    }

    private void Advance() {
        if (currentStep.objectGained != null) {
            InventoryManager.Instance.AddToInventory(currentStep.objectGained);
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
        currentCharacter = GameObject.Find(step.character);
        // check if we should play a sound / music
        if (currentStep.sound != AudioUtils.SoundType.None) {
            AudioUtils.PlaySound(currentStep.sound);
            StartCoroutine(StopFutureSound(currentStep.sound, currentStep.interactionDuration));
        }
        // check if we should speak
        if (!String.IsNullOrEmpty(currentStep.text)) {
            if (currentStep.fmodTextId < 0) { // we don't have a voice line for this
                SpokenLine line = new SpokenLine(currentStep.text, currentStep.interactionDuration);
                currentCharacter.GetComponent<Speakable>().Speak(line);
            } else {
                // we have a spoken line, we want to use that as our timer unless we are moving to a specific position,
                // where we just wait until that position has been reached
                SpokenLine line = new SpokenLine(currentStep.text, currentStep.fmodTextId, currentStep.interactionDuration);
                if (step.type == StepType.MoveCharacter) {
                    currentCharacter.GetComponent<Speakable>().Speak(line, CurrentCutScene.conversation);
                } else {
                    currentCharacter.GetComponent<Speakable>().Speak(line, CurrentCutScene.conversation, Advance);
                }
            }
            
        }
        switch (step.type) {
            case StepType.Intro:
                topMovieStrip.GetComponent<Animator>().SetTrigger("EnterMovie");
                bottomMovieStrip.GetComponent<Animator>().SetTrigger("EnterMovie");
                Camera.main.GetComponent<Animator>().SetTrigger("EnterMovie");
                if (CurrentCutScene.controlMusic) {
                    AudioUtils.PlayMusic(Music.Dialog, 0.5f);
                }
                StartCoroutine(WaitAndAdvance(1));

                break;
            case StepType.Outro:
                topMovieStrip.GetComponent<Animator>().SetTrigger("ExitMovie");
                bottomMovieStrip.GetComponent<Animator>().SetTrigger("ExitMovie");
                Camera.main.GetComponent<Animator>().SetTrigger("ExitMovie");
                if (CurrentCutScene.controlMusic) {
                    AudioUtils.PlayMusic(Music.Puzzle, 0.5f);
                    StartCoroutine(WaitAndAdvance(2, () => {
                        AudioUtils.PlayMusic(Music.MainTheme, 0.7f);
                    }));
                } else {
                    Advance();
                }
                break;
            case StepType.MoveCharacter:
                Movable movable = GameObject.Find(step.character).GetComponent<Movable>();
                if (movable != null) {
                    if (!String.IsNullOrEmpty(currentStep.animationProperty)) {
                        movable.MoveAnimBoolean = currentStep.animationProperty;
                    }
                    movable.MoveTo(currentStep.targetLocation, () => {
                        movable.GetComponent<SpriteRenderer>().flipX = currentStep.flipValue;
                        Advance();
                    });
                }
                break;
            case StepType.AnimateCharacter:
                if (step.animationTrigger != "") {
                    currentCharacter = GameObject.Find(step.character);
                    currentCharacter.GetComponent<Animator>().SetTrigger(step.animationTrigger);
                    // check if we have a vocal line, otherwise use interactionDuration
                    if (currentStep.fmodTextId <= 0) {
                        StartCoroutine(WaitAndAdvance(step.interactionDuration));
                    }
                }
                break;
            case StepType.Teleport:
                currentCharacter = GameObject.Find(step.character);
                if (step.targetLocation != Vector3.zero) {
                    currentCharacter.transform.position = step.targetLocation;
                }
                if (currentCharacter.GetComponent<Movable>() != null) {
                    currentCharacter.GetComponent<Movable>().StopMoving();
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
            case StepType.ShowRenderer:
                currentCharacter = GameObject.Find(step.character);
                if (currentCharacter.GetComponent<SpriteRenderer>() != null) {
                    currentCharacter.GetComponent<SpriteRenderer>().enabled = currentStep.animationValue;
                } else if (currentCharacter.GetComponent<MeshRenderer>() != null) {
                    currentCharacter.GetComponent<MeshRenderer>().enabled = currentStep.animationValue;
                }
                Advance();
                break;
            case StepType.Create:
                Transform newObject = Instantiate(step.objectCreatedPrefab, GameObject.Find("Entities").transform);
                newObject.position = step.targetLocation;
                Advance();
                break;
            case StepType.RemoveFromInventory:
                InventoryManager.Instance.RemoveFromInventory(step.character);
                Advance();
                break;
            case StepType.PlaySound:
                if (currentStep.sound != AudioUtils.SoundType.None) {
                    AudioUtils.PlaySound(currentStep.sound);
                    StartCoroutine(StopFutureSound(currentStep.sound, currentStep.interactionDuration));
                    Advance();
                } else {
                    AudioUtils.PlayDialog(CurrentCutScene.conversation, currentStep.fmodTextId, () => { Advance(); });
                }
                break;
            case StepType.StopSound:
                if (currentStep.sound != AudioUtils.SoundType.None) {
                    AudioUtils.StopSound(currentStep.sound);
                } else {
                    AudioUtils.StopDialog(CurrentCutScene.conversation, currentStep.fmodTextId);
                }
                Advance();
                break;
            case StepType.Wait:
                // if we have a voice line, we'll use that as the timer
                if (currentStep.fmodTextId <= 0) {
                    StartCoroutine(WaitAndAdvance(step.interactionDuration));
                }
                break;
            case StepType.ActivateUI:
                UI.gameObject.SetActive(currentStep.animationValue);
                Advance();
                break;
            case StepType.PlayMusic:
                AudioUtils.PlayMusic((AudioUtils.Music)currentStep.interactionDuration, 0.5f);
                Debug.Log("playing music " + currentStep.interactionDuration);
                Advance();
                break;

        }
    }

    IEnumerator WaitAndAdvance(float duration, Action callback = null) {
        yield return new WaitForSeconds(duration);
        if (currentStep?.type == StepType.AnimateCharacter && currentStep.text != "") {
            GetComponent<FloatingTextManager>().AddText(currentCharacter, "");
        }
        if (callback != null) {
            callback();
        }
        Advance();
    }

    IEnumerator StopFutureSound(SoundType sound, float duration) {
        yield return new WaitForSeconds(duration);
        AudioUtils.StopSound(sound);
    }

    IEnumerator PlayAnimation(GameObject character, string animation, float duration) {
        currentCharacter.GetComponent<Animator>().Play(animation);
        yield return new WaitForSeconds(duration);
        Advance();
    }


}
