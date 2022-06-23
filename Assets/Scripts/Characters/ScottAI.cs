using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScottAI : MonoBehaviour
{

    [SerializeField] public CutScene plantDrugsCutScene;
    private bool isDrugsPlanted;

    public void PlantDrugs() {
        if (!isDrugsPlanted) {
            isDrugsPlanted = true;
            CutSceneManager.Instance.PlayCutscene(plantDrugsCutScene);
        }
    }

    public bool IsDrugsPlanted() {
        return isDrugsPlanted;
    }

    public bool IsOnPhone() {
        if (isDrugsPlanted) {
            return false;
        }
        return GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("scott-pickup-phone");
    }

    // Update is called once per frame
    void Update()
    {
        GetComponent<Speakable>().Busy = IsOnPhone();
    }
}
