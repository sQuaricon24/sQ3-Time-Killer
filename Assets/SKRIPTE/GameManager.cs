using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FirstCollection;
using TMPro;
using DG.Tweening;
using UnityEngine.SceneManagement;
using System.Linq;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Button btnBack;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI xpText;
    public static GameManager gm;
    public SoSetting setting;

    #region//TEMPORARY, only to demonstrate skin color (no functionality)
    [SerializeField] Color[] skinColors;
    [SerializeField] Image[] imageToColorSkin;
    [SerializeField] TextMeshProUGUI textToColorSkin;
    #endregion

    [SerializeField] Transform kanvas;
    [SerializeField] Transform parTile, parToken, parPositions, parTileReplace, parTokenReplace, parHintElements;
    [SerializeField] GameObject levelDoneGO;
    [SerializeField] Button[] btnLevelDone;
    [SerializeField] Button btnSkin, btnHint;

    [SerializeField] GameObject goodMoveMarker;
    [SerializeField] GameObject wrongMoveMarker;
    [SerializeField] List<int> mowesToWinList;


    bool _levelDone;
    int _skinCounter;
    const int CONST_SKINMAX = 4; //number of skin folders in Resources folder
    int _roundCounter;

    Tile[,] _tiles = new Tile[3, 3];
    Transform[,] _tileTransform = new Transform[3, 3];
    Token[,] _tokens = new Token[3, 3];
    Transform[,] _tokenTransform = new Transform[3, 3];
    Vector2[,] _rectPosition = new Vector2[5, 5]; //position is the same for tiles and tokens. Used for animations.
    int[,] _prevTileVrijednost = new int[3, 3]; //buffer values
    int[,] _prevTokenVrijednost = new int[3, 3]; //buffer values
    Token[] _replaceTokens; //used for animation only
    Tile[] _replaceTiles; //used for animation only
    int _moveTokenEvenCounter; //token are moved twice per player action. Some values shouldn't be updated twice and this bool makes sure of that.

    private int currentMovesToWin = -1;
    private int currentPositionIndex = -1;
    private int previousPositionIndex = -1;
    #region//TWEENS
    [SerializeField] Ease izy;
    const float CONST_TWEENDURATION = 1f;
    int _tweenFinishedCounter = 0; // inputs (drags) are disabled until tween finishes. Int is used instead of bool beacuse MoveToken is called twice per drag.
    bool _tweenOneHitCheck = true; // tween are called 6+ times and they all OnComplete(EndTween). That method should be called once, not 6 times.
    #endregion

    #region//HINT VARIABLES
    Transform[] _hintElements;
    Vector2Int[,] _currCoordinates = new Vector2Int[3, 3];
    Vector2Int[,] _prevCoordinates = new Vector2Int[3, 3];
    readonly Dictionary<int[], int[]> _mainPairs = new Dictionary<int[], int[]>();
    readonly List<int[]> _allCombinations = new List<int[]>();
    readonly int[] layNull = new int[4] { 0, 0, 0, 0 };
    readonly int[] lay0 = new int[4] { 1, 2, 3, 4 };
    readonly int[] lay1 = new int[4] { 1, 2, 4, 3 };
    readonly int[] lay2 = new int[4] { 1, 3, 2, 4 };
    readonly int[] lay3 = new int[4] { 1, 3, 4, 2 };
    readonly int[] lay4 = new int[4] { 1, 4, 2, 3 };
    readonly int[] lay5 = new int[4] { 1, 4, 3, 2 };
    readonly int[] lay6 = new int[4] { 2, 1, 3, 4 };
    readonly int[] lay7 = new int[4] { 2, 1, 4, 3 };
    readonly int[] lay8 = new int[4] { 2, 3, 1, 4 };
    readonly int[] lay9 = new int[4] { 2, 3, 4, 1 };
    readonly int[] lay10 = new int[4] { 2, 4, 1, 3 };
    readonly int[] lay11 = new int[4] { 2, 4, 3, 1 };
    readonly int[] lay12 = new int[4] { 3, 1, 2, 4 };
    readonly int[] lay13 = new int[4] { 3, 1, 4, 2 };
    readonly int[] lay14 = new int[4] { 3, 2, 1, 4 };
    readonly int[] lay15 = new int[4] { 3, 2, 4, 1 };
    readonly int[] lay16 = new int[4] { 3, 4, 1, 2 };
    readonly int[] lay17 = new int[4] { 3, 4, 2, 1 };
    readonly int[] lay18 = new int[4] { 4, 1, 2, 3 };
    readonly int[] lay19 = new int[4] { 4, 1, 3, 2 };
    readonly int[] lay20 = new int[4] { 4, 2, 1, 3 };
    readonly int[] lay21 = new int[4] { 4, 2, 3, 1 };
    readonly int[] lay22 = new int[4] { 4, 3, 1, 2 };
    readonly int[] lay23 = new int[4] { 4, 3, 2, 1 };


    Dictionary<int[], HintDirection> dic = new Dictionary<int[], HintDirection>();
    readonly HintDirection[] _directions =
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
    #endregion

    private void Awake()
    {
        gm = this;
    }
    private void Start()
    {
        NewGameBoard();
    }

    #region//EVENTS, BUTTONS
    private void OnEnable()
    {
        scoreText.text = setting.score.ToString();
        xpText.text = setting.xp.ToString();
        HelperScript.LevelFinished += Ev_LevelDone;
        btnLevelDone[0].onClick.AddListener(HandleBtnNextLevelClick);
        btnLevelDone[1].onClick.AddListener(HandleBtnMainMenuClick);
        btnBack.onClick.AddListener(HandleBtnMainMenuClick);
        btnSkin.onClick.AddListener(delegate
        {
            SkinChooser(true);
        });
        btnHint.onClick.AddListener(HandleBtnHintClick);
    }

    private void OnDisable()
    {
        HelperScript.LevelFinished -= Ev_LevelDone;
        btnLevelDone[0].onClick.RemoveListener(HandleBtnNextLevelClick);
        btnLevelDone[1].onClick.RemoveListener(HandleBtnMainMenuClick);
        btnBack.onClick.RemoveListener(HandleBtnMainMenuClick);
        btnSkin.onClick.RemoveAllListeners();
        btnHint.onClick.RemoveListener(HandleBtnHintClick);
    }

    private void Ev_LevelDone(int lv)
    {
        ResetAllHints();
        _tweenFinishedCounter = 100;
        setting.level += 1;

        if (setting.level > 19 && setting.score < setting.firstPhaseScore ||
            setting.level > 99 && setting.score < setting.secondPhaseScore ||
            setting.level > 252 && setting.score > setting.secondPhaseScore)
        {
            setting.level = 0;
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
        setting.showHints = !setting.showHints;
        MainHint();
    }

    #endregion

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) SceneManager.LoadScene(gameObject.scene.name);
        else if (Input.GetKeyDown(KeyCode.Escape)) SceneManager.LoadScene(0);
    }

    #region //INITIALIZATON

    private void NewGameBoard()
    {
        _skinCounter = setting.skinOrdinal;
        SkinChooser(false);

        _replaceTiles = HelperScript.GetAllChildernByType<Tile>(parTileReplace);
        _replaceTokens = HelperScript.GetAllChildernByType<Token>(parTokenReplace);
        InitializationMain(MainType.Tile);
        InitializationMain(MainType.Token);

        IniDic();

        // current position from 23 possible positions
        currentPositionIndex = setting.GetPositionForLevelAndScore(setting.level,setting.score);

        _tokens[1, 0].Vrijednost = _tokens[1, 2].Vrijednost = _allCombinations[currentPositionIndex][0];
        _tokens[2, 0].Vrijednost = _tokens[0, 2].Vrijednost = _allCombinations[currentPositionIndex][1];
        _tokens[0, 0].Vrijednost = _tokens[2, 2].Vrijednost = _allCombinations[currentPositionIndex][2];
        _tokens[0, 1].Vrijednost = _tokens[2, 1].Vrijednost = _allCombinations[currentPositionIndex][3];
        currentMovesToWin = GetMovesToWinForPositionIndex(currentPositionIndex);
        InitializationHint();
    }

    private int GetPositionIndexFromCurrentVrijednost()
    {
        for(int i = 0; i < _allCombinations.Count; i++)
        {
            if(_tokens[1, 0].Vrijednost == _allCombinations[i][0] &&
               _tokens[2, 0].Vrijednost == _allCombinations[i][1] &&
               _tokens[0, 0].Vrijednost == _allCombinations[i][2] &&
               _tokens[0, 1].Vrijednost == _allCombinations[i][3])
            {
                return i;
            }
        }

        Debug.LogError("GetPositionIndexFromCurrentVrijednost returning invalid index -1");
        return -1;
    }

    private int GetMovesToWinForPositionIndex(int positionIndex)
    {
        return mowesToWinList[positionIndex];
    }

    public void HandleMoveFinished()
    {
        int newPosition = GetPositionIndexFromCurrentVrijednost();
        int newMovesToWin = GetMovesToWinForPositionIndex(newPosition);

        previousPositionIndex = currentPositionIndex;
        currentPositionIndex = newPosition;

        //DebugCurrentAndPreviousPosition();

        goodMoveMarker.gameObject.SetActive(false);
        wrongMoveMarker.gameObject.SetActive(false);
        if (newMovesToWin < currentMovesToWin)
        {
            setting.goodMoveStreak++;
            setting.xp += GetXpForWinStreak(setting.goodMoveStreak);
            setting.AddScore(GetScoreForWinStreak(setting.goodMoveStreak));
            PlayerPrefs.SetInt("XP", setting.xp);
            scoreText.text = setting.score.ToString();
            xpText.text = setting.xp.ToString();
            //Debug.LogError("Score is now: " + setting.score);

            StartCoroutine(ShowMarkerThenHideItAfterSeconds(goodMoveMarker, 3f));
        }
        else
        {
            setting.goodMoveStreak = 0;
            StartCoroutine(ShowMarkerThenHideItAfterSeconds(wrongMoveMarker, 3f));
        }

        currentMovesToWin = newMovesToWin;
    }

    private int GetScoreForWinStreak(int streak)
    {
        switch(streak)
        {
            case (0):
                return 0;
            case (1):
                return 0;
            case (2):
                return 10;
            case (3):
                return 20;
            case (4):
                return 40;
            case (5):
                return 80;
            default:
                return 80;
        }
    }

    private int GetXpForWinStreak(int streak)
    {
        switch (streak)
        {
            case (0):
                return 0;
            case (1):
                return 5;
            case (2):
                return 10;
            case (3):
                return 20;
            case (4):
                return 40;
            case (5):
                return 80;
            default:
                return 80;
        }
    }

    private void DebugCurrentAndPreviousPosition()
    {
        _tokens[1, 0].Vrijednost = _tokens[1, 2].Vrijednost = _allCombinations[currentPositionIndex][0];
        _tokens[2, 0].Vrijednost = _tokens[0, 2].Vrijednost = _allCombinations[currentPositionIndex][1];
        _tokens[0, 0].Vrijednost = _tokens[2, 2].Vrijednost = _allCombinations[currentPositionIndex][2];
        _tokens[0, 1].Vrijednost = _tokens[2, 1].Vrijednost = _allCombinations[currentPositionIndex][3];

        Debug.LogError("PREV POS INDEX " + previousPositionIndex);
        if(previousPositionIndex >= 0)
        {
            DebugPositionIndex(previousPositionIndex);
        }
        Debug.LogError("CURRENT POS INDEX " + currentPositionIndex);
        if (currentPositionIndex >= 0)
        {
            DebugPositionIndex(currentPositionIndex);
        }
    }

    private void DebugPositionIndex(int positionIndex)
    {
        Debug.LogError("{" + _allCombinations[positionIndex][0] + "," 
                           + _allCombinations[positionIndex][1] + ","
                           + _allCombinations[positionIndex][2] + ","
                           + _allCombinations[positionIndex][3] + "}");

        Debug.LogError("MOVES TO WIN: " + mowesToWinList[positionIndex]);
    }

    private IEnumerator ShowMarkerThenHideItAfterSeconds(GameObject markerObject, float seconds)
    {
        yield return new WaitForSeconds(0.2f);
        markerObject.SetActive(true);
        yield return new WaitForSeconds(seconds);
        markerObject.SetActive(false);
    }

    /// <summary>
    /// Changes skin. Loads them from RESOURCES folder. Name of each folder that holds skin should be Skin0x, where 0x is number of skin.
    /// </summary>
    /// <param name="incrementSkin">Everything about skins is updated in this method. False just means to update all, but dont incrmenet to next skin. False is sed in initialization only. </param>
    private void SkinChooser(bool incrementSkin)
    {
        if (incrementSkin)
        {
            _skinCounter = (1 + _skinCounter) % CONST_SKINMAX;
            setting.skinOrdinal = _skinCounter;
        }
        setting.tileSprites.Clear();
        setting.tokenSprites.Clear();

        string dec = setting.skinOrdinal < 10 ? "0" : "";
        string folderName = "Skin" + dec + setting.skinOrdinal.ToString();
        Sprite[] allSprites = Resources.LoadAll<Sprite>(folderName);
        if (allSprites == null || allSprites.Length <= 0)
        {
            Debug.Log("Can't find skin, returning default skin");
            allSprites = Resources.LoadAll<Sprite>("Skin00");
        }
        for (int i = 0; i < allSprites.Length; i++)
        {
            if (i % 2 == 0) setting.tileSprites.Add(allSprites[i]);
            else setting.tokenSprites.Add(allSprites[i]);
        }

        HelperScript.SkinUpdated?.Invoke();

        for (int i = 0; i < imageToColorSkin.Length; i++)
        {
            imageToColorSkin[i].color = skinColors[_skinCounter];
        }
        textToColorSkin.color = skinColors[_skinCounter];

    }
    /// <summary>
    /// Randomizes next level. 
    /// </summary>
    /// <param name="prevLevel">Makes sure that next level is different prfom previous</param>
    /// <returns></returns>
    private int RandomLevel(int prevLevel)
    {
        List<int> brojevi = Enumerable.Range(0, 23).ToList();
        brojevi.Remove(prevLevel);
        var rnd = new System.Random();
        List<int> list = brojevi.OrderBy(n => rnd.Next()).ToList();
        setting.level = list[0];
        return list[0];
    }
    private void InitializationMain(MainType mainType)
    {
        int counter = 0;
        for (int j = 0; j < 3; j++)
        {
            for (int i = 0; i < 3; i++)
            {
                if (mainType == MainType.Tile)
                {
                    _tiles[i, j] = parTile.GetChild(counter).GetComponent<Tile>();
                    _tiles[i, j].Pozicija = new Vector2Int(i, j);
                    _tileTransform[i, j] = _tiles[i, j].transform;
                }
                else
                {
                    _tokens[i, j] = parToken.GetChild(counter).GetComponent<Token>();
                    _tokens[i, j].Pozicija = new Vector2Int(i, j);
                  //  print(_tokens[i, j].name);
                    _tokenTransform[i, j] = _tokens[i, j].transform;
                }
                counter++;
            }
        }
        counter = 0;
        if (mainType == MainType.Tile)
        {
            for (int j = 0; j < 5; j++)
            {
                for (int i = 0; i < 5; i++)
                {
                    _rectPosition[i, j] = parPositions.GetChild(counter).position;
                    counter++;
                }
            }
        }
    }
    #endregion

    #region //HINT
    private void InitializationHint()
    {
        _hintElements = HelperScript.GetAllChildernByType<Transform>(parHintElements);
        int counter = 0;
        for (int j = 0; j < 3; j++)
        {
            for (int i = 0; i < 3; i++)
            {
                _currCoordinates[i, j] = new Vector2Int(i, j);
                counter++;
            }
        }

        MainHint();
    }
    private void IniDic()
    {
        _allCombinations.Add(lay0);
        _allCombinations.Add(lay1);
        _allCombinations.Add(lay2);
        _allCombinations.Add(lay3);
        _allCombinations.Add(lay4);
        _allCombinations.Add(lay5);
        _allCombinations.Add(lay6);
        _allCombinations.Add(lay7);
        _allCombinations.Add(lay8);
        _allCombinations.Add(lay9);
        _allCombinations.Add(lay10);
        _allCombinations.Add(lay11);
        _allCombinations.Add(lay12);
        _allCombinations.Add(lay13);
        _allCombinations.Add(lay14);
        _allCombinations.Add(lay15);
        _allCombinations.Add(lay16);
        _allCombinations.Add(lay17);
        _allCombinations.Add(lay18);
        _allCombinations.Add(lay19);
        _allCombinations.Add(lay20);
        _allCombinations.Add(lay21);
        _allCombinations.Add(lay22);
        _allCombinations.Add(lay23);


        _mainPairs.Add(lay0, layNull);
        _mainPairs.Add(lay1, lay15);
        _mainPairs.Add(lay2, lay14);
        _mainPairs.Add(lay3, lay0);
        _mainPairs.Add(lay4, lay0);
        _mainPairs.Add(lay5, lay11);
        _mainPairs.Add(lay6, lay19);
        _mainPairs.Add(lay7, lay8);
        _mainPairs.Add(lay8, lay0);
        _mainPairs.Add(lay9, lay2);
        _mainPairs.Add(lay10, lay18);
        _mainPairs.Add(lay11, lay8);
        _mainPairs.Add(lay12, lay0);
        _mainPairs.Add(lay13, lay7);
        _mainPairs.Add(lay14, lay17);
        _mainPairs.Add(lay15, lay12);
        _mainPairs.Add(lay16, lay2);
        _mainPairs.Add(lay17, lay21);
        _mainPairs.Add(lay18, lay12);
        _mainPairs.Add(lay19, lay3);
        _mainPairs.Add(lay20, lay4);
        _mainPairs.Add(lay21, lay0);
        _mainPairs.Add(lay22, lay8);
        _mainPairs.Add(lay23, lay19);

        dic.Add(lay0, _directions[0]);
        dic.Add(lay1, _directions[1]);
        dic.Add(lay2, _directions[2]);
        dic.Add(lay3, _directions[3]);
        dic.Add(lay4, _directions[4]);
        dic.Add(lay5, _directions[5]);
        dic.Add(lay6, _directions[6]);
        dic.Add(lay7, _directions[7]);
        dic.Add(lay8, _directions[8]);
        dic.Add(lay9, _directions[9]);
        dic.Add(lay10, _directions[10]);
        dic.Add(lay11, _directions[11]);
        dic.Add(lay12, _directions[12]);
        dic.Add(lay13, _directions[13]);
        dic.Add(lay14, _directions[14]);
        dic.Add(lay15, _directions[15]);
        dic.Add(lay16, _directions[16]);
        dic.Add(lay17, _directions[17]);
        dic.Add(lay18, _directions[18]);
        dic.Add(lay19, _directions[19]);
        dic.Add(lay20, _directions[20]);
        dic.Add(lay21, _directions[21]);
        dic.Add(lay22, _directions[22]);
        dic.Add(lay23, _directions[23]);
    }

    private void MainHint()
    {
        HintDirection hintDirection = HintDirection.UpSwipe;
        foreach (KeyValuePair<int[], HintDirection> item in dic)
        {
            if (item.Key[0] == _tokens[1, 0].Vrijednost && 
                item.Key[1] == _tokens[2, 0].Vrijednost && 
                item.Key[2] == _tokens[0, 0].Vrijednost && 
                item.Key[3] == _tokens[2, 1].Vrijednost)
            {
                hintDirection = item.Value;
                break;
            }
        }


        switch (hintDirection)
        {
            case HintDirection.UpSwipe:
                _hintElements[0].position = _tileTransform[2, 1].position;
                _hintElements[0].gameObject.SetActive(true);
                _hintElements[0].localEulerAngles = new Vector3(0f, 0f, 90f);
                _hintElements[1].position = _tileTransform[0, 1].position;
                _hintElements[1].gameObject.SetActive(true);
                _hintElements[1].localEulerAngles = new Vector3(0f, 0f, -90f);
                break;

            case HintDirection.RightSwipe:
                _hintElements[0].position = _tileTransform[1, 0].position;
                _hintElements[0].gameObject.SetActive(true);
                _hintElements[1].position = _tileTransform[1, 2].position;
                _hintElements[1].localEulerAngles = new Vector3(0f, 0f, 180f);
                _hintElements[1].gameObject.SetActive(true);
                break;

            case HintDirection.DownSwipe:
                _hintElements[0].position = _tileTransform[2, 1].position;
                _hintElements[0].gameObject.SetActive(true);
                _hintElements[0].localEulerAngles = new Vector3(0f, 0f, -90f);
                _hintElements[1].position = _tileTransform[0, 1].position;
                _hintElements[1].gameObject.SetActive(true);
                _hintElements[1].localEulerAngles = new Vector3(0f, 0f, 90f);
                break;

            case HintDirection.LeftSwipe:
                _hintElements[0].position = _tileTransform[1, 0].position;
                _hintElements[0].localEulerAngles = new Vector3(0f, 0f, 180f);
                _hintElements[0].gameObject.SetActive(true);
                _hintElements[1].position = _tileTransform[1, 2].position;
                _hintElements[1].gameObject.SetActive(true);
                break;

            case HintDirection.YYdoubleTap:
                _hintElements[2].position = _tileTransform[1, 1].position;
                _hintElements[2].gameObject.SetActive(true);
                break;
            case HintDirection.None:
                if (!_levelDone) HelperScript.LevelFinished?.Invoke(setting.level);
                _levelDone = true;
                break;
        }
        if (!setting.showHints) ResetAllHints();

    }

    private void ResetAllHints()
    {
        for (int i = 0; i < _hintElements.Length; i++)
        {
            _hintElements[i].localEulerAngles = Vector3.zero;
            _hintElements[i].gameObject.SetActive(false);
        }
    }

    #endregion

    #region//MAIN MECHANICS
    public void MoveTokenByPosition(Vector2Int moveDir, Vector2Int poz)
    {
        if (_tweenFinishedCounter > 1) return;
        _tweenFinishedCounter++;
        _tweenOneHitCheck = false;
        bool incrementHint = _moveTokenEvenCounter % 2 == 0;
        if (incrementHint) _roundCounter++;
        RecordPreviousVrijednost();

        _replaceTokens[_moveTokenEvenCounter].gameObject.SetActive(true);

        if (moveDir.x != 0) //horizontal move
        {
            int dirToken = moveDir.x == 1 ? 2 : 1;
            int dirTransform = moveDir.x == 1 ? 0 : 2;
            for (int i = 0; i < 3; i++)
            {
                _tokens[i, poz.y].Vrijednost = _prevTokenVrijednost[(i + dirToken) % 3, poz.y];
                _tokenTransform[i, poz.y].DOMove(_rectPosition[i + 1, poz.y + 1], CONST_TWEENDURATION)
                    .From(_rectPosition[i + dirTransform, poz.y + 1])
                    .SetEase(izy)
                    .OnComplete(() => EndTweenDrag(incrementHint));
            }

            //edge tokens, needed for tween animation only
            if (moveDir.x == 1)
            {
                _replaceTokens[_moveTokenEvenCounter].Vrijednost = _prevTokenVrijednost[2, poz.y];
                _replaceTokens[_moveTokenEvenCounter].transform.DOMove(_rectPosition[4, poz.y + 1], CONST_TWEENDURATION)
                    .SetEase(izy)
                    .From(_rectPosition[3, poz.y + 1]);
            }
            else
            {
                _replaceTokens[_moveTokenEvenCounter].Vrijednost = _prevTokenVrijednost[0, poz.y];
                _replaceTokens[_moveTokenEvenCounter].transform.DOMove(_rectPosition[0, poz.y + 1], CONST_TWEENDURATION)
                    .SetEase(izy)
                    .From(_rectPosition[1, poz.y + 1]);
            }
        }
        else if (moveDir.y != 0) //vertical move
        {
            int dirToken = moveDir.y == 1 ? 2 : 1;
            int dirTransform = moveDir.y == 1 ? 0 : 2;
            for (int j = 0; j < 3; j++)
            {
                _tokens[poz.x, j].Vrijednost = _prevTokenVrijednost[poz.x, (j + dirToken) % 3];
                _tokenTransform[poz.x, j].DOMove(_rectPosition[poz.x + 1, j + 1], CONST_TWEENDURATION)
                    .From(_rectPosition[poz.x + 1, j + dirTransform])
                    .SetEase(izy)
                    .OnComplete(() => EndTweenDrag(incrementHint));
            }

            //edge tokens, needed for tween animation only
            if (moveDir.y == 1)
            {
                _replaceTokens[_moveTokenEvenCounter].Vrijednost = _prevTokenVrijednost[poz.x, 2];
                _replaceTokens[_moveTokenEvenCounter].transform.DOMove(_rectPosition[poz.x + 1, 4], CONST_TWEENDURATION)
                    .SetEase(izy)
                    .From(_rectPosition[poz.x + 1, 3]);

            }
            else
            {
                _replaceTokens[_moveTokenEvenCounter].Vrijednost = _prevTokenVrijednost[poz.x, 0];
                _replaceTokens[_moveTokenEvenCounter].transform.DOMove(_rectPosition[poz.x + 1, 0], CONST_TWEENDURATION)
                    .SetEase(izy)
                    .From(_rectPosition[poz.x + 1, 1]);
            }

        }

        ResetAllHints();
        _moveTokenEvenCounter = (1 + _moveTokenEvenCounter) % 2;
    }

    public Vector2Int OppositePos(Vector2Int moveDirection, Vector2Int currentPosition)
    {
        Vector2Int v2Int = Vector2Int.zero;
        List<int> allPoz = new List<int> { 0, 1, 2 };
        if (moveDirection.x != 0) //horizontal move
        {
            v2Int.x = currentPosition.x;
            allPoz.Remove(currentPosition.y);
            allPoz.Remove(1);
            v2Int.y = allPoz[0];
        }
        else if (moveDirection.y != 0) //vertical move
        {
            v2Int.y = currentPosition.y;
            allPoz.Remove(currentPosition.x);
            allPoz.Remove(1);
            v2Int.x = allPoz[0];
        }

        return v2Int;
    }

    private void RecordPreviousVrijednost()
    {
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                _prevTileVrijednost[i, j] = _tiles[i, j].Vrijednost;
                _prevTokenVrijednost[i, j] = _tokens[i, j].Vrijednost;
                _prevCoordinates[i, j] = _currCoordinates[i, j];
            }
        }
    }

    public void MiddlePointActivation(Vector2Int poz)
    {
        if (_tweenFinishedCounter > 0) return;
        _tweenFinishedCounter = 100;
        _tweenOneHitCheck = false;
        _roundCounter++;

        RecordPreviousVrijednost();
        ResetAllHints();

        Vector2Int greater = LimitVectorInt(poz + Vector2Int.one);
        Vector2Int smaller = LimitVectorInt(poz - Vector2Int.one);

        _tokens[poz.x, greater.y].Vrijednost = _prevTokenVrijednost[smaller.x, poz.y];
        _tokenTransform[poz.x, greater.y].DOMove(_rectPosition[poz.x + 1, greater.y + 1], CONST_TWEENDURATION)
            .From(_rectPosition[smaller.x + 1, poz.y + 1])
            .SetEase(izy)
            .OnComplete(() => EndTweenDrag(true));

        _tokens[smaller.x, poz.y].Vrijednost = _prevTokenVrijednost[poz.x, greater.y];
        _tokenTransform[smaller.x, poz.y].DOMove(_rectPosition[smaller.x + 1, poz.y + 1], CONST_TWEENDURATION)
            .From(_rectPosition[poz.x + 1, greater.y + 1])
            .SetEase(izy);

        _tokens[poz.x, smaller.y].Vrijednost = _prevTokenVrijednost[greater.x, poz.y];
        _tokenTransform[poz.x, smaller.y].DOMove(_rectPosition[poz.x + 1, smaller.y + 1], CONST_TWEENDURATION)
            .From(_rectPosition[greater.x + 1, poz.y + 1])
            .SetEase(izy);

        _tokens[greater.x, poz.y].Vrijednost = _prevTokenVrijednost[poz.x, smaller.y];
        _tokenTransform[greater.x, poz.y].DOMove(_rectPosition[greater.x + 1, poz.y + 1], CONST_TWEENDURATION)
            .From(_rectPosition[poz.x + 1, smaller.y + 1])
            .SetEase(izy);
    }

    private Vector2Int LimitVectorInt(Vector2Int v2Int)
    {
        Vector2Int vToLimit = v2Int;
        if (vToLimit.x > 2) vToLimit.x = 0;
        else if (vToLimit.x < 0) vToLimit.x = 2;
        if (vToLimit.y > 2) vToLimit.y = 0;
        else if (vToLimit.y < 0) vToLimit.y = 2;

        return vToLimit;
    }

    private void EndTweenDrag(bool updateHint)
    {
        if (!_tweenOneHitCheck)
        {
            _tweenOneHitCheck = true;
        }
        else return;
        _tweenFinishedCounter = 0;

        for (int i = 0; i < 9; i++)
        {
            _replaceTiles[i].gameObject.SetActive(false);
            _replaceTokens[i].gameObject.SetActive(false);
        }

        MainHint();

    }
    #endregion

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            PlayerPrefs.Save(); // Save when the app is paused
        }
    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.Save(); // Save when the app is closing
    }
}


