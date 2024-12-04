using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameplayUIManager : MonoBehaviour
{
    private const int CONST_SKINMAX = 4; //number of skin folders in Resources folder
    private const string SKIN_NAME_PREFIX = "Skin";
    private const string DEFAULT_SKIN_FOLDER_NAME = "Skin00";
    private const string ADVENTURE_SKIN_FOLDER_NAME = "SkinAdventure";

    [SerializeField] private Button btnBack;

    [SerializeField] private Button btnTokenSkin1;
    [SerializeField] private Button btnTokenSkin2;
    [SerializeField] private Button btnToggleLightDarkTheme;
    [SerializeField] private Button btnTokenSkin3;
    [SerializeField] private Button btnTokenSkin4;

    [SerializeField] private Button btnHint;

    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI xpText;

    [SerializeField] private GameObject levelDoneGO;
    [SerializeField] private Button[] btnLevelDone;

    [SerializeField] private GameObject goodMoveMarker;
    [SerializeField] private GameObject wrongMoveMarker;

    //[SerializeField] private Image backgroundImageAdventureMode;
    [SerializeField] private Image backgroundImageDark;
    [SerializeField] private Image backgroundImageLight;
    [SerializeField] private Image adventureModeCenterImage;

    [SerializeField] private Color[] skinColors;
    [SerializeField] private TextMeshProUGUI textToColorSkin;

    //[SerializeField] private List<Color> backgroundColors;

    private bool didLoadTokenSpritesForAdventureMode = false;
    private SoSetting settings;
    private bool isDarkThemeActive = true;

    private void OnEnable()
    {
        SetTokenSkinButtonColorsAndDisableIfNeeded();

        settings = SoSetting.Instance;
        scoreText.text = settings.score.ToString();
        xpText.text = settings.xp.ToString();

        //backgroundImageAdventureMode.gameObject.SetActive(false);
        backgroundImageDark.gameObject.SetActive(false);
        backgroundImageLight.gameObject.SetActive(false);
        adventureModeCenterImage.gameObject.SetActive(false);

        if (settings.IsAdventureMode)
        {
            //backgroundImageAdventureMode.gameObject.SetActive(true);
            adventureModeCenterImage.gameObject.SetActive(true);
        }
        else
            backgroundImageDark.gameObject.SetActive(true);

        btnLevelDone[0].onClick.AddListener(HandleBtnNextLevelClick);
        btnLevelDone[1].onClick.AddListener(HandleBtnMainMenuClick);
        btnBack.onClick.AddListener(HandleBtnMainMenuClick);

        btnTokenSkin1.onClick.AddListener(() => SetSkinIndexAndLoadSkin(skinIndex: 0));
        btnTokenSkin2.onClick.AddListener(() => SetSkinIndexAndLoadSkin(skinIndex: 1));
        // this button is different, should be named differently
        btnToggleLightDarkTheme.onClick.AddListener(() => ToggleBetweenDarkAndLightSkin());
        btnTokenSkin3.onClick.AddListener(() => SetSkinIndexAndLoadSkin(skinIndex: 2));
        btnTokenSkin4.onClick.AddListener(() => SetSkinIndexAndLoadSkin(skinIndex: 3));

        btnHint.onClick.AddListener(HandleBtnHintClick);

        SquariconGlobalEvents.OnLevelFinished += HandleLevelFinished;
        SquariconGlobalEvents.OnLevelStarted += HandleLevelStarted;
        SquariconGlobalEvents.OnGoodMoveHappened += HandleGoodMoveHappened;
        SquariconGlobalEvents.OnBadMoveHappened += HandleBadMoveHappened;
        SquariconGlobalEvents.OnScoreUpdated += HandleOnScoreUpdated;
    }

    private void OnDisable()
    {
        btnLevelDone[0].onClick.RemoveListener(HandleBtnNextLevelClick);
        btnLevelDone[1].onClick.RemoveListener(HandleBtnMainMenuClick);
        btnBack.onClick.RemoveListener(HandleBtnMainMenuClick);

        btnTokenSkin1.onClick.RemoveAllListeners();
        btnTokenSkin2.onClick.RemoveAllListeners();
        btnToggleLightDarkTheme.onClick.RemoveAllListeners();
        btnTokenSkin3.onClick.RemoveAllListeners();
        btnTokenSkin4.onClick.RemoveAllListeners();

        SquariconGlobalEvents.OnLevelFinished -= HandleLevelFinished;
        SquariconGlobalEvents.OnLevelStarted -= HandleLevelStarted;
        SquariconGlobalEvents.OnGoodMoveHappened -= HandleGoodMoveHappened;
        SquariconGlobalEvents.OnBadMoveHappened -= HandleBadMoveHappened;
        SquariconGlobalEvents.OnScoreUpdated -= HandleOnScoreUpdated;
    }

    private void SetTokenSkinButtonColorsAndDisableIfNeeded()
    {
        if(SoSetting.Instance.IsAdventureMode)
        {
            btnTokenSkin1.GetComponent<Image>().color = Color.gray;
            btnTokenSkin2.GetComponent<Image>().color = Color.gray;
            btnTokenSkin3.GetComponent<Image>().color = Color.gray;
            btnTokenSkin4.GetComponent<Image>().color = Color.gray;
            btnTokenSkin1.enabled = false;
            btnTokenSkin2.enabled = false;
            btnTokenSkin3.enabled = false;
            btnTokenSkin4.enabled = false;
        }
        else
        {
            btnTokenSkin1.GetComponent<Image>().color = skinColors[0];
            btnTokenSkin2.GetComponent<Image>().color = skinColors[1];
            btnTokenSkin3.GetComponent<Image>().color = skinColors[2];
            btnTokenSkin4.GetComponent<Image>().color = skinColors[3];
            btnTokenSkin1.enabled = true;
            btnTokenSkin2.enabled = true;
            btnTokenSkin3.enabled = true;
            btnTokenSkin4.enabled = true;
        }
    }

    private void HandleLevelStarted()
    {
        LoadCurrentSkin();
    }

    private void HandleBadMoveHappened()
    {
        StartCoroutine(ShowMarkerThenHideItAfterSeconds(wrongMoveMarker, 3f));
    }

    private void HandleGoodMoveHappened()
    {
        StartCoroutine(ShowMarkerThenHideItAfterSeconds(goodMoveMarker, 3f));
    }

    private void HandleLevelFinished()
    {
        SquariconGlobalEvents.OnResetAllHints?.Invoke();

        //tweenFinishedCounter = 100;
        settings.level += 1;

        if (settings.level > 19 && settings.score < settings.firstPhaseScore ||
            settings.level > 99 && settings.score < settings.secondPhaseScore ||
            settings.level > 252 && settings.score > settings.secondPhaseScore)
        {
            settings.level = 0;
        }


        levelDoneGO.SetActive(true);
        btnLevelDone[0].gameObject.SetActive(true);
        btnLevelDone[1].gameObject.SetActive(true);
    }

    private void HandleBtnNextLevelClick()
    {
        SceneManager.LoadScene(1);
    }

    private void HandleBtnMainMenuClick()
    {
        SceneManager.LoadScene(0);
    }

    private void HandleBtnHintClick()
    {
        settings.showHints = !settings.showHints;
        SquariconGlobalEvents.OnMainHint?.Invoke();
    }

    private void LoadCurrentSkin()
    {   
        string skinFolderName = "";

        if (settings.IsAdventureMode)
        {
            //backgroundImageAdventureMode.color = backgroundColors[settings.CurrentSkinIndex];
            if (!didLoadTokenSpritesForAdventureMode)
            {
                skinFolderName = ADVENTURE_SKIN_FOLDER_NAME;
                didLoadTokenSpritesForAdventureMode = true;
            }
            else
            {
                return;
            }
        }
        else
        {
            string skinSuffix = settings.CurrentSkinIndex < 10 ? "0" : "";
            skinSuffix += settings.CurrentSkinIndex.ToString();
            skinFolderName = SKIN_NAME_PREFIX + skinSuffix;
        }

        Sprite[] skinSprites = Resources.LoadAll<Sprite>(skinFolderName);
        if (skinSprites == null || skinSprites.Length <= 0)
        {
            Debug.Log("Can't find skin, returning default skin");
            skinSprites = Resources.LoadAll<Sprite>(DEFAULT_SKIN_FOLDER_NAME);
        }

        settings.tileSprites.Clear();
        settings.tokenSprites.Clear();
        for (int i = 0; i < skinSprites.Length; i++)
        {
            if (i % 2 == 0) settings.tileSprites.Add(skinSprites[i]);
            else settings.tokenSprites.Add(skinSprites[i]);
        }

        //textToColorSkin.color = skinColors[settings.CurrentSkinIndex];

        SquariconGlobalEvents.OnSkinUpdated?.Invoke();        
    }

    private void ToggleBetweenDarkAndLightSkin()
    {
        isDarkThemeActive = !isDarkThemeActive;
        backgroundImageDark.gameObject.SetActive(isDarkThemeActive);
        backgroundImageLight.gameObject.SetActive(!isDarkThemeActive);
    }

    /*private void IncrementAndLoadSkin()
    {
        int newSkinIndex = settings.CurrentSkinIndex + 1;
        if (newSkinIndex >= CONST_SKINMAX)
            newSkinIndex = 0;

        settings.CurrentSkinIndex = newSkinIndex;
        LoadCurrentSkin();
    }*/

    private void SetSkinIndexAndLoadSkin(int skinIndex)
    {
        settings.CurrentSkinIndex = skinIndex;
        LoadCurrentSkin();
    }

    private IEnumerator ShowMarkerThenHideItAfterSeconds(GameObject markerObject, float seconds)
    {
        yield return new WaitForSeconds(0.2f);
        goodMoveMarker.gameObject.SetActive(false);
        wrongMoveMarker.gameObject.SetActive(false);
        markerObject.SetActive(true);
        yield return new WaitForSeconds(seconds);
        markerObject.SetActive(false);
    }

    private void HandleOnScoreUpdated()
    {
        scoreText.text = settings.score.ToString();
        xpText.text = settings.xp.ToString();
    }
}
