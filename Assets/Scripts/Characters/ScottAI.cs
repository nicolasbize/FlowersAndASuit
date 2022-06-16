using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScottAI : MonoBehaviour
{

    [SerializeField] public CutScene plantDrugsCutScene;
    [SerializeField] Transform gameLogicManager;
    private bool isDrugsPlanted = false;

    public void PlantDrugs() {
        if (!isDrugsPlanted) {
            isDrugsPlanted = true;
            gameLogicManager.GetComponent<CutScenePlayer>().PlayCutscene(plantDrugsCutScene);
        }
    }

    public bool IsDrugsPlanted() {
        return isDrugsPlanted;
    }

    public bool IsOnPhone() {
        return GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("scott-idle-phone");
    }

    // Update is called once per frame
    void Update()
    {
        GetComponent<Interactive>().busy = IsOnPhone();
    }
}
