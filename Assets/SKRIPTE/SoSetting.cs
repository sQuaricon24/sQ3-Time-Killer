using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Playables;

[CreateAssetMenu]
public class SoSetting : ScriptableObject
{
    public int firstPhaseScore = 20;
    public int secondPhaseScore = 40;
    public int level = 1;
    public int goodMoveStreak = 0;
    public int score;
    public int xp;
    public bool showHints;
    [Header("-=SKINS=-")]
    public int CurrentSkinIndex;
    [HideInInspector] public List<Sprite> tileSprites, tokenSprites;
    [SerializeField] List<int> positionsPerLevel;
    [SerializeField] List<int> positionsPerLevelSecondPhase;
    [SerializeField] List<int> positionsPerLevelThirdPhase;
    [SerializeField] private bool isAdventureMode = false;
    public bool IsAdventureMode => isAdventureMode;

    private const string FILE_PATH = "ScriptableObjects/Settings";
    private static SoSetting instance;
    public static SoSetting Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Resources.Load<SoSetting>(FILE_PATH);

                if (instance == null)
                {
                    Debug.LogError($"No instance of {typeof(SoSetting).Name} found in Resources.");
                }
            }
            return instance;
        }
    }

    public void SetAdventureMode(bool isAdvMode)
    {
        isAdventureMode = isAdvMode;
    }

    public void AddScore(int addedScore)
    {
        int newScore = score + addedScore;
        if((score < firstPhaseScore && newScore >= firstPhaseScore) ||
            (score < secondPhaseScore && newScore >= secondPhaseScore))
        {
            level = -1;
            goodMoveStreak = 0;
        }

        score = newScore;
    }

    public void LoadSecondAndThirdPhasePositions()
    {
        positionsPerLevelSecondPhase = LoadPositionsFromCSV("CSV/secondPhase");
        positionsPerLevelThirdPhase = LoadPositionsFromCSV("CSV/thirdPhase");
    }

    public int GetPositionForLevelAndScore(int level, int score)
    {
        //Debug.LogError("Returning level " + level + " for score " + score);

        if (score > secondPhaseScore)
        {
            return positionsPerLevelThirdPhase[level];
        }
        else if (score > firstPhaseScore)
        {
            return positionsPerLevelSecondPhase[level];
        }
        else
        {
            return positionsPerLevel[level];
        }
    }

    private List<int> LoadPositionsFromCSV(string path)
    {
        TextAsset csvFile = Resources.Load<TextAsset>(path);
        if (csvFile == null)
        {
            Debug.LogError($"CSV file not found at: {path}");
            return new List<int>();
        }

        string[] lines = csvFile.text.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
        List<int> positions = new List<int>();

        foreach (string line in lines)
        {
            string[] values = line.Split(',');
            foreach (string value in values)
            {
                if (int.TryParse(value, out int number))
                {
                    positions.Add(number);
                }
            }
        }

        return positions;
    }
}
