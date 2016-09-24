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
		private float _y_rotation;

		private SuperCharacterController _controller;

		void Start()
		{
			_input = CharacterTarget.GetComponent<CrustCharacterInput>();
			_machine = CharacterTarget.GetComponent<CrustCharacterMachine>();
			_controller = CharacterTarget.GetComponent<SuperCharacterController>();
			_target = CharacterTarget.transform;
		}

		void LateUpdate()
		{
			transform.position = _target.position;

			_y_rotation += _input.Current.CameraVector.y;

			/*
			Vector3 left = Vector3.Cross(_machine.Facing, _controller.up);

			transform.rotation = Quaternion.LookRotation(_machine.Facing, _controller.up);
			transform.rotation = Quaternion.AngleAxis(_y_rotation, left) * transform.rotation;
			*/

			transform.position -= transform.forward * Distance;
			transform.position += _controller.up * Height;
		}
	}
}

