using System;
using UnityEngine;

namespace AssemblyCSharp
{
	public class CrustCharacterCamera : MonoBehaviour {

		public float Distance = 5.0f;
		public float Height = 2.0f;
		public float RotationDegreesPerSecond = 180.0f;
		public float RotationDeceleration = 10.0f;
		public float RotationDeadZone = 45.0f;

		public GameObject CharacterTarget;

		private CrustCharacterMachine _machine;
		private SuperCharacterController _controller;
		private Transform _target;

		private Vector3 _look_direction;
		private float _rotation_speed;

		void Start()
		{
			_machine = CharacterTarget.GetComponent<CrustCharacterMachine>();
			_controller = CharacterTarget.GetComponent<SuperCharacterController>();
			_target = CharacterTarget.transform;

			this._look_direction = this.transform.forward;
			this._rotation_speed = 0.0f;
		}

		void LateUpdate()
		{
			this.transform.position = this._target.position;

//			if ((CrustCharacterMachine.States)this._machine.currentState == CrustCharacterMachine.States.WALK)
//			{
//				//get the camera direction
//				Vector2 cameraDirection = new Vector2(this.transform.forward.x, this.transform.forward.z).normalized;
//				Vector2 cameraBackDirection = -1 * cameraDirection;
//
//				//get the turn direction (it's already in world space relative to the camera)
//				Vector2 turnDirection = this._machine.LastTurnDirection;
//
//				//get the camera angle and the target angle
//				float targetAngle = Mathf.Atan2(turnDirection.y, turnDirection.x);
//				float currentAngle = Mathf.Atan2(cameraDirection.y, cameraDirection.x);
//
//				//DEBUG
//				Debug.DrawRay(this._target.position, Quaternion.AngleAxis(this.RotationDeadZone, Vector3.up) * new Vector3(cameraBackDirection.x, 0.0f, cameraBackDirection.y), Color.red);
//				Debug.DrawRay(this._target.position, Quaternion.AngleAxis(-this.RotationDeadZone, Vector3.up) * new Vector3(cameraBackDirection.x, 0.0f, cameraBackDirection.y), Color.red);
//
//				//check if the target angle is inside the back facing dead zone; if so, do nothing
//				float backAngleDifference = Mathf.Atan2(cameraBackDirection.y, cameraBackDirection.x) - targetAngle;
//				if (Mathf.Abs(backAngleDifference) < this.RotationDeadZone * Mathf.Deg2Rad)
//				{
//					//do nothing
//				}
//				else
//				{
//					//get the angle difference between the target and the camera
//					float angleDifference = currentAngle - targetAngle;
//
//					//wrap it if necessary (should never happen)
//					if (angleDifference > Mathf.PI)
//						angleDifference = angleDifference - Mathf.PI * 2.0f;
//
//					if (angleDifference < -Mathf.PI)
//						angleDifference = Mathf.PI * 2.0f + angleDifference;
//
//					//get clockwise or counterclockwise speed
//					float turnRate;
//					if (angleDifference < 0)
//						turnRate = Mathf.Max(angleDifference * Mathf.Rad2Deg, -this.RotationDegreesPerSecond * Easing.Quartic.easeOut(Mathf.Abs(angleDifference / Mathf.PI)) * this._controller.deltaTime);
//					else
//						turnRate = Mathf.Min(angleDifference * Mathf.Rad2Deg, this.RotationDegreesPerSecond * Easing.Quartic.easeOut(Mathf.Abs(angleDifference / Mathf.PI)) * this._controller.deltaTime);
//
//					//HACK
//					this._rotation_speed = turnRate;
//
//					//rotate the camera direction towards the turn direction
//					this._look_direction = Quaternion.AngleAxis(turnRate, this._controller.up) * this._look_direction;
//					transform.rotation = Quaternion.LookRotation(this._look_direction);
//				}
//			}
//			else if (Mathf.Abs(this._rotation_speed) > 0.0f)
//			{
//				//decelerate
//				this._rotation_speed = Mathf.MoveTowards(this._rotation_speed, 0.0f, this.RotationDeceleration * this._controller.deltaTime);
//
//				//rotate the camera direction towards the turn direction
//				this._look_direction = Quaternion.AngleAxis(this._rotation_speed, this._controller.up) * this._look_direction;
//				transform.rotation = Quaternion.LookRotation(this._look_direction);
//			}

			//get the camera forward direction
//			Vector2 cameraForward = new Vector2(this.transform.forward.x, this.transform.forward.z).normalized;

			//put the camera back (in the direction opposite our camera direction vector) and up behind the character
//			this.transform.position -= new Vector3(cameraForward.x, 0.0f, cameraForward.y) * this.Distance;

			this.transform.position -= this.transform.forward * this.Distance;
			//this.transform.position += _controller.up * this.Height;
		}
	}
}