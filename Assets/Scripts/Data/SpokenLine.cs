using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SpokenLine
{
    [field: SerializeField] public string Text { get; private set; }
    [field: SerializeField] public int FmodId { get; private set; }

    public SpokenLine() { }
    public SpokenLine(string text, int id) {
        Text = text;
        FmodId = id;
    }
}
