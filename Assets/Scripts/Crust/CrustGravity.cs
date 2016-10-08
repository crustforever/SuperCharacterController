using UnityEngine;
using System.Collections;

namespace AssemblyCSharp
{
	/// <summary>
	/// Rotates this transform to align it towards the target transform's position
	/// </summary>
	public class CrustGravity : MonoBehaviour
	{
		public Transform Target;

		void Update()
		{
			Vector3 dir = (this.transform.position - this.Target.position).normalized;
			this.transform.rotation = Quaternion.FromToRotation(transform.up, dir) * transform.rotation;

			//HACK HACK HACK
			Camera.main.transform.rotation = Quaternion.FromToRotation(Camera.main.transform.up, dir) * Camera.main.transform.rotation;
		}
	}
}