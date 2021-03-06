using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AudioUtils;

[CreateAssetMenu(fileName = "New CutScene", menuName = "CutScene")]
public class CutScene : ScriptableObject
{
    public enum StepType { Intro, MoveCharacter, CameraPan, AnimateCharacter, Wait, Outro, Teleport, Destroy, Create, RemoveFromInventory, WaitForClick, ActivateUI, PlayMusic, PlaySound, StopSound, ShowRenderer }

    [System.Serializable]
    public class Step
    {
        public StepType type;
        public string character;
        public Vector3 targetLocation;
        public float interactionDuration; // also add sound event?
        public string animationProperty;
        public bool animationValue;
        public string animationTrigger;
        public bool flipValue;
        public Transform objectCreatedPrefab;
        public string text;
        public InventoryItem objectGained;
        public SoundType sound;
        public int fmodTextId;
    }

    public AudioUtils.DialogConversation conversation;
    public bool controlMusic = true;
    public Step[] steps;

}
