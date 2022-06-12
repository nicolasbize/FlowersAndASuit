using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New CutScene", menuName = "CutScene")]
public class CutScene : ScriptableObject
{
    public enum StepType { MoveCharacter, CameraPan, AnimateCharacter }

    [System.Serializable]
    public class Step
    {
        public StepType type;
        public string character;
        public Vector3 targetLocation;
        public float interactionDuration; // also add sound event?
        public string animationProperty;
        public bool animationValue;
        public string text;
        
    }

    public Step[] steps;

}