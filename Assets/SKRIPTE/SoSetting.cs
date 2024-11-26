using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

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
    public int skinOrdinal;
    [HideInInspector] public List<Sprite> tileSprites, tokenSprites;
    [SerializeField] List<int> positionsPerLevel;
    [SerializeField] List<int> positionsPerLevelSecondPhase;
    [SerializeField] List<int> positionsPerLevelThirdPhase;

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
