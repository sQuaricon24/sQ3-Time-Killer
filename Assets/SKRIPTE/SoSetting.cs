using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

[CreateAssetMenu]
public class SoSetting : ScriptableObject
{

    public int level = 1;
    public int score;
    public bool showHints;
    [Header("-=SKINS=-")]
    public int skinOrdinal;
    [HideInInspector] public List<Sprite> tileSprites, tokenSprites;
    [SerializeField] List<LevelPositionPair> positionsPerLevel;

    public int GetPositionForLevel(int level)
    {
        LevelPositionPair p = positionsPerLevel.FirstOrDefault(x => x.levelIndex == level);
        return p.positionValue;
    }
}

[System.Serializable]
public class LevelPositionPair
{
    public int levelIndex;
    public int positionValue;
}
