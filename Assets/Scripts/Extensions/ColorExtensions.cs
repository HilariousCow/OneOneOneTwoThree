using UnityEngine;
using System.Collections;

public static class ColorExtensions  {

    public static Color Transparency(this Color rgb, float Transparency)
    {
        return new Color(rgb.r, rgb.g, rgb.b, Transparency);
    }
}
