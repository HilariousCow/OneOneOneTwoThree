using UnityEngine;
using System.Collections;


public static class Matrix4x4Extensions {
	
	
	public static void setPosition(this Matrix4x4 mat, Vector3 pos)
	{
	 
		mat.SetTRS(pos, mat.rotation(), mat.scale());
		
	}
	public static Vector3 position(this Matrix4x4 mat)
	{
	 
		return mat.MultiplyPoint3x4(Vector3.zero); 
		
	}
	
	public static Vector3 scale(this Matrix4x4 mat)
	{
	 
		return new Vector3( mat.m00, mat.m11, mat.m33);
		
	}
	
	public static Vector3 up(this Matrix4x4 mat)
	{
	 
		return mat.MultiplyVector(Vector3.up); 
		
	}
	
	public static Vector3 right(this Matrix4x4 mat)
	{
	 
		return mat.MultiplyVector(Vector3.right); 
		
	}
	
	public static Vector3 forward(this Matrix4x4 mat)
	{
	 
		return mat.MultiplyVector(Vector3.forward); 
		
	}
	
	public static Quaternion rotation(this Matrix4x4 mat)
	{
	 
		return Quaternion.LookRotation(mat.forward(), mat.up());
		
	}
	
	
	public static void setRotation(this Matrix4x4 mat, Quaternion rot)
	{
	 
		mat = Matrix4x4.TRS(mat.position(), rot, mat.scale());
		
	}

	
	
	
}
