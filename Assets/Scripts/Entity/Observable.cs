using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Observable : MonoBehaviour
{
    [field: SerializeField] public SpokenLine Observation { get; private set; }
}

