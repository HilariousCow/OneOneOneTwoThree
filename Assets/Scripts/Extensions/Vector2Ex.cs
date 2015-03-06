using UnityEngine;
using System.Collections;

public static class Vector2Ex
{
	public static float Angle360(Vector2 from, Vector2 to)
	{
		float ang = Vector2.Angle(from, to);
		Vector3 cross = Vector3.Cross(from, to);

		if (cross.z > 0)
			ang = 360 - ang;

		return ang;
 	}

	internal static float AngleSigned(Vector2 from, Vector2 to)
	{
		float ang = Vector2.Angle(from, to);
		Vector3 cross = Vector3.Cross(from, to);

		if (cross.z > 0)
			ang = -ang;

		return ang;
	}
}

