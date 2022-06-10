using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory")]
public class InventoryItem : ScriptableObject
{
    public string itemName;
    public Texture2D texture;

    [System.Serializable]
    public class Interaction
    {
        public string objectName;
        public string playerThought;
    }

    public Interaction[] interactions;

}