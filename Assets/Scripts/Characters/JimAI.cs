using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JimAI : MonoBehaviour
{

    [SerializeField] CutScene kiteCutScene;

    public bool CanCutKite() {
        return GameObject.Find("Officer Lewis") == null;
    }

    public void CutKite() {
        CutSceneManager.Instance.PlayCutscene(kiteCutScene);
    }

}
