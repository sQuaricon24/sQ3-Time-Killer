using FirstCollection;
using System.Collections.Generic;
using UnityEngine;

public class HintManager : MonoBehaviour
{
    [SerializeField] Transform parTile, parToken, parPositions, parTileReplace, parTokenReplace, parHintElements;

    private Transform[] hintElements;
    private bool isLevelDone;

    private Dictionary<int[], HintDirection> hintDirections = new Dictionary<int[], HintDirection>();
    readonly HintDirection[] allPossibleHintDirections =
    {
        HintDirection.None,
        HintDirection.YYdoubleTap,
        HintDirection.LeftSwipe,
        HintDirection.DownSwipe,
        HintDirection.UpSwipe,
        HintDirection.YYdoubleTap,
        HintDirection.YYdoubleTap,
        HintDirection.DownSwipe,
        HintDirection.RightSwipe,
        HintDirection.YYdoubleTap,
        HintDirection.LeftSwipe,
        HintDirection.UpSwipe,
        HintDirection.LeftSwipe,
        HintDirection.YYdoubleTap,
        HintDirection.DownSwipe,
        HintDirection.DownSwipe,
        HintDirection.RightSwipe,
        HintDirection.LeftSwipe,
        HintDirection.YYdoubleTap,
        HintDirection.LeftSwipe,
        HintDirection.RightSwipe,
        HintDirection.YYdoubleTap,
        HintDirection.YYdoubleTap,
        HintDirection.DownSwipe
    };


    private void Awake()
    {
        hintDirections.Add(GameManager.lay0, allPossibleHintDirections[0]);
        hintDirections.Add(GameManager.lay1, allPossibleHintDirections[1]);
        hintDirections.Add(GameManager.lay2, allPossibleHintDirections[2]);
        hintDirections.Add(GameManager.lay3, allPossibleHintDirections[3]);
        hintDirections.Add(GameManager.lay4, allPossibleHintDirections[4]);
        hintDirections.Add(GameManager.lay5, allPossibleHintDirections[5]);
        hintDirections.Add(GameManager.lay6, allPossibleHintDirections[6]);
        hintDirections.Add(GameManager.lay7, allPossibleHintDirections[7]);
        hintDirections.Add(GameManager.lay8, allPossibleHintDirections[8]);
        hintDirections.Add(GameManager.lay9, allPossibleHintDirections[9]);
        hintDirections.Add(GameManager.lay10, allPossibleHintDirections[10]);
        hintDirections.Add(GameManager.lay11, allPossibleHintDirections[11]);
        hintDirections.Add(GameManager.lay12, allPossibleHintDirections[12]);
        hintDirections.Add(GameManager.lay13, allPossibleHintDirections[13]);
        hintDirections.Add(GameManager.lay14, allPossibleHintDirections[14]);
        hintDirections.Add(GameManager.lay15, allPossibleHintDirections[15]);
        hintDirections.Add(GameManager.lay16, allPossibleHintDirections[16]);
        hintDirections.Add(GameManager.lay17, allPossibleHintDirections[17]);
        hintDirections.Add(GameManager.lay18, allPossibleHintDirections[18]);
        hintDirections.Add(GameManager.lay19, allPossibleHintDirections[19]);
        hintDirections.Add(GameManager.lay20, allPossibleHintDirections[20]);
        hintDirections.Add(GameManager.lay21, allPossibleHintDirections[21]);
        hintDirections.Add(GameManager.lay22, allPossibleHintDirections[22]);
        hintDirections.Add(GameManager.lay23, allPossibleHintDirections[23]);
    }

    private void OnEnable()
    {
        SquariconGlobalEvents.OnMainHint += MainHint;
        SquariconGlobalEvents.OnResetAllHints += ResetAllHints;
        SquariconGlobalEvents.OnInitializationHint += InitializationHint;
    }

    private void OnDisable()
    {
        SquariconGlobalEvents.OnMainHint -= MainHint;
        SquariconGlobalEvents.OnResetAllHints -= ResetAllHints;
        SquariconGlobalEvents.OnInitializationHint -= InitializationHint;
    }

    #region //HINT
    private void InitializationHint()
    {
        hintElements = HelperScript.GetAllChildernByType<Transform>(parHintElements);
        int counter = 0;
        for (int j = 0; j < 3; j++)
        {
            for (int i = 0; i < 3; i++)
            {
                GameManager.CurrCoordinates[i, j] = new Vector2Int(i, j);
                counter++;
            }
        }

        //MainHint();
    }
    

    private void MainHint(bool justCheckIfLevelDone = false)
    {
        HintDirection hintDirection = HintDirection.UpSwipe;
        foreach (KeyValuePair<int[], HintDirection> item in hintDirections)
        {
            if (item.Key[0] == GameManager.Tokens[1, 0].Value &&
                item.Key[1] == GameManager.Tokens[2, 0].Value &&
                item.Key[2] == GameManager.Tokens[0, 0].Value &&
                item.Key[3] == GameManager.Tokens[2, 1].Value)
            {
                hintDirection = item.Value;
                break;
            }
        }


        switch (hintDirection)
        {
            case HintDirection.UpSwipe:
                hintElements[0].position = GameManager.TileTransforms[2, 1].position;
                hintElements[0].gameObject.SetActive(true);
                hintElements[0].localEulerAngles = new Vector3(0f, 0f, 90f);
                hintElements[1].position = GameManager.TileTransforms[0, 1].position;
                hintElements[1].gameObject.SetActive(true);
                hintElements[1].localEulerAngles = new Vector3(0f, 0f, -90f);
                break;

            case HintDirection.RightSwipe:
                hintElements[0].position = GameManager.TileTransforms[1, 0].position;
                hintElements[0].gameObject.SetActive(true);
                hintElements[1].position = GameManager.TileTransforms[1, 2].position;
                hintElements[1].localEulerAngles = new Vector3(0f, 0f, 180f);
                hintElements[1].gameObject.SetActive(true);
                break;

            case HintDirection.DownSwipe:
                hintElements[0].position = GameManager.TileTransforms[2, 1].position;
                hintElements[0].gameObject.SetActive(true);
                hintElements[0].localEulerAngles = new Vector3(0f, 0f, -90f);
                hintElements[1].position = GameManager.TileTransforms[0, 1].position;
                hintElements[1].gameObject.SetActive(true);
                hintElements[1].localEulerAngles = new Vector3(0f, 0f, 90f);
                break;

            case HintDirection.LeftSwipe:
                hintElements[0].position = GameManager.TileTransforms[1, 0].position;
                hintElements[0].localEulerAngles = new Vector3(0f, 0f, 180f);
                hintElements[0].gameObject.SetActive(true);
                hintElements[1].position = GameManager.TileTransforms[1, 2].position;
                hintElements[1].gameObject.SetActive(true);
                break;

            case HintDirection.YYdoubleTap:
                hintElements[2].position = GameManager.TileTransforms[1, 1].position;
                hintElements[2].gameObject.SetActive(true);
                break;
            case HintDirection.None:
                if (!isLevelDone) SquariconGlobalEvents.OnLevelFinished?.Invoke();
                isLevelDone = true;
                break;
        }

        if (justCheckIfLevelDone)
            ResetAllHints();
        else
        {
            // showing hint resets good move streak
            SoSetting.Instance.goodMoveStreak = 0;
        }

        //if (!SoSetting.Instance.showHints) ResetAllHints();

    }

    private void ResetAllHints()
    {
        for (int i = 0; i < hintElements.Length; i++)
        {
            hintElements[i].localEulerAngles = Vector3.zero;
            hintElements[i].gameObject.SetActive(false);
        }
    }

    #endregion
}
