using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class TransformExtensions
{
    public static void SetAllGameObjectFlags(this Transform trans, HideFlags flags)
    {
        Transform[] transforms = trans.GetComponentsInChildren<Transform>();
        foreach (Transform transform in transforms)
        {
            transform.gameObject.hideFlags = flags;
        }
    }
    public static Bounds WorldBounds<T>(this List<T> listOfThings) where T : MonoBehaviour
    {
        Bounds bounds = new Bounds(listOfThings[0].transform.position, Vector3.zero);
        foreach (T thing in listOfThings)
        {
            bounds.Encapsulate(thing.transform.position);
        }
        return bounds;
    }

    public static Bounds RenderBounds<T>(this List<T> listOfThings) where T : Renderer
    {
        Bounds bounds = listOfThings[0].bounds;
        foreach (T thing in listOfThings)
        {
            bounds.Encapsulate(thing.bounds);
        }
        return bounds;
    }

    public static Bounds RenderBounds(this Transform transform)
    {
        List<Renderer> rends =  new List<Renderer>(transform.GetComponentsInChildren<Renderer>());

        if (rends.Count > 0)
        {
            return rends.RenderBounds();
        }
        else
        {
            return new Bounds(transform.position, Vector3.zero);
        }
    }

    public static void PositionAlongLineCentered<T>(this List<T> things, Vector3 direction, float gap , Vector3 offset) where T :MonoBehaviour
    {
        Dictionary<T, Bounds> eachThingsChildBounds = new  Dictionary<T, Bounds> ();
        float totalLenghOfTheThingsInDirection = 0f;
        foreach (T thing in things)
        {
            Bounds boundsOfThing = thing.transform.RenderBounds();
            eachThingsChildBounds.Add(thing,boundsOfThing);
            totalLenghOfTheThingsInDirection += Vector3.Dot(boundsOfThing.size, direction);
        }

        float totalLengthPlusGaps = totalLenghOfTheThingsInDirection + gap*(things.Count + 1);

        Vector3 localStartPoint = direction*-totalLengthPlusGaps*0.5f;
        float posAlongLine = gap;
        foreach (T thing in things)
        {
            float boundsInDirection = Vector3.Dot(eachThingsChildBounds[thing].size, direction);//something of an assumption about centered objects.
            float halfBoundsInDirection = boundsInDirection*0.5f;

            thing.transform.localPosition = localStartPoint + direction * (posAlongLine + halfBoundsInDirection) + offset;

            posAlongLine += boundsInDirection;
            posAlongLine += gap;
        }

    }

	public static void SetLayer(this Transform trans, int layer) 
	{
		trans.gameObject.layer = layer;
		foreach(Transform child in trans)
			child.SetLayer( layer);
	}

    //Wasn't there a recursive version of this?
    public static T FindChild<T>(this Transform t, string path) where T : Component
    {
        Transform target = t;
        string[] pathArray = path.Split('.');

        for (int index = 0; index < pathArray.Length; index++)
        {
            string p = pathArray[index];
            target = target.FindChild(p);
        }
        T targetComponent = target.GetComponent<T>() as T;


        return targetComponent;
    }
    //Find or inject
    public static T FindChildOrInject<T>(this Transform t, string path) where T : Component
    {
        Transform child = t.FindChild<Transform>(path);
	
        if (child == null)
        {
            Debug.Log("Specified Game Object Not Found... " + path);
            return null;
			 
        }

        T Component = child.GetComponent<T>();
        if (Component == null)
        {
            //Inject and return

            return child.gameObject.AddComponent<T>();
        } else //Already exists
        {
            return Component;
        }
    }

    public static Quaternion LookRotation(this Transform trans, Vector3 targetPos)
    {
        if ((targetPos - trans.position).sqrMagnitude > 0.10f)
        {
            return Quaternion.LookRotation((targetPos - trans.position), trans.up);
        } else
        {
            return Quaternion.identity;
        }
    }

    public static void ResetToParent(this Transform trans)
    {
        if (trans is RectTransform)
        {
            RectTransform uiTransform = trans as RectTransform;
            uiTransform.anchoredPosition3D = Vector3.zero;
            //todo: not sure how to reset this, really?
            //typically, if you're spawning ui elements, assume you want to keep what ever offset they had when you were designing them?
        }
        else
        {
            trans.localPosition = Vector3.zero;
            trans.localRotation = Quaternion.identity;
            trans.localScale = Vector3.one;
        }
    }

    public static T InstantiateChild<T>(this Transform trans, string name/* = null*/)  where T : MonoBehaviour
    {
        GameObject go = new GameObject();
        go.name = (name == null) ? typeof (T).Name : name;
        go.transform.parent = trans;
        go.transform.ResetToParent();
        return go.AddComponent<T>();
    }

    public static T InstantiateChild<T>(this Transform trans, T prefab)  where T : MonoBehaviour
    {
        if(prefab == null)
        {
            Debug.LogError("Tried to spawn null prefab on object " + trans.gameObject.name + " with type: " + typeof(T).ToString() + ". Probably forgot to hook up a prefab in the inspector");
        }
        T instantiatedThing = (Object.Instantiate(prefab.gameObject) as GameObject).GetComponent<T>();

        if (instantiatedThing.transform is RectTransform)
        {
            (instantiatedThing.transform as RectTransform).SetParent(trans, false);    
        }
        else
        {
            instantiatedThing.transform.parent = trans;
        }

        
        instantiatedThing.transform.ResetToParent();
        return instantiatedThing;
    }

	public static GameObject InstantiateChild(this Transform trans, GameObject prefab) 
	{
		GameObject instantiatedThing = (Object.Instantiate(prefab) ) as GameObject;
		instantiatedThing.transform.parent = trans;
		instantiatedThing.transform.ResetToParent();
		return instantiatedThing;
	}
    //Instantiates the given prefab. 
    //Returns the T given if it's there
    //If it's not, it creates it for free and returns that
    public static T InstantiateAndInjectChild<T>(this Transform trans, GameObject prefab)  where T : MonoBehaviour
    {
        GameObject prefabHandle = (Object.Instantiate(prefab) as GameObject);
        T instantiatedThing = prefabHandle.GetComponent<T>();
        if (instantiatedThing == null)
        {
            instantiatedThing = prefabHandle.AddComponent<T>();
        }
        instantiatedThing.transform.parent = trans;
        instantiatedThing.transform.ResetToParent();
        return instantiatedThing;
    }

	public static void SetPosX(this Transform trans, float x)
	{
		var temp = trans.position;
		temp.x = x;
		trans.position = temp;
	}

	public static void SetFlipX(this Transform trans, bool flipped)
	{
		var temp = trans.localScale;
		float magX = Mathf.Abs(temp.x);
		if (flipped)
		{
			temp.x = -magX;
		}
		else
		{
			temp.x = magX;
		}
		trans.localScale = temp;
	}

	public static void SetPosY(this Transform trans, float y)
	{
		var temp = trans.position;
		temp.y = y;
		trans.position = temp;
	}


    //This orders from top left to top right, then goes down a row.
    //todo: optional gap
    //assumes view elements contents are centered?
    //and that rectTransform is centered?
    public static void OrganizeIntoRect<T>(this RectTransform rectTransform, List<T> viewElements) where T : MonoBehaviour
    {
        //
        RectTransform sampleItem = viewElements[0].GetComponent<RectTransform>();

        float itemWidth = sampleItem.rect.width;
        float itemHeight = sampleItem.rect.height;

        int numCols = Mathf.FloorToInt(rectTransform.rect.width/itemWidth);
        for (int i = 0; i < viewElements.Count; i++)
        {
            int x = i%numCols;
            int y = i/numCols;
            Vector2 posFromTopLeft = new Vector2( x *itemWidth, y*itemHeight  );
            RectTransform itemRect = viewElements[i].GetComponent<RectTransform>();
            itemRect.SetParent(rectTransform);
            itemRect.anchoredPosition = posFromTopLeft;


        }

    }
}
