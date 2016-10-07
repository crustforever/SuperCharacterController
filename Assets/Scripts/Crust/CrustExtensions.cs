using System;
using UnityEngine;

namespace AssemblyCSharp
{
	public static class CrustExtensions
	{
		/// <summary>
		/// Rotates the 3D forward vector of the given transform into the XZ plane and returns just those components giving a 2D top-down forward vector
		/// </summary>
		public static Vector2 TopDownForward(this Transform t)
		{
			float angle = Vector3.Angle(t.up, Vector3.up);
			Vector3 perp = Vector3.Cross(t.up, Vector3.up);
			Vector3 rotated = Quaternion.AngleAxis(angle, perp) * t.forward;
			return new Vector2(rotated.x, rotated.z).normalized;
		}
	}
}