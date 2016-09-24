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
				//get the direction the character wants to move
				Vector3 movementDirection = LocalMovementDirection();

				//apply an acceleration in that direction
				_velocity = Vector3.MoveTowards(_velocity, movementDirection * WalkSpeed, WalkAcceleration * _controller.deltaTime);

				//apply a tween to rotate the character towards the movement vector
				//first find the angular difference between the current facing's angle and the movement direction's angle
				//remember that these angles are calculated using only the X and Z (2D vectors from the topdown perspective)
				float moveAngle = Mathf.Atan2(movementDirection.z, movementDirection.x);
				float facingAngle = Mathf.Atan2(this.transform.forward.z, this.transform.forward.x);
				float angleDifference = moveAngle - facingAngle;

				//this angle difference is essentially a measurement of how far away from the desired movement vector we are
				//the angle difference therefore should always be between -180 and 180 degrees (or -PI and PI)

				//if the magnitude of the angle difference is greater than 180, set it to it's smaller complimentary angle
				if (angleDifference > Mathf.PI)
					angleDifference = angleDifference - Mathf.PI * 2.0f;

				if (angleDifference < -Mathf.PI)
					angleDifference = Mathf.PI * 2.0f + angleDifference;

				//now that we have an angle from -180 to 180, we map it to a value from (0-1) so that we can use a tween operation on it
				//for tweening purposes we only care about this mapped number insofar as it gives us a measurement of our current vector facing in a full turn (180 degrees arc)
				//therefore we can ignore the sign of the angle (keeping track of it for later tells us the direction of our spin)
				bool clockwise = angleDifference < 0;
				float mappedAngleDifference = 1.0f - Mathf.Abs(angleDifference) / Mathf.PI;

				//our tween function is quadratic "ease out"
				//x = -1 * ((t - 1)^4 - 1)

				//if we were turning the character linearly, our t value (the time parameter) would be exactly equal to our mapped angle difference
				//but because we are not turning the character at a constant rate, we need to find the value of t which lines up with our current angle facing
				//we do this by solving for t in the tween equation
				//t = (1 - x)^(1 / 4) + 1
				float t = -1.0f * Mathf.Pow(1.0f - mappedAngleDifference, 1.0f / 4.0f) + 1.0f;

				//now that we have our position in the eased tween that corresponds to our current angle, we can step that t value linearly (w/ our turn time parameter)
				float timeStep = this._controller.deltaTime / this.TurnSeconds;
				float stepped = Mathf.Min(t + timeStep, 1.0f);

				//popping that stepped value into the original tween function gives us our new stepped angle difference
				float eased = Easing.Quartic.easeOut(stepped);

				//but it is still parameterized from 0-1, so we convert it to degrees
				float steppedAngleDifference = eased * 180.0f;

				//then we multiply by -1 if necessary to apply the proper spin direction
				if (!clockwise)
					steppedAngleDifference *= -1.0f;

				//finally we use a quaternion to rotate our back facing vector around the Y axis by the new stepped angle location which gives us a new facing vector which has
				//been stepped through the tween function
				//NOTE: it is important to realize that the vector that we're actually rotating to get the result is the vector opposite from the movement vector
				//we rotate that one because our tween function goes from 0-1 and the opposite vector (180 degrees away) is the reference point for the beginning of the tween (0) and
				//the movement vector itself is the reference point for the end of the tween (1)
				Vector3 steppedFacing = Quaternion.AngleAxis(steppedAngleDifference, Vector3.up) * new Vector3(-movementDirection.x, 0.0f, -movementDirection.z);

				//set rotation on the transform by converting to a quaternion 
				this.transform.rotation = Quaternion.LookRotation(steppedFacing);
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

