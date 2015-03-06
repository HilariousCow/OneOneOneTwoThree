using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour 
{
    private static T _instance;
    public static T Instance
    {

        get
        {
            if(_instance == null)
            {
                _instance = (T)FindObjectOfType(typeof (T));
                if(_instance == null)
                {
                    GameObject go = new GameObject(typeof(T).ToString());
                    go.name = typeof (T).ToString();
                    _instance = go.AddComponent<T>();
                }
                
            }
            return _instance;
        }
    }

    void Awake()
    {
        _instance = (T)FindObjectOfType(typeof (T));
    }

    public static bool Exists()
    {
        return _instance != null;
    }
    public static void ForceExist()
    {
        _instance = (T)FindObjectOfType(typeof(T));
    }
}
