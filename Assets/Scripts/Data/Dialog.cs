using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dialog", menuName = "Dialog")]
public class Dialog : ScriptableObject
{
    public string greeting;
    public string reply;
    
    [System.Serializable]
    public class Branch
    {
        public string question;
        public string answer;
        public string reaction;
        public bool visited;
        public InventoryItem objectGained;
        public InventoryItem requiresObject;
        public bool final; // will close the discussion
        public Branch[] branches;
        public Branch parent;
    }

    public Branch[] branches;

}
