using UnityEngine;
using System.Collections;

public static class Vector3Extensions {
	
    //note: may be wrong. use flatz
	public static Vector3 Flattened(this Vector3 vec, Vector3 planeNormal)
	{
		return Vector3.ProjectOnPlane(Vector3.right, planeNormal);
	}
	
	public static Vector3 FlatX(this Vector3 vec)
	{
        return Vector3.ProjectOnPlane(vec, Vector3.right);
	}
	
	public static Vector3 FlatY(this Vector3 vec)
	{
        return Vector3.ProjectOnPlane(vec, Vector3.up);
	}
	
	public static Vector3 FlatZ(this Vector3 vec)
	{
        return Vector3.ProjectOnPlane(vec, Vector3.forward);
	}
	
	public static Vector3 SinCosX(this Vector3 vec, float frac)
	{
		float twopi = frac * Mathf.PI * 2.0f;
		return new Vector3(0.0f, Mathf.Sin(twopi), Mathf.Cos(twopi));
	}
	
	public static Vector3 SinCosY(this Vector3 vec, float frac)
	{
	float twopi = frac * Mathf.PI * 2.0f;
		return new Vector3( Mathf.Sin(twopi), 0.0f, Mathf.Cos(twopi));
	}
	
	public static Vector3 SinCosZ(this Vector3 vec, float frac)
	{
			float twopi = frac * Mathf.PI * 2.0f;
		return new Vector3( Mathf.Sin(twopi),  Mathf.Cos(twopi), 0.0f);
	}
	
	public static Vector2 Cross(this Vector2 crosser)
	{
		return new Vector2(crosser.y, -crosser.x);
	}
	
	public static Vector2 XY(this Vector3 vec)
	{
		return new Vector2(vec.x, vec.y);
	}
	
	public static Vector2 XZ(this Vector3 vec)
	{
		return new Vector2(vec.x, vec.z);
	}

    public static Vector2 NormalizedOrZero(this Vector2 vec)
    {
        if (vec == Vector2.zero)
        {
            return Vector2.zero;
        }
        else return vec.normalized;
    }

    public static Vector3 NormalizedOrZero(this Vector3 vec)
    {
        if (vec == Vector3.zero)
        {
            return Vector3.zero;
        }
        else return vec.normalized;
    }

	public static bool RayIntersectionPoint(this Plane plane, Ray ray, out Vector3 intersectionPos)
	{
		float endDist;
		intersectionPos = Vector3.zero;
		
		if(plane.Raycast(ray, out endDist) )
		{
			intersectionPos = ray.origin + ray.direction * endDist;
			return true;
		}
		else
		{
			return false;
		}
	}
	
    public static Vector4 ToVector4(this Vector3 vec, float w)
    {
        return new Vector4(vec.x, vec.y, vec.z, w);
    }


}
