using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Controller2D))]
public class Player : MonoBehaviour {

	public float maxJumpHeight = 6;
	public float minJumpHeight = 2;
	public float timeToJumpApex = .6f;
	public float accelerationTimeAirborne = .2f;
	public float accelerationTimeGrounded = .1f;
	public float moveSpeed = 10;
	public float jumpGraceTime = .12f;
	public float jumpBufferTime = .12f;
	public float wallSlideMaxSpeed = 3;
	public float wallStickTime = .25f;
	public Vector2 wallJumpToward;
	public Vector2 wallJumpNeutral;
	public Vector2 wallJumpAway;

	float gravity;
	float maxJumpVelocity;
	float minJumpVelocity;
	float jumpGraceTimer;
	float maxJumpBufferTimer;
	float minJumpBufferTimer;
	float timeToWallUnstick;
	Vector3 velocity;
	Controller2D controller;

	void Start() 
	{
		controller = GetComponent<Controller2D>();
	}

	void CalculateGravityAndJump() 
	{
		// Calculate gravity and jump velocity by using jumpHeight, gravity and timeToJumpApex
		gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
		maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
		minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);
	}

	float Approach(float value, float target, float maxMove) 
	{
		// Return value +/- maxMove, capping value to target
		return value > target ? 
			Mathf.Max(value - maxMove, target) : Mathf.Min(value + maxMove, target);
	}

	void Update() 
	{
		// Get movement input
		Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

		// Handle wall interactions
		bool onWall = false;
		int wallDirX = controller.collisions.left ? -1 : 1;
		if (controller.collisions.left || controller.collisions.right) 
		{
			// Stop x velocity if player is touching a wall
			velocity.x = 0;
			
			if (!controller.collisions.below)
			{
				// Player is wall sliding if not touching the ground
				onWall = true;

				// Cap y velocity
				if (velocity.y < -wallSlideMaxSpeed)
				{
					velocity.y = -wallSlideMaxSpeed;
				}

				// Keep track of wall stick timer
				// Run timer if moving away from wall, otherwise reset timer
				if (timeToWallUnstick > 0)
				{
					if (input.x != wallDirX && input.x != 0)
					{
						timeToWallUnstick -= Time.deltaTime;
					}
					else
					{
						timeToWallUnstick = wallStickTime;
					}
				}
				else
				{
					timeToWallUnstick = wallStickTime;
				}
			}
		}

		// Stop y velocity if hitting ground or ceiling
		if (controller.collisions.above || controller.collisions.below) 
		{
			velocity.y = 0;
		}

		// Keep track of jump grace timer
		if (controller.collisions.below) 
		{
			jumpGraceTimer = jumpGraceTime;
		}
		else
		{
			jumpGraceTimer -= Time.deltaTime;
		}

		// TODO: For better performance, this should be in Start(). Used in Update() for 
		// flexible tweaking of values during runtime.
		CalculateGravityAndJump();

		// Get jump input and set buffer timer
		if (Input.GetButtonDown("Jump"))
		{
			// Wall jump if available
			if (onWall)
			{
				if (input.x == wallDirX)
				{
					// Jump while pushing against wall
					velocity.x = -wallDirX * wallJumpToward.x;
					velocity.y = wallJumpToward.y;
				}
				else if (input.x == 0)
				{
					// Jump while not moving
					velocity.x = -wallDirX * wallJumpNeutral.x;
					velocity.y = wallJumpNeutral.y;
				}
				else
				{
					// Jump while pushing away from wall
					velocity.x = -wallDirX * wallJumpAway.x;
					velocity.y = wallJumpAway.y;
				}				
			}
			else
			{
				// Set timer for ground jump
				maxJumpBufferTimer = jumpBufferTime;
			}
		}
		else if (Input.GetButtonUp("Jump"))
		{
			// Set timer for min jump
			minJumpBufferTimer = jumpBufferTime;
		}
		else
		{
			// Advance timers
			maxJumpBufferTimer -= Time.deltaTime;
			minJumpBufferTimer -= Time.deltaTime;
		}

		// Jumping from ground
		if (maxJumpBufferTimer >= 0 && jumpGraceTimer >= 0) 
		{
			// Jump if the player has been on the ground and pressed jump within a certain time
			velocity.y = minJumpBufferTimer >= 0 ? minJumpVelocity : maxJumpVelocity;

			// Zero out jump timers to prevent multiple jumps
			jumpGraceTimer = 0;
			maxJumpBufferTimer = 0;
			minJumpBufferTimer = 0;
		}
		else if (minJumpBufferTimer >= 0 && velocity.y > minJumpVelocity)
		{
			// Reduce the height of the jump if releasing the jump button
			velocity.y = minJumpVelocity;
			minJumpBufferTimer = 0;
		}

		// Keep track of the old velocity for final velocity calculation
		Vector3 oldVelocity = velocity;

		// Accelerate along the x axis
		// Approach the target velocity from the current velocity using acceleration
		float targetVelocityX = input.x * moveSpeed;
		float accelerationX = moveSpeed / 
			(controller.collisions.below ? accelerationTimeGrounded : accelerationTimeAirborne);
		velocity.x = Approach(velocity.x, targetVelocityX, accelerationX * Time.deltaTime);

		if (onWall && timeToWallUnstick > 0 && !Input.GetButtonDown("Jump"))
		{
			// Stick to wall
			velocity.x = 0;
		}
		
		// Accelerate along the y axis
		velocity.y += gravity * Time.deltaTime;

		// Move the player
		// Use the average between the old and new velocity for more accurate results
		controller.Move((oldVelocity + velocity) * 0.5f * Time.deltaTime);
	}
}
