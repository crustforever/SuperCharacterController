using System;
using UnityEngine;

namespace AssemblyCSharp
{
	[RequireComponent(typeof(SuperCharacterController))]
	[RequireComponent(typeof(CrustCharacterInput))]
	public class CrustCharacterMachine : SuperStateMachine
	{
		//public Transform AnimatedMesh;

		public float WalkSpeed = 4.0f;
		public float WalkAcceleration = 30.0f;
		public float JumpAcceleration = 5.0f;
		public float JumpHeight = 3.0f;
		public float Gravity = 25.0f;

		// Add more states by comma separating them
		enum States { Idle, Walk, Jump, Fall }

		private SuperCharacterController _controller;

		private Vector3 _velocity;
		public Vector3 Facing { get; private set; }

		private CrustCharacterInput _input;

		void Start () {
			// Put any code here you want to run ONCE, when the object is initialized

			_input = gameObject.GetComponent<CrustCharacterInput>();

			// Grab the controller object from our object
			_controller = gameObject.GetComponent<SuperCharacterController>();

			// Our character's current facing direction, planar to the ground
			Facing = transform.forward;

			// Set our currentState to idle on startup
			currentState = States.Idle;
		}

		protected override void EarlyGlobalSuperUpdate()
		{
			// Rotate out facing direction horizontally based on mouse input
			//_facing = Quaternion.AngleAxis(_input.Current.CameraVector.x, _controller.up) * _facing;

			// Put any code in here you want to run BEFORE the state's update function.
			// This is run regardless of what state you're in
		}

		protected override void LateGlobalSuperUpdate()
		{
			// Put any code in here you want to run AFTER the state's update function.
			// This is run regardless of what state you're in

			// Move the player by our velocity every frame
			transform.position += this._velocity * this._controller.deltaTime;

			// Rotate our mesh to face where we are "looking"
			//AnimatedMesh.rotation = Quaternion.LookRotation(_facing, _controller.up);
		}

		private bool AcquiringGround()
		{
			return this._controller.currentGround.IsGrounded(false, 0.01f);
		}

		private bool MaintainingGround()
		{
			return this._controller.currentGround.IsGrounded(true, 0.5f);
		}

//		public void RotateGravity(Vector3 up)
//		{
//			_facing = Quaternion.FromToRotation(transform.up, up) * _facing;
//		}

		/// <summary>
		/// Constructs a vector representing our movement local to our facing
		/// </summary>
		private Vector3 LocalMovementDirection()
		{
			Vector3 right = Vector3.Cross(this._controller.up, this.Facing);

			Vector3 local = Vector3.zero;
			if (!Mathf.Approximately(this._input.Current.MovementVector.x, 0.0f))
			{
				local += right * this._input.Current.MovementVector.x;
			}

			if (!Mathf.Approximately(this._input.Current.MovementVector.y, 0.0f))
			{
				local += this.Facing * this._input.Current.MovementVector.y;
			}

			return local.normalized;
		}

		// Calculate the initial velocity of a jump based off gravity and desired maximum height attained
		private float CalculateJumpSpeed(float jumpHeight, float gravity)
		{
			return Mathf.Sqrt(2 * jumpHeight * gravity);
		}
			
		#region Idle

		void Idle_EnterState()
		{
			this._controller.EnableSlopeLimit();
			this._controller.EnableClamping();
		}

		void Idle_SuperUpdate()
		{
			// Run every frame we are in the idle state

			if (_input.Current.Jump)
			{
				currentState = States.Jump;
				return;
			}

			if (!MaintainingGround())
			{
				currentState = States.Fall;
				return;
			}

			if (_input.Current.MovementVector != Vector2.zero)
			{
				currentState = States.Walk;
				return;
			}

			//apply friction to slow us to a halt
			_velocity = Vector3.MoveTowards(this._velocity, Vector3.zero, 10.0f * this._controller.deltaTime);
		}

		#endregion

		#region Walk

		void Walk_SuperUpdate()
		{
			if (_input.Current.Jump)
			{
				currentState = States.Jump;
				return;
			}

			if (!MaintainingGround())
			{
				currentState = States.Fall;
				return;
			}

			if (_input.Current.MovementVector != Vector2.zero)
			{
				_velocity = Vector3.MoveTowards(_velocity, LocalMovementDirection() * WalkSpeed, WalkAcceleration * _controller.deltaTime);
			}
			else
			{
				currentState = States.Idle;
				return;
			}
		}

		#endregion

		#region Jump

		void Jump_EnterState()
		{
			_controller.DisableClamping();
			_controller.DisableSlopeLimit();

			_velocity += _controller.up * CalculateJumpSpeed(JumpHeight, Gravity);
		}

		void Jump_SuperUpdate()
		{
			Vector3 planarMoveDirection = Math3d.ProjectVectorOnPlane(_controller.up, _velocity);
			Vector3 verticalMoveDirection = _velocity - planarMoveDirection;

			if (Vector3.Angle(verticalMoveDirection, _controller.up) > 90 && AcquiringGround())
			{
				_velocity = planarMoveDirection;
				currentState = States.Idle;
				return;            
			}

			planarMoveDirection = Vector3.MoveTowards(planarMoveDirection, LocalMovementDirection() * WalkSpeed, JumpAcceleration * _controller.deltaTime);
			verticalMoveDirection -= _controller.up * Gravity * _controller.deltaTime;

			_velocity = planarMoveDirection + verticalMoveDirection;
		}

		#endregion

		#region Fall

		void Fall_EnterState()
		{
			_controller.DisableClamping();
			_controller.DisableSlopeLimit();

			// moveDirection = trueVelocity;
		}

		void Fall_SuperUpdate()
		{
			if (AcquiringGround())
			{
				_velocity = Math3d.ProjectVectorOnPlane(_controller.up, _velocity);
				currentState = States.Idle;
				return;
			}

			_velocity -= _controller.up * Gravity * _controller.deltaTime;
		}

		#endregion
	}
}

