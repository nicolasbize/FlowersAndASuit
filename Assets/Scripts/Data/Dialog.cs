using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dialog", menuName = "Dialog")]
public class Dialog : ScriptableObject
{
    public string greeting;
    public string reply;
    public AudioUtils.DialogConversation fmodEvent;

    [System.Serializable]
    public class Branch
    {
        public string question;
        public string answer;
        public string reaction;
        public bool visited;
        public bool requiresPlantedDrugs;
        public InventoryItem objectGained;
        public InventoryItem requiresObject;
        public CutScene cutscene;
        public bool final; // will close the discussion
        public int fmodQuestionId = -1;
        public int fmodAnswerId = -1;
        public int fmodReactionId = -1;
        public Branch[] branches;
        public Branch parent;
    }

    public class SingleDialogText
    {
        public AudioUtils.DialogConversation fmodEvent;
        public Speakable speaker;
        public string text;
        public int fmodId;
    }

    public Branch[] branches;

}
