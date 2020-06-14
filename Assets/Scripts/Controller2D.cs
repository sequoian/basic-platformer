using UnityEngine;

[RequireComponent (typeof (BoxCollider2D))]
public class Controller2D : RaycastController {

	public CollisionInfo collisions;

	[HideInInspector]
	public int faceDirection;
	
	public override void Start() 
	{
		base.Start();
		faceDirection = 1;
	}

	public void Move(Vector3 velocity) 
	{
		// Prepare for collisions
		UpdateRaycastOrigins();
		collisions.Reset();

		if (velocity.x != 0)
		{
			// Set which direction the player is facing
			faceDirection = (int)Mathf.Sign(velocity.x);
		}

		// Detect and resolve collisions
		HorizontalCollisions(ref velocity);

		if (velocity.y != 0) {
			VerticalCollisions(ref velocity);
		}

		// Move player
		transform.Translate(velocity);
	}

	void HorizontalCollisions(ref Vector3 velocity) 
	{
		// Set the length and direction for the raycast
		float directionX = faceDirection;
		float rayLength = Mathf.Abs(velocity.x) + skinWidth;
		if (Mathf.Abs(velocity.x) < skinWidth)
		{
			// Set minimal ray length if not moving
			rayLength = skinWidth * 2;
		}
		
		// For each horizontal ray
		for (int i = 0; i < horizontalRayCount; i ++) {
			// Set the origin of the ray
			Vector2 rayOrigin = (directionX == -1) ? 
				raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
			rayOrigin += Vector2.up * (horizontalRaySpacing * i);

			// Cast the ray in the direction of horizontal movement and detect collisions
			RaycastHit2D hit = Physics2D.Raycast(
				rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

			Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.green);

			if (hit) {
				// set the velocity to the length between both colliders to avoid overlap
				velocity.x = (hit.distance - skinWidth) * directionX;

				// Reduce the ray length for the following raycasts to only detect closer collisions
				rayLength = hit.distance;

				// Set collision info for which side had collided
				collisions.left = directionX == -1;
				collisions.right = directionX == 1;
			}
		}
	}

	void VerticalCollisions(ref Vector3 velocity) 
	{
		// Set the length and direction for the raycast
		float directionY = Mathf.Sign(velocity.y);
		float rayLength = Mathf.Abs(velocity.y) + skinWidth;

		for (int i = 0; i < verticalRayCount; i ++) {
			// Set the origin of the ray
			Vector2 rayOrigin = (directionY == -1) ? 
				raycastOrigins.bottomLeft : raycastOrigins.topLeft;
			rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);

			// Cast the ray in the direction of horizontal movement
			RaycastHit2D hit = Physics2D.Raycast(
				rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

			Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.blue);

			if (hit) {
				// set the velocity to the length between both colliders to avoid overlap
				velocity.y = (hit.distance - skinWidth) * directionY;

				// Reduce the ray length for the following raycasts to only detect closer collisions
				rayLength = hit.distance;

				// Set collision info for which side had collided
				collisions.below = directionY == -1;
				collisions.above = directionY == 1;
			}
		}
	}

	// Represents which side of the collider was hit by a collision
	public struct CollisionInfo 
	{
		public bool above, below;
		public bool left, right;

		// Sets all values to false
		public void Reset() 
		{
			above = below = false;
			left = right = false;
		}
	}

}
