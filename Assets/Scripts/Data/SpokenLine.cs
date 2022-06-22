using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SpokenLine
{
    [field: SerializeField] public string Text { get; private set; }
    [field: SerializeField] public int FmodId { get; private set; }
    [field: SerializeField] public float ManualDuration { get; private set; }

    public SpokenLine() { }
    // an actual voice line
    public SpokenLine(string text, int id) {
        Text = text;
        FmodId = id;
    }
    // a voice line we want to interrupt after some time
    public SpokenLine(string text, int id, float duration) {
        Text = text;
        FmodId = id;
        ManualDuration = duration;
    }
    // a written line where there is no voice acting yet
    public SpokenLine(string text, float duration) {
        Text = text;
        FmodId = -1;
        ManualDuration = duration;
    }
}
