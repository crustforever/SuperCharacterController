using System;
using UnityEngine;

namespace AssemblyCSharp
{
	public class CrustCharacterInput : MonoBehaviour
	{
		public CrustInput Current;
//		public Vector2 RightStickMultiplier = new Vector2(3, -1.5f);

		void Update()
		{
			Vector2 leftAxis = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
//			Vector2 rightAxis = new Vector2(Input.GetAxis("RightH"), Input.GetAxis("RightV"));

//			Vector2 cameraVector = new Vector2(rightAxis.x * RightStickMultiplier.x, rightAxis.y * RightStickMultiplier.y);
//			if (Mathf.Approximately(rightAxis.x, 0.0f) && Mathf.Approximately(rightAxis.y, 0.0f))
//			{
//				//use mouse as camera vector instead
//				cameraVector = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
//			}

			bool jumpInput = Input.GetButtonDown("Jump");

			Current = new CrustInput()
			{
				LeftAxis = leftAxis,
//				RightAxis = rightAxis,
				Jump = jumpInput
			};
		}
	}

	public struct CrustInput
	{
		public Vector2 LeftAxis;
//		public Vector2 RightAxis;
		public bool Jump;
	}
}