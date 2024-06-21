using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefsUtils : MonoBehaviour
{
    public static void SaveInt(string key, int number)
    {
        PlayerPrefs.SetInt(key, number);
        PlayerPrefs.Save();
    }

    public static int GetInt(string key)
    {
        return PlayerPrefs.GetInt(key, 0);
    }
}
