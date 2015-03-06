using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public static class ImageExtensions  {


    public static void SetSpriteOrDisable(this Image image, Sprite sprite)
    {
        image.sprite = sprite;
        image.gameObject.SetActive(sprite != null);
        //image.SetAllDirty();
    }
	
}
