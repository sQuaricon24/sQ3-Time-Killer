using System.Collections;
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
    [SerializeField] private SoSetting setting;
    [SerializeField] private Transform parEffects;
    [SerializeField] private Button btnAdventure;
    [SerializeField] private Button btnClassic;
    [SerializeField] private GameObject mainMenuBackground;

    private UITransitionEffect[] effects;
    private static bool didPlayIntro = false;

    private void Start()
    {
        btnAdventure.gameObject.SetActive(false);
        btnClassic.gameObject.SetActive(false);

        setting.level = 0;
        setting.score = 0;
        setting.goodMoveStreak = 0;
        setting.xp = PlayerPrefs.GetInt("XP", 0);
        setting.LoadSecondAndThirdPhasePositions();

        if(!didPlayIntro)
            StartCoroutine(IntroSequence());
        else
        {
            ShowMainMenu();
        }
    }

    private IEnumerator IntroSequence()
    {
        effects = HelperScript.GetAllChildernByType<UITransitionEffect>(parEffects);
        for (int i = 0; i < effects.Length; i++)
        {
            effects[i].gameObject.SetActive(true);
            effects[i].Show();
            yield return new WaitForSeconds(2);
        }

        didPlayIntro = true;
        ShowMainMenu();
    }

    private void ShowMainMenu()
    {
        mainMenuBackground.gameObject.SetActive(true);
        btnAdventure.gameObject.SetActive(true);
        btnClassic.gameObject.SetActive(true);
    }

    private void OnEnable()
    {
        btnAdventure.onClick.AddListener(HandleBtnAdventureClick);
        btnClassic.onClick.AddListener(HandleBtnClassicClick);
    }

    private void OnDisable()
    {
        btnAdventure.onClick.RemoveListener(HandleBtnAdventureClick);
        btnClassic.onClick.RemoveListener(HandleBtnClassicClick);
    }

    private void HandleBtnAdventureClick()
    {
        SceneManager.LoadScene(1);
        StopAllCoroutines();
    }

    private void HandleBtnClassicClick()
    {
        SceneManager.LoadScene(2);
        StopAllCoroutines();
    }
}
