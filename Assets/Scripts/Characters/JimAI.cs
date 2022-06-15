using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JimAI : MonoBehaviour
{

    [SerializeField] CutScene kiteCutScene;
    [SerializeField] Transform gameLogicManager;

    public bool CanCutKite() {
        return GameObject.Find("Officer Lewis") == null;
    }

    public void CutKite() {
        gameLogicManager.GetComponent<CutScenePlayer>().PlayCutscene(kiteCutScene);
    }

}
