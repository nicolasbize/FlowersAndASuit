using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AudioUtils;
using static Dialog;

public class Interactive : MonoBehaviour
{
    [field: SerializeField] public string HintText { get; private set; }
    [field: SerializeField] public float DistanceToInteraction { get; private set; }
    [field: SerializeField] public float OutlineThickness { get; private set; }

}
