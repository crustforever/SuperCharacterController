using System;
using UnityEngine;

namespace AssemblyCSharp
{
	public class CrustCharacterCamera : MonoBehaviour {

		public float Distance = 5.0f;
		public float Height = 2.0f;

		public float MaxRotationSpeed = 180.0f;
		public float MinRotationSpeed;
		public float RotationAcceleration;

		public GameObject CharacterTarget;

		private CrustCharacterInput _input;
		private CrustCharacterMachine _machine;
		private SuperCharacterController _controller;
		private Transform _target;

		private Vector3 _look_direction;

		void Start()
		{
			_input = CharacterTarget.GetComponent<CrustCharacterInput>();
			_machine = CharacterTarget.GetComponent<CrustCharacterMachine>();
			_controller = CharacterTarget.GetComponent<SuperCharacterController>();
			_target = CharacterTarget.transform;

			this._look_direction = this.transform.forward;
		}

		void LateUpdate()
		{
			this.transform.position = this._target.position;

			//get the camera direction
			Vector2 cameraDirection = new Vector2(this.transform.forward.x, this.transform.forward.z).normalized;

			if ((CrustCharacterMachine.States)this._machine.currentState == CrustCharacterMachine.States.WALK)
			{
				//get the turn direction (it's already in world space relative to the camera)
				Vector2 turnDirection = this._machine.LastTurnDirection;

				//get the angle difference between them
				float targetAngle = Mathf.Atan2(turnDirection.y, turnDirection.x);
				float currentAngle = Mathf.Atan2(cameraDirection.y, cameraDirection.x);
				float angleDifference = currentAngle - targetAngle;

				//wrap it if necessary (should never happen)
				if (angleDifference > Mathf.PI)
					angleDifference = angleDifference - Mathf.PI * 2.0f;

				if (angleDifference < -Mathf.PI)
					angleDifference = Mathf.PI * 2.0f + angleDifference;

				//get clockwise or counterclockwise speed
				float turnRate;
				if (angleDifference < 0)
					turnRate = Mathf.Max(angleDifference, -this.MaxRotationSpeed * this._controller.deltaTime);
				else
					turnRate = Mathf.Min(angleDifference, this.MaxRotationSpeed * this._controller.deltaTime);

				//rotate the camera direction towards the turn direction
				this._look_direction = Quaternion.AngleAxis(turnRate, this._controller.up) * this._look_direction;
				transform.rotation = Quaternion.LookRotation(this._look_direction);
			}

			//put the camera back (in the direction opposite our camera direction vector) and up behind the character
			this.transform.position -= new Vector3(cameraDirection.x, 0.0f, cameraDirection.y) * this.Distance;
			this.transform.position += _controller.up * this.Height;
		}
	}
}