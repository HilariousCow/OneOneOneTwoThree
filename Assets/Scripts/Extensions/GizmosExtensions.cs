using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class GizmosExtensions {

    static List<Color> GizmoColorStack = new List<Color>();
 
    public static void PushColor(Color col)
    {
        GizmoColorStack.Push(Gizmos.color);
        Gizmos.color = col;
    }

    public static void PopColor(Color col)
    {
        Gizmos.color = GizmoColorStack.Pop();
    }
}
