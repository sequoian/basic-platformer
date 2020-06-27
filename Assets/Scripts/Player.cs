using UnityEngine;

[RequireComponent (typeof (Controller2D))]
public class Player : MonoBehaviour {

	[Header("Running")]
	public float moveSpeed = 12;
	public float accelGrounded = .1f;
	public float decelGrounded = .1f;
	public float accelAirborne = .2f;
	public float decelAirborne = .2f;

	[Header("Jumping")]
	public float maxJumpHeight = 4.5f;
	public float minJumpHeight = .5f;
	public float timeToJumpApex = .5f;
	public float jumpGraceTime = .1f;
	public float jumpBufferTime = .08f;
	public float halfGravityThreshold = 1;

	[Header("Wall Jumping")]
	public Vector2 wallJumpToward;
	public Vector2 wallJumpNeutral;
	public Vector2 wallJumpAway;
	public float wallStickTime = .15f;
	public float wallSlideMaxSpeed = 10;

	[Header("Falling")]
	public float terminalVelocity = 20f;
	
	// Private variables
	Vector3 velocity;
	Controller2D controller;
	float gravity;
	float maxJumpVelocity;
	float minJumpVelocity;
	float jumpGraceTimer;
	float jumpBufferTimer;
	float timeToWallUnstick;
	
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

	void Update()
	{
		// TODO: For better performance, this should be in Start(). Used in Update() for 
		// flexible tweaking of values during runtime.
		CalculateGravityAndJump();

		NormalUpdate();

	}

	void NormalUpdate() 
	{
		// Handle horizontal collisions
		bool onWall = false;
		int wallDirX = controller.collisions.left ? -1 : 1; 
		if (controller.collisions.left || controller.collisions.right) 
		{
			// Stop x velocity if player is touching a wall
			velocity.x = 0;
			
			if (!controller.collisions.below)
			{
				// Player is on wall if not touching the ground
				onWall = true;
			}
		}

		// Handle vertical collisions
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

		// Get movement input
		Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

		// Set jump timer
		if (Input.GetButtonDown("Jump"))
		{
			jumpBufferTimer = jumpBufferTime;
		}
		else if (Input.GetButtonUp("Jump"))
		{
			jumpBufferTimer = 0;
		}
		else
		{
			// Advance timers
			jumpBufferTimer -= Time.deltaTime;
		}

		// Jump
		bool wallJumping = false;
		if (onWall && jumpBufferTimer > 0)
		{
			// Wall jump
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

			jumpBufferTimer = 0;
			wallJumping = true;

		}
		else if (jumpBufferTimer > 0 && jumpGraceTimer > 0) 
		{
			// Jump from ground
			velocity.y = maxJumpVelocity;

			jumpGraceTimer = 0;
			jumpBufferTimer = 0;
		}
		else if (Input.GetButtonUp("Jump") && velocity.y > minJumpVelocity)
		{
			// Reduce the height of the jump if releasing the jump button
			velocity.y = minJumpVelocity;
		}

		// Keep track of the old velocity for final velocity calculation
		Vector3 oldVelocity = velocity;

		// Calulate horizontal acceleration
		float targetVelocityX = input.x * moveSpeed;
		float accelerationX = 0;
		if (input.x == 0)
		{
			// Decelerate
			accelerationX = moveSpeed / 
			(controller.collisions.below ? decelGrounded : decelAirborne);
		}
		else
		{
			// Accelerate
			accelerationX = moveSpeed / 
			(controller.collisions.below ? accelGrounded : accelAirborne);
		}

		// Approach the target velocity from the current velocity using acceleration
		velocity.x = Math.Approach(velocity.x, targetVelocityX, accelerationX * Time.deltaTime);

		// Sticky wall
		if (onWall)
		{
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

			// Stick to wall if the timer has not run out
			if (timeToWallUnstick > 0 && !wallJumping)
			{
				velocity.x = 0;
			}
		}
		
		// Half gravity at peak of jump
		float gravMultiplier = Mathf.Abs(velocity.y) < halfGravityThreshold
			&& Input.GetButton("Jump") ? 0.5f : 1f;

		// Apply gravity
		velocity.y += gravity * gravMultiplier * Time.deltaTime;

		// Cap vertical velocity
		if (onWall && velocity.y < -wallSlideMaxSpeed)
		{
			// Wall slide
			velocity.y = -wallSlideMaxSpeed;
		}
		else if (velocity.y < -terminalVelocity)
		{
			// Terminal velocity
			velocity.y = -terminalVelocity;
		}

		// Move the player
		// Use the average between the old and new velocity for more accurate results
		controller.Move((oldVelocity + velocity) * 0.5f * Time.deltaTime);
	}
}
