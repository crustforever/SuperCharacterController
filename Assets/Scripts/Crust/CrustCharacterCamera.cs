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
				//get the camera's forward direction in the character's XZ plane
				Vector3 planarCameraForward = Math3d.ProjectVectorOnPlane(this._controller.up, this.transform.forward).normalized;

				//check if the target angle is inside the back facing dead zone; if so, kill the rotation
				//HACK HACK HACK
				Vector3 characterMovementDirection = this._machine.StickToWorldDirection(0.0f);
				float backFacingAngularDifference = Vector3.Angle(characterMovementDirection, -planarCameraForward);
				if (backFacingAngularDifference < this.RotationDeadZone)
				{
					this._rotation_speed = 0.0f;
				}
				else
				{
					//get the angular difference between the camera direction and the character's movement direction
					float angularDifference = Vector3.Angle(planarCameraForward, characterMovementDirection);

					//clockwise?
					Vector3 perp = Vector3.Cross(planarCameraForward, characterMovementDirection);
					if (perp.y < 0)
						angularDifference *= -1;

					if (this._controller.up.y < 0)
						angularDifference *= -1;

					//wrap it if necessary (should never happen)
					if (angularDifference > Mathf.PI * Mathf.Rad2Deg)
						angularDifference = angularDifference - Mathf.PI * 2.0f * Mathf.Rad2Deg;

					if (angularDifference < -Mathf.PI * Mathf.Rad2Deg)
						angularDifference = Mathf.PI * 2.0f * Mathf.Rad2Deg + angularDifference;

					//get the rotation speed called for by the tween
					if (angularDifference < 0)
						this._rotation_speed = Mathf.Max(angularDifference, -this.RotationDegreesPerSecond * Easing.Quartic.easeOut(Mathf.Abs(angularDifference / (Mathf.PI * Mathf.Rad2Deg))) * this._controller.deltaTime);
					else
						this._rotation_speed = Mathf.Min(angularDifference, this.RotationDegreesPerSecond * Easing.Quartic.easeOut(Mathf.Abs(angularDifference / (Mathf.PI * Mathf.Rad2Deg))) * this._controller.deltaTime);
				}
			}
			else if (Mathf.Abs(this._rotation_speed) > 0.0f)
			{
				//decelerate
				this._rotation_speed = Mathf.MoveTowards(this._rotation_speed, 0.0f, this.RotationDeceleration * this._controller.deltaTime);
			}

			//rotate the camera direction around the controller's up vector by the rotation speed
			Vector3 rotated = Quaternion.AngleAxis(this._rotation_speed, this._controller.up) * this.transform.forward;
			//this.transform.rotation.SetLookRotation(rotated, this._controller.up);
			//this.transform.rotation *= Quaternion.FromToRotation(this.transform.forward, rotated);
			this.transform.rotation = Quaternion.LookRotation(rotated, this._controller.up);

			//get the camera's new forward direction in the character's XZ plane
			Vector3 newPlanarCameraForward = Math3d.ProjectVectorOnPlane(this._controller.up, this.transform.forward).normalized;

			//add the follow contribution back after applying rotation
			Vector3 follow = Vector3.zero;
			follow -= newPlanarCameraForward * this.BackFollow;
			follow += this._controller.up * this.UpFollow;
			this.transform.position += follow;

			//DEBUG stizz
			DebugExtension.DebugArrow(this._controller.transform.position, newPlanarCameraForward * 2, Color.green);
			Debug.DrawRay(this.transform.position + this.transform.right, this.transform.forward * 10, Color.green);
			Debug.DrawRay(this.transform.position - this.transform.right, this.transform.forward * 10, Color.green);
		}
	}
}