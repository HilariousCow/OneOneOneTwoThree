using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Singleton<T, L> : MonoBehaviour where T : MonoBehaviour where L : MonoBehaviour
{
    public delegate void ManagedObjectEvent(L obj);
    public static ManagedObjectEvent EventAdded = delegate { };
    public static ManagedObjectEvent EventRemoved = delegate { };

    private static T _instance;

    private List<L> _listOfObjects;
    public List<L> ListOfObjects
    {
        get
        {
            if (_listOfObjects == null)
            {
                _listOfObjects = new List<L>();
            }
            return _listOfObjects;
        }
        set { _listOfObjects = value; }
    }

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
                    _instance = go.AddComponent<T>();
                    
                }
                
            }
            return _instance;
        }
    }

    public virtual void Add(L obj)
    {
        if (!ListOfObjects.Contains(obj))
        {
            ListOfObjects.Add(obj);
            EventAdded.Invoke(obj);
        }
    }

    public virtual void Remove(L obj)
    {
        if (!ListOfObjects.Contains(obj))
        {
            ListOfObjects.Remove(obj);
            EventRemoved.Invoke(obj);
        }
    }

}
