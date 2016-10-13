using System;
using UnityEngine;

namespace AssemblyCSharp
{
	[RequireComponent(typeof(SuperCharacterController))]
	[RequireComponent(typeof(CrustCharacterInput))]
	public class CrustCharacterMachine : SuperStateMachine
	{
		public float MoveSpeed = 4.0f;
		public float MoveAcceleration = 30.0f;
		public float FrictionDeceleration = 10.0f;
		public float MoveDeadZone = 0.7f;
		public float TurnDeadZone = 0.5f;
		public float FullTurnSeconds = 1.0f;
		public float AirborneAcceleration = 5.0f;
		public float JumpHeight = 3.0f;
		public float Gravity = 25.0f;

		public float DebugMagnitude = 2.0f;

		public enum States { IDLE, WALK, JUMP, FALL }

		public Transform CharacterCameraTransform;

		private SuperCharacterController _controller;
		private CrustCharacterInput _input;

		private Vector3 _velocity;
		private Vector3 _movement_direction;

		public Vector2 LastStickDirection { get; private set; }

		void Start()
		{
			this._input = gameObject.GetComponent<CrustCharacterInput>();
			this._controller = gameObject.GetComponent<SuperCharacterController>();

			//set state to idle on start
			currentState = States.IDLE;
		}

		protected override void EarlyGlobalSuperUpdate()
		{
			//get the world movement direction as a function of stick direction and camera facing
			this._movement_direction = StickToWorld(this._input.Current.LeftAxis, this.MoveDeadZone);
		}

		protected override void LateGlobalSuperUpdate()
		{
			//move the character by its velocity
			transform.position += this._velocity * this._controller.deltaTime;

			//update facing to the last non-zero turn direction
			Vector3 facing = StickToWorld(this._input.Current.LeftAxis, this.TurnDeadZone);
			if (facing != Vector3.zero)
			{
				this.LastStickDirection = this._input.Current.LeftAxis;
			}
			else
			{
				facing = StickToWorld(this.LastStickDirection, this.TurnDeadZone);
			}

			if (facing != Vector3.zero)
				UpdateFacing(facing, this.FullTurnSeconds);

			//DEBUG stizz
			DebugExtension.DebugCircle(this.transform.position, this.transform.up, Color.yellow, this.DebugMagnitude);
			DebugExtension.DebugArrow(this.transform.position, facing * this.DebugMagnitude, Color.black);
			DebugExtension.DebugArrow(this.transform.position, this._movement_direction * this.DebugMagnitude, Color.red);
		}

		private bool AcquiringGround()
		{
			return this._controller.currentGround.IsGrounded(false, 0.01f);
		}

		private bool MaintainingGround()
		{
			return this._controller.currentGround.IsGrounded(true, 0.5f);
		}

		/// <summary>
		/// Takes a vector2 representing a control stick direction and returns a vector3 representing that vector in world space (relative to the character's normal and the camera's forward)
		/// </summary>
		public Vector3 StickToWorld(Vector2 stickAxis, float deadZoneMagnitude)
		{
			//apply the deadzone
			if (Mathf.Abs(stickAxis.magnitude) < deadZoneMagnitude)
				return Vector3.zero;
			else
				stickAxis = stickAxis.normalized;

			Vector3 planarCameraForward = Math3d.ProjectVectorOnPlane(this.transform.up, this.CharacterCameraTransform.forward).normalized;
			Vector3 planarCameraRight = Math3d.ProjectVectorOnPlane(this.transform.up, this.CharacterCameraTransform.right).normalized;

			//build a world vector by adding the stick's X and Y to the camera's right and forward vectors
			Vector3 worldDirection = Vector3.zero;
			if (!Mathf.Approximately(stickAxis.x, 0.0f))
			{
				worldDirection += planarCameraRight * stickAxis.x;
			}

			if (!Mathf.Approximately(stickAxis.y, 0.0f))
			{
				worldDirection += planarCameraForward * stickAxis.y;
			}

			return worldDirection;
		}

		// Calculate the initial velocity of a jump based off gravity and desired maximum height attained
		private float CalculateJumpSpeed(float jumpHeight, float gravity)
		{
			return Mathf.Sqrt(2 * jumpHeight * gravity);
		}

		/// <summary>
		/// Rotate the character through a tween towards the given facing such that a FULL TURN will take this.TurnSeconds
		/// </summary>
		void UpdateFacing(Vector3 targetFacing, float fullTurnSeconds)
		{
			//first find the angular difference between the current facing's angle and the movement direction's angle
			float angularDifference = Vector3.Angle(this.transform.forward, targetFacing);

			//clockwise?
			Vector3 perp = Vector3.Cross(targetFacing, this.transform.forward);
			if (perp.y < 0)
				angularDifference *= -1;

			//upside-down?
			if (this.transform.up.y < 0)
				angularDifference *= -1;

			//this angle difference is a measurement of how far away from the desired facing vector we are
			//the angle difference therefore should always be between -180 and 180 degrees (or -PI and PI)

			//if the magnitude of the angle difference is greater than 180, set it to it's smaller complimentary angle
			if (angularDifference > Mathf.PI * Mathf.Rad2Deg)
				angularDifference = angularDifference - Mathf.PI * 2.0f * Mathf.Rad2Deg;

			if (angularDifference < -Mathf.PI * Mathf.Rad2Deg)
				angularDifference = Mathf.PI * 2.0f * Mathf.Rad2Deg + angularDifference;

			//now that we have an angle from -180 to 180, we map it to a value from (0-1) so that we can use a tween operation on it
			//for tweening purposes we only care about this mapped number insofar as it gives us a measurement of our current vector facing as part a FULL TURN (180 degrees arc)
			//therefore we can ignore the sign of the angle (keeping track of it for later tells us the direction of our spin)
			bool clockwise = angularDifference < 0;
			float mappedAngleDifference = 1.0f - Mathf.Abs(angularDifference) / (Mathf.PI * Mathf.Rad2Deg);

			//our tween function is quadratic "ease out"
			//x = -1 * ((t - 1)^4 - 1)

			//if we were turning the character linearly, our t value (the time parameter) would be exactly equal to our mapped angle difference
			//but because we are not turning the character at a constant rate, we need to find the value of t which lines up with our current angle facing
			//we do this by solving for t in the tween equation
			//t = (1 - x)^(1 / 4) + 1
			float t = -1.0f * Mathf.Pow(1.0f - mappedAngleDifference, 1.0f / 4.0f) + 1.0f;

			//now that we have our position in the eased tween that corresponds to our current angle, we can step that t value linearly (w/ our turn time parameter)
			float timeStep = this._controller.deltaTime / fullTurnSeconds;
			float stepped = Mathf.Min(t + timeStep, 1.0f);

			//popping that stepped value into the original tween function gives us our new stepped angle difference
			float eased = Easing.Quartic.easeOut(stepped);

			//but it is still parameterized from 0-1, so we convert it to degrees
			float steppedAngleDifference = eased * Mathf.PI * Mathf.Rad2Deg;

			//then we multiply by -1 if necessary to apply the proper spin direction
			if (!clockwise)
				steppedAngleDifference *= -1.0f;

			//finally we use a quaternion to rotate our back facing vector around the Y axis by the new stepped angle location which gives us a new facing vector which has
			//been stepped through the tween function
			//NOTE: it is important to realize that the vector that we're actually rotating to get the result is the vector opposite from the movement vector
			//we rotate that one because our tween function goes from 0-1 and the opposite vector (180 degrees away) is the reference point for the beginning of the tween (0) and
			//the movement vector itself is the reference point for the end of the tween (1)
			Vector3 steppedFacing = Quaternion.AngleAxis(steppedAngleDifference, this.transform.up) * -targetFacing;
			this.transform.rotation = Quaternion.LookRotation(steppedFacing, this.transform.up);
		}
			
		#region Idle

		void IDLE_EnterState()
		{
			this._controller.EnableSlopeLimit();
			this._controller.EnableClamping();
		}

		void IDLE_SuperUpdate()
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

			if (this._movement_direction != Vector3.zero)
			{
				currentState = States.WALK;
				return;
			}

			//apply friction
			_velocity = Vector3.MoveTowards(this._velocity, Vector3.zero, this.FrictionDeceleration * this._controller.deltaTime);
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

			if (this._movement_direction != Vector3.zero)
			{
				//apply an acceleration in that direction
				_velocity = Vector3.MoveTowards(this._velocity, this._movement_direction * MoveSpeed, MoveAcceleration * _controller.deltaTime);
			}
			else
			{
				currentState = States.IDLE;
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
			Vector3 horizontalVelocity = Math3d.ProjectVectorOnPlane(this._controller.up, this._velocity);
			Vector3 verticalVelocity = this._velocity - horizontalVelocity;

			if (Vector3.Angle(verticalVelocity, this._controller.up) > 90 && AcquiringGround())
			{
				this._velocity = horizontalVelocity;
				this.currentState = States.IDLE;
				return;
			}

			horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, this._movement_direction * MoveSpeed, AirborneAcceleration * this._controller.deltaTime);
			verticalVelocity -= this._controller.up * Gravity * this._controller.deltaTime;

			this._velocity = horizontalVelocity + verticalVelocity;
		}

		#endregion

		#region Fall

		void FALL_EnterState()
		{
			_controller.DisableClamping();
			_controller.DisableSlopeLimit();
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