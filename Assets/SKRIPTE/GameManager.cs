using System.Collections.Generic;
using UnityEngine;
using FirstCollection;
using DG.Tweening;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine.UI;

public partial class GameManager : MonoBehaviour
{
    private const float CONST_TWEENDURATION = 1f;

    public static GameManager Instance;
    // temporary solution to check if token is on correct place, list index values represent the board
    [SerializeField] private List<Sprite> correctSpriteList;

    [SerializeField] Transform kanvas;
    [SerializeField] private Button undoBtn;
    [SerializeField] private Button redoBtn;
    [SerializeField] Transform parTile, parToken, parPositions, parTileReplace, parTokenReplace;
    [SerializeField] List<int> movesToWinList;
    [SerializeField] private Ease easingType;

    private Tile[,] tiles = new Tile[3, 3];
    public static Transform[,] TileTransforms = new Transform[3, 3];
    public static Token[,] Tokens = new Token[3, 3];
    private Transform[,] tokenTransforms = new Transform[3, 3];
    private Vector2[,] rectPositions = new Vector2[5, 5]; //position is the same for tiles and tokens. Used for animations.
    private int[,] prevTileValues = new int[3, 3]; //buffer values
    private int[,] prevTokenValues = new int[3, 3]; //buffer values
    private Token[] substituteTokens; //used for animation only
    private Tile[] substituteTiles; //used for animation only
    private int moveTokenEvenCounter; //token are moved twice per player action. Some values shouldn't be updated twice and this bool makes sure of that.

    private int currentMovesToWin = -1;
    private int currentPositionIndex = -1;
    private int previousPositionIndex = -1;
    private int PreviousPositionIndex
    {
        get { return previousPositionIndex; }
        set
        {
            previousPositionIndex = value;
            if(previousPositionIndex == CurrentPositionIndex)
            {
                undoBtn.gameObject.SetActive(false);
                redoBtn.gameObject.SetActive(false);
            }
            else
            {
                undoBtn.gameObject.SetActive(true);
            }
        }
    }

    private int CurrentPositionIndex
    {
        get { return currentPositionIndex; }
        set
        {
            currentPositionIndex = value;
            if (previousPositionIndex == currentPositionIndex || currentPositionIndex == 0)
            {
                undoBtn.gameObject.SetActive(false);
                redoBtn.gameObject.SetActive(false);
            }
            else
            {
                undoBtn.gameObject.SetActive(true);
            }
        }
    }

    private int tweenFinishedCounter = 0; // inputs (drags) are disabled until tween finishes. Int is used instead of bool beacuse MoveToken is called twice per drag.
    private bool isTweenOneHit = true; // tween are called 6+ times and they all OnComplete(EndTween). That method should be called once, not 6 times.

    public static Vector2Int[,] CurrCoordinates = new Vector2Int[3, 3];
    private Vector2Int[,] prevCoordinates = new Vector2Int[3, 3];
    readonly Dictionary<int[], int[]> mainPairs = new Dictionary<int[], int[]>();
    readonly List<int[]> allCombinations = new List<int[]>();

    #region HARDCODED LAYOUTS
    public static readonly int[] layNull = new int[4] { 0, 0, 0, 0 };
    public static readonly int[] lay0 = new int[4] { 1, 2, 3, 4 };
    public static readonly int[] lay1 = new int[4] { 1, 2, 4, 3 };
    public static readonly int[] lay2 = new int[4] { 1, 3, 2, 4 };
    public static readonly int[] lay3 = new int[4] { 1, 3, 4, 2 };
    public static readonly int[] lay4 = new int[4] { 1, 4, 2, 3 };
    public static readonly int[] lay5 = new int[4] { 1, 4, 3, 2 };
    public static readonly int[] lay6 = new int[4] { 2, 1, 3, 4 };
    public static readonly int[] lay7 = new int[4] { 2, 1, 4, 3 };
    public static readonly int[] lay8 = new int[4] { 2, 3, 1, 4 };
    public static readonly int[] lay9 = new int[4] { 2, 3, 4, 1 };
    public static readonly int[] lay10 = new int[4] { 2, 4, 1, 3 };
    public static readonly int[] lay11 = new int[4] { 2, 4, 3, 1 };
    public static readonly int[] lay12 = new int[4] { 3, 1, 2, 4 };
    public static readonly int[] lay13 = new int[4] { 3, 1, 4, 2 };
    public static readonly int[] lay14 = new int[4] { 3, 2, 1, 4 };
    public static readonly int[] lay15 = new int[4] { 3, 2, 4, 1 };
    public static readonly int[] lay16 = new int[4] { 3, 4, 1, 2 };
    public static readonly int[] lay17 = new int[4] { 3, 4, 2, 1 };
    public static readonly int[] lay18 = new int[4] { 4, 1, 2, 3 };
    public static readonly int[] lay19 = new int[4] { 4, 1, 3, 2 };
    public static readonly int[] lay20 = new int[4] { 4, 2, 1, 3 };
    public static readonly int[] lay21 = new int[4] { 4, 2, 3, 1 };
    public static readonly int[] lay22 = new int[4] { 4, 3, 1, 2 };
    public static readonly int[] lay23 = new int[4] { 4, 3, 2, 1 };
    #endregion

    

    private void Awake()
    {
        Instance = this;
        undoBtn.gameObject.SetActive(false);
        redoBtn.gameObject.SetActive(false);
        undoBtn.onClick.AddListener(() => HandleUndoClick());
        redoBtn.onClick.AddListener(() => HandleRedoClick());
    }

    private void Start()
    {
        NewGameBoard();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) SceneManager.LoadScene(gameObject.scene.name);
        else if (Input.GetKeyDown(KeyCode.Escape)) SceneManager.LoadScene(0);
    }

    #region //INITIALIZATON

    private void NewGameBoard()
    {
        SquariconGlobalEvents.OnLevelStarted?.Invoke();
     

        substituteTiles = HelperScript.GetAllChildernByType<Tile>(parTileReplace);
        substituteTokens = HelperScript.GetAllChildernByType<Token>(parTokenReplace);
        InitializationMain(MainType.Tile);
        InitializationMain(MainType.Token);

        IniDic();

        // current position from 23 possible positions
        CurrentPositionIndex = SoSetting.Instance.GetPositionForLevelAndScore(SoSetting.Instance.level, SoSetting.Instance.score);
        PreviousPositionIndex = CurrentPositionIndex;

        Tokens[1, 0].Value = Tokens[1, 2].Value = allCombinations[currentPositionIndex][0];
        Tokens[2, 0].Value = Tokens[0, 2].Value = allCombinations[currentPositionIndex][1];
        Tokens[0, 0].Value = Tokens[2, 2].Value = allCombinations[currentPositionIndex][2];
        Tokens[0, 1].Value = Tokens[2, 1].Value = allCombinations[currentPositionIndex][3];
        currentMovesToWin = GetMovesToWinForPositionIndex(currentPositionIndex);
        if (SoSetting.Instance.IsAdventureMode)
        {
            AnimateTokensAppearOrDissapear();
        }
        SquariconGlobalEvents.OnInitializationHint?.Invoke();
    }

    private int GetPositionIndexFromCurrentVrijednost()
    {
        for(int i = 0; i < allCombinations.Count; i++)
        {
            if(Tokens[1, 0].Value == allCombinations[i][0] &&
               Tokens[2, 0].Value == allCombinations[i][1] &&
               Tokens[0, 0].Value == allCombinations[i][2] &&
               Tokens[0, 1].Value == allCombinations[i][3])
            {
                return i;
            }
        }

        Debug.LogError("GetPositionIndexFromCurrentVrijednost returning invalid index -1");
        return -1;
    }

    private int GetMovesToWinForPositionIndex(int positionIndex)
    {
        return movesToWinList[positionIndex];
    }

    private void HandleUndoClick()
    {
        if (isDragActive)
            return;

        SoSetting.Instance.goodMoveStreak = 0;
        if (PreviousPositionIndex != CurrentPositionIndex)
        {
            if (doAnimateUndoRedo)
            {
                if (isLatestActionMiddlePointActivation)
                {
                    MiddlePointActivation(new Vector2Int(1, 1));
                    HandleMoveFinished();
                }
                else
                    SimulateReverseDrag(previousMoveDir, previousPoz);
            }
            else
            {
                // Swap the current and previous indices
                int temp = CurrentPositionIndex;
                CurrentPositionIndex = PreviousPositionIndex;
                PreviousPositionIndex = temp;

                UpdateBoardToCurrentPosition();
                SquariconGlobalEvents.OnResetAllHints?.Invoke();
                //bool justCheckIfLevelDone = false;
                //SquariconGlobalEvents.OnMainHint?.Invoke(justCheckIfLevelDone);
            }

            redoBtn.gameObject.SetActive(true);
            undoBtn.gameObject.SetActive(false);

            // not sure if undo triggers good move so reseting it here to 0 again
            SoSetting.Instance.goodMoveStreak = 0;
        }
        else
        {
            Debug.LogWarning("No move to undo.");
        }
    }

    private bool doAnimateUndoRedo = true;

    private void HandleRedoClick()
    {
        if (isDragActive)
            return;

        SoSetting.Instance.goodMoveStreak = 0;
        if (PreviousPositionIndex != CurrentPositionIndex)
        {
            // Update the tokens and moves based on the restored current position
            if(doAnimateUndoRedo)
            {
                if (isLatestActionMiddlePointActivation)
                {
                    MiddlePointActivation(new Vector2Int(1, 1));
                    HandleMoveFinished();
                }
                else
                    SimulateReverseDrag(previousMoveDir, previousPoz);
            }
            else
            {
                // Swap the current and previous indices back
                int temp = PreviousPositionIndex;
                PreviousPositionIndex = CurrentPositionIndex;
                CurrentPositionIndex = temp;

                UpdateBoardToCurrentPosition();
                SquariconGlobalEvents.OnResetAllHints?.Invoke();
               //bool justCheckIfLevelDone = false;
                //SquariconGlobalEvents.OnMainHint?.Invoke(justCheckIfLevelDone);
            }
            Debug.Log("Redo successful. Current position: " + currentPositionIndex);

            redoBtn.gameObject.SetActive(false);
            undoBtn.gameObject.SetActive(true);
            // not sure if redo triggers good move so reseting it here to 0 again
            SoSetting.Instance.goodMoveStreak = 0;
        }
        else
        {
            Debug.LogWarning("No move to redo.");
        }
    }

    private void SimulateReverseDrag(Vector2Int moveDir, Vector2Int poz)
    {
        MoveTokenByPosition(moveDir, poz);

        Vector2Int oppositePosition = OppositePos(moveDir, poz);

        MoveTokenByPosition(-moveDir, oppositePosition);

        HandleMoveFinished();
    }

    private void UpdateBoardToCurrentPosition()
    {
        // Updates the token values based on the currentPositionIndex
        Tokens[1, 0].Value = Tokens[1, 2].Value = allCombinations[currentPositionIndex][0];
        Tokens[2, 0].Value = Tokens[0, 2].Value = allCombinations[currentPositionIndex][1];
        Tokens[0, 0].Value = Tokens[2, 2].Value = allCombinations[currentPositionIndex][2];
        Tokens[0, 1].Value = Tokens[2, 1].Value = allCombinations[currentPositionIndex][3];

        // Update moves left to win
        currentMovesToWin = GetMovesToWinForPositionIndex(currentPositionIndex);

        // Trigger animations or visual updates if needed
        if (SoSetting.Instance.IsAdventureMode)
        {
            AnimateTokensAppearOrDissapear();
        }

        // Notify other systems about the state change
        SquariconGlobalEvents.OnScoreUpdated?.Invoke();
    }

    public void HandleMoveFinished()
    {
        int newPosition = GetPositionIndexFromCurrentVrijednost();
        int newMovesToWin = GetMovesToWinForPositionIndex(newPosition);

        PreviousPositionIndex = CurrentPositionIndex;
        CurrentPositionIndex = newPosition;

        //DebugCurrentAndPreviousPosition();


        if (newMovesToWin < currentMovesToWin)
        {
            SoSetting.Instance.goodMoveStreak++;
            SoSetting.Instance.xp += GetXpForWinStreak(SoSetting.Instance.goodMoveStreak);
            SoSetting.Instance.AddScore(GetScoreForWinStreak(SoSetting.Instance.goodMoveStreak));
            PlayerPrefs.SetInt("XP", SoSetting.Instance.xp);

            SquariconGlobalEvents.OnScoreUpdated?.Invoke();
            SquariconGlobalEvents.OnGoodMoveHappened?.Invoke();
        }
        else
        {
            SoSetting.Instance.goodMoveStreak = 0;
            SquariconGlobalEvents.OnBadMoveHappened?.Invoke();
        }

        currentMovesToWin = newMovesToWin;

        if(SoSetting.Instance.IsAdventureMode)
        {
            AnimateTokensAppearOrDissapear();
        }
    }

    private int GetXpForWinStreak(int streak)
    {
        switch (streak)
        {
            case 0: return 0;
            case 1: return 5;
            case 2: return 10;
            case 3: return 20;
            case 4: return 40;
            case 5: return 80;
            default: return 80;
        }
    }

    private int GetScoreForWinStreak(int streak)
    {
        switch (streak)
        {
            case 0: return 0;
            case 1: return 0;
            case 2: return 10;
            case 3: return 20;
            case 4: return 40;
            case 5: return 80;
            default: return 80;
        }
    }

    private void AnimateTokensAppearOrDissapear()
    {
        //Debug.LogError("TOKEN 0,0: " + Tokens[0, 0].ImageName);
        
        if (Tokens[0, 0].ImageName != correctSpriteList[0].name)
        {
            Tokens[0, 0].FadeIn();
            Tokens[2, 2].FadeIn();
        }
        else
        {
            Tokens[0, 0].FadeOut();
            Tokens[2, 2].FadeOut();
        }
        ////////////////////////////////////////////////////////////////////////////////////////
        if (Tokens[1, 0].ImageName != correctSpriteList[1].name)
        {
            Tokens[1, 0].FadeIn();
            Tokens[1, 2].FadeIn();
        }
        else
        {
            Tokens[1, 0].FadeOut();
            Tokens[1, 2].FadeOut();
        }
        ////////////////////////////////////////////////////////////////////////////////////////
        if (Tokens[2, 0].ImageName != correctSpriteList[2].name)
        {
            Tokens[2, 0].FadeIn();
            Tokens[0, 2].FadeIn();
        }
        else
        {
            Tokens[2, 0].FadeOut();
            Tokens[0, 2].FadeOut();
        }

        ////////////////////////////////////////////////////////////////////////////////////////
        if (Tokens[0, 1].ImageName != correctSpriteList[3].name)
        {
            Tokens[0, 1].FadeIn();
            Tokens[2, 1].FadeIn();
        }
        else
        {
            Tokens[0, 1].FadeOut();
            Tokens[2, 1].FadeOut();
        }
    }

    private void DebugCurrentAndPreviousPosition()
    {
        /*
        Tokens[1, 0].Value = Tokens[1, 2].Value = allCombinations[currentPositionIndex][0];
        Tokens[2, 0].Value = Tokens[0, 2].Value = allCombinations[currentPositionIndex][1];
        Tokens[0, 0].Value = Tokens[2, 2].Value = allCombinations[currentPositionIndex][2];
        Tokens[0, 1].Value = Tokens[2, 1].Value = allCombinations[currentPositionIndex][3];*/

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
        Debug.LogError("{" + allCombinations[positionIndex][0] + "," 
                           + allCombinations[positionIndex][1] + ","
                           + allCombinations[positionIndex][2] + ","
                           + allCombinations[positionIndex][3] + "}");

        Debug.LogError("MOVES TO WIN: " + movesToWinList[positionIndex]);
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
                    tiles[i, j] = parTile.GetChild(counter).GetComponent<Tile>();
                    tiles[i, j].MyPosition = new Vector2Int(i, j);
                    TileTransforms[i, j] = tiles[i, j].transform;
                }
                else
                {
                    Tokens[i, j] = parToken.GetChild(counter).GetComponent<Token>();
                    Tokens[i, j].MyPosition = new Vector2Int(i, j);
                    tokenTransforms[i, j] = Tokens[i, j].transform;
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
                    rectPositions[i, j] = parPositions.GetChild(counter).position;
                    counter++;
                }
            }
        }
    }
    #endregion

    private void IniDic()
    {
        allCombinations.Add(lay0);
        allCombinations.Add(lay1);
        allCombinations.Add(lay2);
        allCombinations.Add(lay3);
        allCombinations.Add(lay4);
        allCombinations.Add(lay5);
        allCombinations.Add(lay6);
        allCombinations.Add(lay7);
        allCombinations.Add(lay8);
        allCombinations.Add(lay9);
        allCombinations.Add(lay10);
        allCombinations.Add(lay11);
        allCombinations.Add(lay12);
        allCombinations.Add(lay13);
        allCombinations.Add(lay14);
        allCombinations.Add(lay15);
        allCombinations.Add(lay16);
        allCombinations.Add(lay17);
        allCombinations.Add(lay18);
        allCombinations.Add(lay19);
        allCombinations.Add(lay20);
        allCombinations.Add(lay21);
        allCombinations.Add(lay22);
        allCombinations.Add(lay23);


        mainPairs.Add(lay0, layNull);
        mainPairs.Add(lay1, lay15);
        mainPairs.Add(lay2, lay14);
        mainPairs.Add(lay3, lay0);
        mainPairs.Add(lay4, lay0);
        mainPairs.Add(lay5, lay11);
        mainPairs.Add(lay6, lay19);
        mainPairs.Add(lay7, lay8);
        mainPairs.Add(lay8, lay0);
        mainPairs.Add(lay9, lay2);
        mainPairs.Add(lay10, lay18);
        mainPairs.Add(lay11, lay8);
        mainPairs.Add(lay12, lay0);
        mainPairs.Add(lay13, lay7);
        mainPairs.Add(lay14, lay17);
        mainPairs.Add(lay15, lay12);
        mainPairs.Add(lay16, lay2);
        mainPairs.Add(lay17, lay21);
        mainPairs.Add(lay18, lay12);
        mainPairs.Add(lay19, lay3);
        mainPairs.Add(lay20, lay4);
        mainPairs.Add(lay21, lay0);
        mainPairs.Add(lay22, lay8);
        mainPairs.Add(lay23, lay19);
    }

    private Vector2Int previousMoveDir;
    private Vector2Int previousPoz;

    private Vector2Int currentMoveDir;
    private Vector2Int currentPoz;

    //#region//MAIN MECHANICS
    public void MoveTokenByPosition(Vector2Int moveDir, Vector2Int poz)
    {
        if (tweenFinishedCounter > 1) return;

        isLatestActionMiddlePointActivation = false;
        previousMoveDir = currentMoveDir * -1;
        previousPoz = currentPoz;
        currentMoveDir = moveDir;
        currentPoz = poz;
        isDragActive = true;

        tweenFinishedCounter++;
        isTweenOneHit = false;
        RecordPreviousValue();

        substituteTokens[moveTokenEvenCounter].gameObject.SetActive(true);

        if (moveDir.x != 0) //horizontal move
        {
            int dirToken = moveDir.x == 1 ? 2 : 1;
            int dirTransform = moveDir.x == 1 ? 0 : 2;
            for (int i = 0; i < 3; i++)
            {
                Tokens[i, poz.y].Value = prevTokenValues[(i + dirToken) % 3, poz.y];
                tokenTransforms[i, poz.y].DOMove(rectPositions[i + 1, poz.y + 1], CONST_TWEENDURATION)
                    .From(rectPositions[i + dirTransform, poz.y + 1])
                    .SetEase(easingType)
                    .OnComplete(() => EndTweenDrag());
            }

            //edge tokens, needed for tween animation only
            if (moveDir.x == 1)
            {
                substituteTokens[moveTokenEvenCounter].Value = prevTokenValues[2, poz.y];
                substituteTokens[moveTokenEvenCounter].transform.DOMove(rectPositions[4, poz.y + 1], CONST_TWEENDURATION)
                    .SetEase(easingType)
                    .From(rectPositions[3, poz.y + 1]);
            }
            else
            {
                substituteTokens[moveTokenEvenCounter].Value = prevTokenValues[0, poz.y];
                substituteTokens[moveTokenEvenCounter].transform.DOMove(rectPositions[0, poz.y + 1], CONST_TWEENDURATION)
                    .SetEase(easingType)
                    .From(rectPositions[1, poz.y + 1]);
            }
        }
        else if (moveDir.y != 0) //vertical move
        {
            int dirToken = moveDir.y == 1 ? 2 : 1;
            int dirTransform = moveDir.y == 1 ? 0 : 2;
            for (int j = 0; j < 3; j++)
            {
                Tokens[poz.x, j].Value = prevTokenValues[poz.x, (j + dirToken) % 3];
                tokenTransforms[poz.x, j].DOMove(rectPositions[poz.x + 1, j + 1], CONST_TWEENDURATION)
                    .From(rectPositions[poz.x + 1, j + dirTransform])
                    .SetEase(easingType)
                    .OnComplete(() => EndTweenDrag());
            }

            //edge tokens, needed for tween animation only
            if (moveDir.y == 1)
            {
                substituteTokens[moveTokenEvenCounter].Value = prevTokenValues[poz.x, 2];
                substituteTokens[moveTokenEvenCounter].transform.DOMove(rectPositions[poz.x + 1, 4], CONST_TWEENDURATION)
                    .SetEase(easingType)
                    .From(rectPositions[poz.x + 1, 3]);

            }
            else
            {
                substituteTokens[moveTokenEvenCounter].Value = prevTokenValues[poz.x, 0];
                substituteTokens[moveTokenEvenCounter].transform.DOMove(rectPositions[poz.x + 1, 0], CONST_TWEENDURATION)
                    .SetEase(easingType)
                    .From(rectPositions[poz.x + 1, 1]);
            }

        }

        SquariconGlobalEvents.OnResetAllHints?.Invoke();
        moveTokenEvenCounter = (1 + moveTokenEvenCounter) % 2;
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

    public void HandleResetProgresClick()
    {
        SoSetting.Instance.score = 0;
        SoSetting.Instance.xp = 0;
        SoSetting.Instance.goodMoveStreak = 0;
        SquariconGlobalEvents.OnScoreUpdated();
    }

    private void RecordPreviousValue()
    {
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                prevTileValues[i, j] = tiles[i, j].TileSpriteIndex;
                prevTokenValues[i, j] = Tokens[i, j].Value;
                prevCoordinates[i, j] = CurrCoordinates[i, j];
            }
        }
    }

    private bool isLatestActionMiddlePointActivation = false;
    public void MiddlePointActivation(Vector2Int poz)
    {
        if (tweenFinishedCounter > 0) return;
        tweenFinishedCounter = 100;
        isTweenOneHit = false;

        isLatestActionMiddlePointActivation = true;
        previousPoz = currentPoz;
        currentPoz = poz;
        isDragActive = true;

        RecordPreviousValue();
        SquariconGlobalEvents.OnResetAllHints?.Invoke();

        Vector2Int greater = LimitVectorInt(poz + Vector2Int.one);
        Vector2Int smaller = LimitVectorInt(poz - Vector2Int.one);

        Tokens[poz.x, greater.y].Value = prevTokenValues[smaller.x, poz.y];
        tokenTransforms[poz.x, greater.y].DOMove(rectPositions[poz.x + 1, greater.y + 1], CONST_TWEENDURATION)
            .From(rectPositions[smaller.x + 1, poz.y + 1])
            .SetEase(easingType)
            .OnComplete(() => EndTweenDrag());

        Tokens[smaller.x, poz.y].Value = prevTokenValues[poz.x, greater.y];
        tokenTransforms[smaller.x, poz.y].DOMove(rectPositions[smaller.x + 1, poz.y + 1], CONST_TWEENDURATION)
            .From(rectPositions[poz.x + 1, greater.y + 1])
            .SetEase(easingType);

        Tokens[poz.x, smaller.y].Value = prevTokenValues[greater.x, poz.y];
        tokenTransforms[poz.x, smaller.y].DOMove(rectPositions[poz.x + 1, smaller.y + 1], CONST_TWEENDURATION)
            .From(rectPositions[greater.x + 1, poz.y + 1])
            .SetEase(easingType);

        Tokens[greater.x, poz.y].Value = prevTokenValues[poz.x, smaller.y];
        tokenTransforms[greater.x, poz.y].DOMove(rectPositions[greater.x + 1, poz.y + 1], CONST_TWEENDURATION)
            .From(rectPositions[poz.x + 1, smaller.y + 1])
            .SetEase(easingType);
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

    private bool isDragActive = false;
    private void EndTweenDrag()
    {
        if (!isTweenOneHit)
        {
            isTweenOneHit = true;
        }
        else return;
        tweenFinishedCounter = 0;

        for (int i = 0; i < 9; i++)
        {
            substituteTiles[i].gameObject.SetActive(false);
            substituteTokens[i].gameObject.SetActive(false);
        }

        isDragActive = false;

        // due to extremely bad design and coupling we need to use Hint logic do determine if level is done and trigger level completion proces
        bool justCheckIfLevelDone = true;
        SquariconGlobalEvents.OnMainHint?.Invoke(justCheckIfLevelDone);
    }
}