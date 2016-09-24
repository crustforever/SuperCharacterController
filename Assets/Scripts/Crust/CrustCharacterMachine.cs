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
		public float TurnSeconds = 1.0f;
		public float JumpAcceleration = 5.0f;
		public float JumpHeight = 3.0f;
		public float Gravity = 25.0f;

		// Add more states by comma separating them
		enum States { IDLE, WALK, JUMP, FALL }

		private SuperCharacterController _controller;

		private Vector3 _velocity;
//		public float Rotation {
//			get { return this.transform.eulerAngles.y; }
//			private set { this.transform.eulerAngles.y = Mathf.Abs(value) % 360.0f; }
//		}

		private CrustCharacterInput _input;

		void Start () {
			// Put any code here you want to run ONCE, when the object is initialized

			_input = gameObject.GetComponent<CrustCharacterInput>();

			// Grab the controller object from our object
			_controller = gameObject.GetComponent<SuperCharacterController>();

			// Our character's current facing direction, planar to the ground
			//Rotation = transform.forward;

			// Set our currentState to idle on startup
			currentState = States.IDLE;
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
			Vector3 right = Vector3.Cross(this._controller.up, new Vector3(0.0f, 0.0f, 1.0f));

			Vector3 local = Vector3.zero;
			if (!Mathf.Approximately(this._input.Current.MovementVector.x, 0.0f))
			{
				local += right * this._input.Current.MovementVector.x;
			}

			if (!Mathf.Approximately(this._input.Current.MovementVector.y, 0.0f))
			{
				local += new Vector3(0.0f, 0.0f, 1.0f) * this._input.Current.MovementVector.y;
			}

			return local.normalized;
		}

		// Calculate the initial velocity of a jump based off gravity and desired maximum height attained
		private float CalculateJumpSpeed(float jumpHeight, float gravity)
		{
			return Mathf.Sqrt(2 * jumpHeight * gravity);
		}
			
		#region Idle

		void IDLE_EnterState()
		{
			this._controller.EnableSlopeLimit();
			this._controller.EnableClamping();
		}

		void IDLE_SuperUpdate()
		{
			// Run every frame we are in the idle state

			if (_input.Current.Jump)
			{
				currentState = States.JUMP;
				return;
			}

			if (!MaintainingGround())
			{
				currentState = States.FALL;
				return;
			}

			if (_input.Current.MovementVector != Vector2.zero)
			{
				currentState = States.WALK;
				return;
			}

			//apply friction to slow us to a halt
			_velocity = Vector3.MoveTowards(this._velocity, Vector3.zero, 10.0f * this._controller.deltaTime);
		}

		#endregion

		#region Walk

		void WALK_SuperUpdate()
		{
			if (_input.Current.Jump)
			{
				currentState = States.JUMP;
				return;
			}

			if (!MaintainingGround())
			{
				currentState = States.FALL;
				return;
			}

			if (_input.Current.MovementVector != Vector2.zero)
			{
				Vector3 localMovementDirection = LocalMovementDirection();

				_velocity = Vector3.MoveTowards(_velocity, localMovementDirection * WalkSpeed, WalkAcceleration * _controller.deltaTime);

				//turn the character's forward vector towards our movement vector (which is a topdown 2D vector -- X and Z)
				//Vector3 movementVector3 = new Vector3(this._input.Current.MovementVector.x, 0.0f, this._input.Current.MovementVector.y);
//				Debug.DrawRay(this.transform.position, movementVector3, Color.green, 20);
//
//				//calculate the angle between the movement (joystick) and the current facing (transform.forward)
				//float angleDelta = Vector3.Angle(this.transform.forward, localMovementDirection);
//
//				//find the current position of the full 180 degree turn that we're on
//				float t = (180.0f - angleDelta) / 180.0f;
//
//				//step it
//				float step = this._controller.deltaTime / this.TurnSeconds;
//				float stepped = Mathf.Min(t + step, 1.0f);
//
//				//applying an ease to it will map the stepped value from the linear 0-1 to the eased 0-1 depending on the easing function
//				float eased = Easing.Quartic.easeOut(stepped);
//
//				Vector3 origin = Quaternion.AngleAxis(180.0f, Vector3.up) * movementVector3;
//				Debug.DrawRay(this.transform.position, origin, Color.red, 20);
//
//				Vector3 forward = Quaternion.AngleAxis(eased * 180.0f, Vector3.up) * origin;
//				Debug.DrawRay(this.transform.position, forward, Color.cyan, 20);
//
//				this.transform.rotation = Quaternion.LookRotation(forward);

				//float t = (180.0f - angleDelta) / 180.0f;
				//float stepped = Mathf.Min(t + this._controller.deltaTime / this.TurnSeconds, 1.0f);

				//float linearStep = this._controller.deltaTime / this.TurnSeconds;
				//float linearStepDelta = t + linearStep - t;

				//HOW TO MAP A LINEAR STEP TO AN EASED STEP BASED ON THE CURRENT ANGLE BETWEEN THE CURRENT FACING AND THE TARGET FACING?
				float angleDifference = Mathf.Atan2(localMovementDirection.z, localMovementDirection.x) - Mathf.Atan2(this.transform.forward.z, this.transform.forward.x);

				//map the angle difference to a value from 0 - 1 that indicates its position in the linear path from 180 degrees to 0 degrees
				float parameterizedAngleDifference = 1.0f - Mathf.Abs(angleDifference) / Mathf.PI;

				//using our easing function, find the eased t value that corresponds to this linear position
				//x = -1 * ((t - 1)^4 - 1)
				//t = (1 - x)^(1 / 4) + 1
				//float t = (angleDifference > 0) ? Mathf.Pow(1.0f - parameterizedAngleDifference, 1.0f / 4.0f) + 1 : -1 * Mathf.Pow(1.0f - parameterizedAngleDifference, 1.0f / 4.0f) + 1;
				float t = -1.0f * Mathf.Pow(1.0f - parameterizedAngleDifference, 1.0f / 4.0f) + 1.0f;

				//step t
				float timeStep = this._controller.deltaTime / this.TurnSeconds;
				float stepped = Mathf.Min(t + timeStep, 1.0f);
				float eased = Easing.Quartic.easeOut(stepped);

				//convert back to an angle
				float steppedAngleDifference = eased * Mathf.PI;
				float steppedAngleDifferenceDeg = steppedAngleDifference * Mathf.Rad2Deg;

				//clockwise / counterclockwise
				if (angleDifference > 0)
					steppedAngleDifferenceDeg *= -1.0f;

				//find the new facing by rotating the back facing movement vector by the proper number of degrees
				Vector3 steppedFacing = Quaternion.AngleAxis(steppedAngleDifferenceDeg, Vector3.up) * new Vector3(-localMovementDirection.x, 0.0f, -localMovementDirection.z);
				this.transform.rotation = Quaternion.LookRotation(steppedFacing);

				//Vector3 rotated = Vector3.RotateTowards(this.transform.forward, localMovementDirection, this._controller.deltaTime / this.TurnSeconds * Mathf.PI, 0.0f);

				//Vector3 steppedFacing = Quaternion.AngleAxis(180.0f * this._controller.deltaTime / this.TurnSeconds, Vector3.up) * this.transform.forward;
				//this.transform.rotation = Quaternion.LookRotation(rotated);

				//Vector3 steppedVector = Vector3.RotateTowards(this.transform.forward, movementVector3, easedStepDelta * Mathf.PI, 0.0f);

				//set it
				//this.transform.rotation = Quaternion.LookRotation(steppedVector);

				//float speed = angleDelta / 180.0f;

				//now i need to convert that parameterized and eased t value back into a speed contribution
				//this.transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(this.transform.forward, new Vector3(this._input.Current.MovementVector.x, 0.0f, this._input.Current.MovementVector.y), (Mathf.PI + Mathf.PI * speed) * this._controller.deltaTime, 0.0f));
			}
			else
			{
				currentState = States.IDLE;
				return;
			}
		}

		#endregion

		#region Jump

		void JUMP_EnterState()
		{
			_controller.DisableClamping();
			_controller.DisableSlopeLimit();

			_velocity += _controller.up * CalculateJumpSpeed(JumpHeight, Gravity);
		}

		void JUMP_SuperUpdate()
		{
			Vector3 planarMoveDirection = Math3d.ProjectVectorOnPlane(_controller.up, _velocity);
			Vector3 verticalMoveDirection = _velocity - planarMoveDirection;

			if (Vector3.Angle(verticalMoveDirection, _controller.up) > 90 && AcquiringGround())
			{
				_velocity = planarMoveDirection;
				currentState = States.IDLE;
				return;            
			}

			planarMoveDirection = Vector3.MoveTowards(planarMoveDirection, LocalMovementDirection() * WalkSpeed, JumpAcceleration * _controller.deltaTime);
			verticalMoveDirection -= _controller.up * Gravity * _controller.deltaTime;

			_velocity = planarMoveDirection + verticalMoveDirection;
		}

		#endregion

		#region Fall

		void FALL_EnterState()
		{
			_controller.DisableClamping();
			_controller.DisableSlopeLimit();

			// moveDirection = trueVelocity;
		}

		void FALL_SuperUpdate()
		{
			if (AcquiringGround())
			{
				_velocity = Math3d.ProjectVectorOnPlane(_controller.up, _velocity);
				currentState = States.IDLE;
				return;
			}

			_velocity -= _controller.up * Gravity * _controller.deltaTime;
		}

		#endregion
	}
}

