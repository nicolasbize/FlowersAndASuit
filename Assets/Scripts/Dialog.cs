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
        public bool visited;
        public string objectGained;
        public Branch[] branches;
    }

    public Branch[] branches;

}
