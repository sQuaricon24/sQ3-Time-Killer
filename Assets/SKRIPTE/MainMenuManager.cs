using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Coffee.UIEffects;
using FirstCollection;

/// <summary>
/// Coffee.UIEffects is used only for intro transitions. 
/// To remove it delete Corouitne IntroSequence() and delete gameobject TRANSITIONS in hierarchy.
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [SerializeField] SoSetting setting;
    [SerializeField] Transform parEffects;
    UITransitionEffect[] effects;
    [SerializeField] Button btnPlay;

    private void Start()
    {
        setting.level = 0;
        setting.score = 0;
        setting.goodMoveStreak = 0;
        setting.xp = PlayerPrefs.GetInt("XP", 0);
        setting.LoadSecondAndThirdPhasePositions();
        StartCoroutine(IntroSequence());
    }

    IEnumerator IntroSequence()
    {
        effects = HelperScript.GetAllChildernByType<UITransitionEffect>(parEffects);
        for (int i = 0; i < effects.Length; i++)
        {
            effects[i].gameObject.SetActive(true);
            effects[i].Show();
            yield return new WaitForSeconds(2);
           // if(i < effects.Length - 1) effects[i].gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        btnPlay.onClick.AddListener(Btn_Play);
    }
    private void OnDisable()
    {
        btnPlay.onClick.RemoveListener(Btn_Play);
    }
    void Btn_Play()
    {
        SceneManager.LoadScene(1);
        StopAllCoroutines();
    }
}
