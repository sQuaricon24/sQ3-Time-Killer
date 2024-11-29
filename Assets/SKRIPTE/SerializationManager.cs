using UnityEngine;

public class SerializationManager : MonoBehaviour
{
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


