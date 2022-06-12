using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New CutScene", menuName = "CutScene")]
public class CutScene : ScriptableObject
{
    public enum StepType { MoveCharacter, BackgroundInteract, CameraPan, AnimateCharacter }

    [System.Serializable]
    public class Step
    {
        public StepType type;
        public Transform character;
        public Transform targetLocation;
        public float interactionDuration; // also add sound event?
        public string animationName;
        public string text;
        
    }

    public Step[] steps;

}
