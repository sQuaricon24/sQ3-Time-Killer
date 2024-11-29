using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameplayUIManager : MonoBehaviour
{
    private const int CONST_SKINMAX = 4; //number of skin folders in Resources folder

    [SerializeField] public SoSetting settings;

    [SerializeField] private Button btnBack;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI xpText;

    [SerializeField] GameObject levelDoneGO;
    [SerializeField] Button[] btnLevelDone;
    [SerializeField] Button btnSkin, btnHint;

    [SerializeField] GameObject goodMoveMarker;
    [SerializeField] GameObject wrongMoveMarker;

    [SerializeField] Color[] skinColors;
    [SerializeField] Image[] imageToColorSkin;
    [SerializeField] TextMeshProUGUI textToColorSkin;

    private int skinCounter = 0;

    private void OnEnable()
    {
        scoreText.text = settings.score.ToString();
        xpText.text = settings.xp.ToString();
        SquariconGlobalEvents.OnLevelFinished += Ev_LevelDone;
        btnLevelDone[0].onClick.AddListener(HandleBtnNextLevelClick);
        btnLevelDone[1].onClick.AddListener(HandleBtnMainMenuClick);
        btnBack.onClick.AddListener(HandleBtnMainMenuClick);
        btnSkin.onClick.AddListener(delegate
        {
            SkinChooser(true);
        });
        btnHint.onClick.AddListener(HandleBtnHintClick);

        SquariconGlobalEvents.OnLevelStarted += HandleLevelStarted;
        SquariconGlobalEvents.OnGoodMoveHappened += HandleGoodMoveHappened;
        SquariconGlobalEvents.OnBadMoveHappened += HandleBadMoveHappened;
        SquariconGlobalEvents.OnScoreUpdated += HandleOnScoreUpdated;
    }

    private void OnDisable()
    {
        SquariconGlobalEvents.OnLevelFinished -= Ev_LevelDone;
        btnLevelDone[0].onClick.RemoveListener(HandleBtnNextLevelClick);
        btnLevelDone[1].onClick.RemoveListener(HandleBtnMainMenuClick);
        btnBack.onClick.RemoveListener(HandleBtnMainMenuClick);
        btnSkin.onClick.RemoveAllListeners();
        btnHint.onClick.RemoveListener(HandleBtnHintClick);

        SquariconGlobalEvents.OnLevelStarted -= HandleLevelStarted;
        SquariconGlobalEvents.OnGoodMoveHappened -= HandleGoodMoveHappened;
        SquariconGlobalEvents.OnBadMoveHappened -= HandleBadMoveHappened;
        SquariconGlobalEvents.OnScoreUpdated -= HandleOnScoreUpdated;
    }

    private void HandleLevelStarted()
    {
        SkinChooser(incrementSkin: false);
    }


    private void HandleBadMoveHappened()
    {
        StartCoroutine(ShowMarkerThenHideItAfterSeconds(wrongMoveMarker, 3f));
    }

    private void HandleGoodMoveHappened()
    {
        StartCoroutine(ShowMarkerThenHideItAfterSeconds(goodMoveMarker, 3f));
    }

    private void Ev_LevelDone(int lv)
    {
        skinCounter = settings.skinOrdinal;

        SquariconGlobalEvents.OnResetAllHints?.Invoke();
        //ResetAllHints();
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
        //MainHint();
    }

    /// <summary>
    /// Changes skin. Loads them from RESOURCES folder. Name of each folder that holds skin should be Skin0x, where 0x is number of skin.
    /// </summary>
    /// <param name="incrementSkin">Everything about skins is updated in this method. False just means to update all, but dont incrmenet to next skin. False is sed in initialization only. </param>
    private void SkinChooser(bool incrementSkin)
    {
        if (incrementSkin)
        {
            skinCounter = (1 + skinCounter) % CONST_SKINMAX;
            settings.skinOrdinal = skinCounter;
        }
        settings.tileSprites.Clear();
        settings.tokenSprites.Clear();

        string dec = settings.skinOrdinal < 10 ? "0" : "";
        string folderName = "Skin" + dec + settings.skinOrdinal.ToString();
        Sprite[] allSprites = Resources.LoadAll<Sprite>(folderName);
        if (allSprites == null || allSprites.Length <= 0)
        {
            Debug.Log("Can't find skin, returning default skin");
            allSprites = Resources.LoadAll<Sprite>("Skin00");
        }
        for (int i = 0; i < allSprites.Length; i++)
        {
            if (i % 2 == 0) settings.tileSprites.Add(allSprites[i]);
            else settings.tokenSprites.Add(allSprites[i]);
        }

        SquariconGlobalEvents.OnSkinUpdated?.Invoke();

        /*
        for (int i = 0; i < imageToColorSkin.Length; i++)
        {
            imageToColorSkin[i].color = skinColors[skinCounter];
        }*/

        textToColorSkin.color = skinColors[skinCounter];

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
