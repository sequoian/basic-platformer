using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Controller2D))]
public class Player : MonoBehaviour {

	public float jumpHeight = 4;
	public float timeToJumpApex = .4f;
	public float accelerationTimeAirborne = .2f;
	public float accelerationTimeGrounded = .1f;
	public float moveSpeed = 6;

	float gravity;
	float jumpVelocity;
	Vector3 velocity;
	Controller2D controller;

	void Start() 
	{
		controller = GetComponent<Controller2D>();
	}

	void CalculateGravityAndJump() 
	{
		// Calculate gravity and jump velocity by using jumpHeight, gravity and timeToJumpApex
		gravity = -(2 * jumpHeight) / Mathf.Pow(timeToJumpApex, 2);
		jumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
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
		if (controller.collisions.above || controller.collisions.below) {
			velocity.y = 0;
		}

		// Stop x velocity if hitting a wall
		if (controller.collisions.left || controller.collisions.right) {
			velocity.x = 0;
		}

		// TODO: For better performance, this should be in Start(). Used in Update() for 
		// flexible tweaking of values during runtime.
		CalculateGravityAndJump();

		// Get movement input
		Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

		// Jump if the player presses space and is on the ground
		if (Input.GetButtonDown("Jump") && controller.collisions.below) {
			velocity.y = jumpVelocity;
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
