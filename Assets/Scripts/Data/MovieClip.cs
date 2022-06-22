using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AudioUtils;

[CreateAssetMenu(fileName = "New Movie", menuName = "Movie")]
[Serializable]
public class MovieClip : ScriptableObject
{
    [Serializable]
    public class IntroStep
    {
        public Music music;
    }
    [Serializable]
    public class AnimateCharacter
    {
        public GameObject character;
        public string trigger;
    }

    [Serializable]
    public class MoveCharacter
    {
        public GameObject character;
        public Vector3 destination;
        public float speed;
    }
    [Serializable]
    public class TalkCharacter
    {
        public GameObject character;
        public string text;
        public int fmodId;
    }

    [Serializable]
    public class Clip
    {
        public IntroStep intro;
        public AnimateCharacter[] animations;
        public MoveCharacter[] moves;
        public TalkCharacter[] dialogs;
    }

    public AudioUtils.DialogConversation audioFile;
    public Clip[] clips;

}
