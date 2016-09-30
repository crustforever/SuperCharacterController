using System;
using UnityEngine;

namespace AssemblyCSharp
{
	public class CrustCharacterCamera : MonoBehaviour {

		public float Distance = 5.0f;
		public float Height = 2.0f;

		public GameObject CharacterTarget;

		private CrustCharacterInput _input;
		private Transform _target;
		private CrustCharacterMachine _machine;

		private Vector3 _look_direction;

		private SuperCharacterController _controller;

		void Start()
		{
			_input = CharacterTarget.GetComponent<CrustCharacterInput>();
			_machine = CharacterTarget.GetComponent<CrustCharacterMachine>();
			_controller = CharacterTarget.GetComponent<SuperCharacterController>();
			_target = CharacterTarget.transform;

			this._look_direction = this._target.forward;
		}

		void LateUpdate()
		{
			this.transform.position = this._target.position;

			this._look_direction = Quaternion.AngleAxis(this._input.Current.RightAxis.x, this._controller.up) * this._look_direction;
			transform.rotation = Quaternion.LookRotation(this._look_direction, this._controller.up);

			//_y_rotation += _input.Current.CameraVector.y;

			/*
			Vector3 left = Vector3.Cross(_machine.Facing, _controller.up);

			transform.rotation = Quaternion.LookRotation(_machine.Facing, _controller.up);
			transform.rotation = Quaternion.AngleAxis(_y_rotation, left) * transform.rotation;
			*/

			this.transform.position -= transform.forward * Distance;
			this.transform.position += _controller.up * Height;
		}
	}
}