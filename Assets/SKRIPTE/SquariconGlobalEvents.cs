using System;
using UnityEngine;

public class SquariconGlobalEvents : MonoBehaviour
{
    public static Action OnLevelStarted = null;
    public static Action OnLevelFinished;
    public static Action OnSkinUpdated;

    public static Action OnResetAllHints = null;

    public static Action OnMainHint = null;

    public static Action OnInitializationHint = null;

    public static Action OnScoreUpdated = null;
    public static Action OnGoodMoveHappened = null;
    public static Action OnBadMoveHappened = null;
}
