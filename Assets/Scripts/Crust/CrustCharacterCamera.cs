using System;
using UnityEngine;

namespace AssemblyCSharp
{
	public class CrustCharacterCamera : MonoBehaviour {

		public float BackFollow = 5.0f;
		public float UpFollow = 2.0f;
		public float RotationDegreesPerSecond = 180.0f;
		public float RotationDeceleration = 10.0f;
		public float RotationDeadZone = 45.0f;

		public GameObject CharacterTarget;

		private CrustCharacterMachine _machine;
		private SuperCharacterController _controller;

		private float _rotation_speed;

		void Start()
		{
			_machine = CharacterTarget.GetComponent<CrustCharacterMachine>();
			_controller = CharacterTarget.GetComponent<SuperCharacterController>();

			this._rotation_speed = 0.0f;
		}

		void LateUpdate()
		{
			this.transform.position = this._controller.transform.position;

			if ((CrustCharacterMachine.States)this._machine.currentState == CrustCharacterMachine.States.WALK)
			{
				//get the camera's top down forward direction
				Vector2 cameraDirection = this.transform.TopDownForward(this._controller.up);
				Vector2 cameraBackDirection = -1 * cameraDirection;

				//get the character's turn direction (it's already in world space relative to the camera)
				Vector2 turnDirection = this._machine.LastTurnDirection;

				//get the camera's current angle and the target angle
				float currentAngle = Mathf.Atan2(cameraDirection.y, cameraDirection.x);
				float targetAngle = Mathf.Atan2(turnDirection.y, turnDirection.x);

				//DEBUG
				//Debug.DrawRay(this._target.position, Quaternion.AngleAxis(this.RotationDeadZone, Vector3.up) * new Vector3(cameraBackDirection.x, 0.0f, cameraBackDirection.y), Color.red);
				//Debug.DrawRay(this._target.position, Quaternion.AngleAxis(-this.RotationDeadZone, Vector3.up) * new Vector3(cameraBackDirection.x, 0.0f, cameraBackDirection.y), Color.red);

				//check if the target angle is inside the back facing dead zone; if so, do nothing
				float backAngleDifference = Mathf.Atan2(cameraBackDirection.y, cameraBackDirection.x) - targetAngle;
				if (Mathf.Abs(backAngleDifference) < this.RotationDeadZone * Mathf.Deg2Rad)
				{
					//do nothing
				}
				else
				{
					//get the angle difference between the target and the camera
					float angleDifference = currentAngle - targetAngle;

					//wrap it if necessary (should never happen)
					if (angleDifference > Mathf.PI)
						angleDifference = angleDifference - Mathf.PI * 2.0f;

					if (angleDifference < -Mathf.PI)
						angleDifference = Mathf.PI * 2.0f + angleDifference;

					//get clockwise or counterclockwise speed
					float turnRate;
					if (angleDifference < 0)
						turnRate = Mathf.Max(angleDifference * Mathf.Rad2Deg, -this.RotationDegreesPerSecond * Easing.Quartic.easeOut(Mathf.Abs(angleDifference / Mathf.PI)) * this._controller.deltaTime);
					else
						turnRate = Mathf.Min(angleDifference * Mathf.Rad2Deg, this.RotationDegreesPerSecond * Easing.Quartic.easeOut(Mathf.Abs(angleDifference / Mathf.PI)) * this._controller.deltaTime);

					//rotate the camera direction around the controller's up vector towards the turn direction
					Vector3 rotated = Quaternion.AngleAxis(turnRate, this._controller.up) * this.transform.forward;
					this.transform.rotation = Quaternion.LookRotation(rotated, this._controller.up);

					//HACK store rotation speed for deceleration when idle
					this._rotation_speed = turnRate;
				}
			}
			else if (Mathf.Abs(this._rotation_speed) > 0.0f)
			{
				//decelerate
				this._rotation_speed = Mathf.MoveTowards(this._rotation_speed, 0.0f, this.RotationDeceleration * this._controller.deltaTime);

				//rotate the camera direction towards the turn direction by the remaining rotation speed
				Vector3 rotated = Quaternion.AngleAxis(this._rotation_speed, this._controller.up) * this.transform.forward;
				transform.rotation = Quaternion.LookRotation(rotated, this._controller.up);
			}

			//get the camera's top down forward direction
			Vector2 cameraForward = this.transform.TopDownForward(this._controller.up);

			//negate it and scale it by BackFollow
			cameraForward *= -this.BackFollow;

			//make it a 3D vector and rotate it back to world space relative to the character's up vector
			float angle = Vector3.Angle(Vector3.up, this._controller.up);
			Vector3 perp = Vector3.Cross(Vector3.up, this._controller.up);
			Vector3 world = Quaternion.AngleAxis(angle, perp) * new Vector3(cameraForward.x, 0.0f, cameraForward.y);

			//back follow contribution
			this.transform.position += world;

			//up follow contribution
			this.transform.position += this._controller.up * this.UpFollow;
		}
	}
}