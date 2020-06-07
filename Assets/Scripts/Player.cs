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

	float gravity;
	float maxJumpVelocity;
	float minJumpVelocity;
	float jumpGraceTimer;
	float maxJumpBufferTimer;
	float minJumpBufferTimer;
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
		// Stop y velocity if hitting ground or ceiling
		if (controller.collisions.above || controller.collisions.below) 
		{
			velocity.y = 0;
		}

		// Stop x velocity if hitting a wall
		if (controller.collisions.left || controller.collisions.right) 
		{
			velocity.x = 0;
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

		// Get movement input
		Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

		// Get jump input and set buffer timer
		if (Input.GetButtonDown("Jump"))
		{
			maxJumpBufferTimer = jumpBufferTime;
		}
		else if (Input.GetButtonUp("Jump"))
		{
			minJumpBufferTimer = jumpBufferTime;
		}
		else
		{
			maxJumpBufferTimer -= Time.deltaTime;
			minJumpBufferTimer -= Time.deltaTime;
		}

		// Jumping
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
		
		// Accelerate along the y axis
		velocity.y += gravity * Time.deltaTime;

		// Move the player
		// Use the average between the old and new velocity for more accurate results
		controller.Move((oldVelocity + velocity) * 0.5f * Time.deltaTime);
	}
}
