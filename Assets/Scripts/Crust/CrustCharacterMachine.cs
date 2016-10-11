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
		public float TurnSeconds = 1.0f;
		public float AirborneAcceleration = 5.0f;
		public float JumpHeight = 3.0f;
		public float Gravity = 25.0f;

		public enum States { IDLE, WALK, JUMP, FALL }

		public Transform CharacterCameraTransform;

		private SuperCharacterController _controller;
		private CrustCharacterInput _input;
		private Vector3 _velocity;
		private Vector2 _last_turn_direction;
		public Vector2 LastTurnDirection { get { return this._last_turn_direction; }}

		void Start()
		{
			this._input = gameObject.GetComponent<CrustCharacterInput>();
			this._controller = gameObject.GetComponent<SuperCharacterController>();

			//use initial top down direction as the last turn direction
			this._last_turn_direction = this.transform.TopDownForward();

			//set state to idle on start
			currentState = States.IDLE;
		}

		protected override void EarlyGlobalSuperUpdate()
		{
			//
		}

		protected override void LateGlobalSuperUpdate()
		{
			//move the player by our velocity
			transform.position += this._velocity * this._controller.deltaTime;

			//update facing
			Vector2 turnDirection = TopDownCameraRelativeDirection(this.TurnDeadZone);
			if (turnDirection != Vector2.zero)
			{
				this._last_turn_direction = turnDirection;
			}
			UpdateFacing(this._last_turn_direction);
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
		/// Takes the top down direction of the camera and adds the left stick direction to it
		/// </summary>
		private Vector2 TopDownCameraRelativeDirection(float deadZoneMagnitude)
		{
			//get the left stick axis
			Vector2 leftAxis = this._input.Current.LeftAxis;

			//apply the deadzone
			if (Mathf.Abs(leftAxis.magnitude) < deadZoneMagnitude)
				return Vector2.zero;

			//get the camera's top down right and forward directions
			Vector2 camTopDownForward = this.CharacterCameraTransform.TopDownForward(this.transform.up);
			float rightAngle = (this.CharacterCameraTransform.up.y >= 0.0f) ? 90.0f : -90.0f;
			Vector3 camTopDownRight3D = Quaternion.AngleAxis(rightAngle, Vector3.up) * new Vector3(camTopDownForward.x, 0.0f, camTopDownForward.y);
			Vector2 camTopDownRight = new Vector3(camTopDownRight3D.x, camTopDownRight3D.z);

			//TODO move to camera!
			//Debug.DrawRay(this.CharacterCameraTransform.position, new Vector3(camTopDownForward.x, 0.0f, camTopDownForward.y), Color.cyan);
			//Debug.DrawRay(this.CharacterCameraTransform.position, new Vector3(camTopDownRight.x, 0.0f, camTopDownRight.y), Color.yellow);

			//build a top down vector local to the camera by taking the right and forward contributions respective to the camera facing
			Vector2 cameraLocal = Vector2.zero;
			if (!Mathf.Approximately(leftAxis.x, 0.0f))
			{
				cameraLocal += camTopDownRight * leftAxis.x;
			}

			if (!Mathf.Approximately(leftAxis.y, 0.0f))
			{
				cameraLocal += camTopDownForward * leftAxis.y;
			}

			//normalize it to make it a direction
			return cameraLocal.normalized;
		}

		// Calculate the initial velocity of a jump based off gravity and desired maximum height attained
		private float CalculateJumpSpeed(float jumpHeight, float gravity)
		{
			return Mathf.Sqrt(2 * jumpHeight * gravity);
		}

		/// <summary>
		/// Takes a 2D vector representing the desired top down facing
		/// </summary>
		void UpdateFacing(Vector2 targetFacing)
		{
			//get our current facing as a top down 2D vector
			Vector2 currentFacing = this.transform.TopDownForward();

			//apply a tween to rotate the character towards the movement vector such that a FULL TURN will take this.TurnSeconds
			//first find the angular difference between the current facing's angle and the movement direction's angle
			//remember that these angles are calculated using only the X and Z (2D vectors from the topdown perspective)
			float targetAngle = Mathf.Atan2(targetFacing.y, targetFacing.x);
			float currentAngle = Mathf.Atan2(currentFacing.y, currentFacing.x);
			float angleDifference = targetAngle - currentAngle;

			//this angle difference is essentially a measurement of how far away from the desired movement vector we are
			//the angle difference therefore should always be between -180 and 180 degrees (or -PI and PI)

			//if the magnitude of the angle difference is greater than 180, set it to it's smaller complimentary angle
			if (angleDifference > Mathf.PI)
				angleDifference = angleDifference - Mathf.PI * 2.0f;

			if (angleDifference < -Mathf.PI)
				angleDifference = Mathf.PI * 2.0f + angleDifference;

			//now that we have an angle from -180 to 180, we map it to a value from (0-1) so that we can use a tween operation on it
			//for tweening purposes we only care about this mapped number insofar as it gives us a measurement of our current vector facing as part a FULL TURN (180 degrees arc)
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
			Vector3 steppedFacing = Quaternion.AngleAxis(steppedAngleDifference, Vector3.up) * new Vector3(-targetFacing.x, 0.0f, -targetFacing.y);

			//rotate the stepped facing back into world space relative to the character's up vector
			float angle = Vector3.Angle(Vector3.up, this.transform.up);
			Vector3 perp = Vector3.Cross(Vector3.up, this.transform.up);
			Vector3 steppedWorldFacing = Quaternion.AngleAxis(angle, perp) * steppedFacing;

			//set rotation on the transform by converting to a quaternion
			this.transform.rotation = Quaternion.LookRotation(steppedWorldFacing, this.transform.up);

//			Debug.DrawRay(this.transform.position, new Vector3(targetFacing.x, 0.0f, targetFacing.y) * 10.0f, Color.green);
//			Debug.DrawRay(this.transform.position, steppedWorldFacing * 10.0f, Color.yellow);
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

			if (TopDownCameraRelativeDirection(this.MoveDeadZone) != Vector2.zero)
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

			//get the 2D world direction the character wants to move
			Vector2 movementDirection = TopDownCameraRelativeDirection(this.MoveDeadZone);

			//transform to 3D with respect to the character's up vector
			float angle = Vector3.Angle(Vector3.up, this.transform.up);
			Vector3 perp = Vector3.Cross(Vector3.up, this.transform.up);
			Vector3 movementDirection3D = Quaternion.AngleAxis(angle, perp) * new Vector3(movementDirection.x, 0.0f, movementDirection.y);

			if (movementDirection3D != Vector3.zero)
			{
				//apply an acceleration in that direction
				_velocity = Vector3.MoveTowards(this._velocity, movementDirection3D * MoveSpeed, MoveAcceleration * _controller.deltaTime);
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

			//get the 2D world direction the character wants to move
			Vector2 movementDirection = TopDownCameraRelativeDirection(this.MoveDeadZone);

			//transform to 3D with respect to the character's up vector
			float angle = Vector3.Angle(Vector3.up, this.transform.up);
			Vector3 perp = Vector3.Cross(Vector3.up, this.transform.up);
			Vector3 movementDirection3D = Quaternion.AngleAxis(angle, perp) * new Vector3(movementDirection.x, 0.0f, movementDirection.y);

			horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, movementDirection3D * MoveSpeed, AirborneAcceleration * this._controller.deltaTime);
			verticalVelocity -= this._controller.up * Gravity * this._controller.deltaTime;

			this._velocity = horizontalVelocity + verticalVelocity;
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