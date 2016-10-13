using System;
using UnityEngine;

namespace AssemblyCSharp
{
	public class CrustCharacterInput : MonoBehaviour
	{
		public CrustInput Current;
		public Vector2 LeftStickDebug;

		void Update()
		{
			Vector2 leftAxis = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

			if (LeftStickDebug != Vector2.zero)
				leftAxis = LeftStickDebug;

			bool jumpInput = Input.GetButtonDown("Jump");

			Current = new CrustInput()
			{
				LeftAxis = leftAxis,
				Jump = jumpInput
			};
		}
	}

	public struct CrustInput
	{
		public Vector2 LeftAxis;
		public bool Jump;
	}
}