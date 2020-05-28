using UnityEngine;
using System.Collections;

[RequireComponent (typeof (BoxCollider2D))]
public class Controller2D : MonoBehaviour {

	public LayerMask collisionMask;
	public CollisionInfo collisions;
	public int horizontalRayCount = 4;
	public int verticalRayCount = 4;

	const float skinWidth = .015f;
	float horizontalRaySpacing;
	float verticalRaySpacing;
	BoxCollider2D collider2d;
	RaycastOrigins raycastOrigins;
	
	void Start() 
	{
		collider2d = GetComponent<BoxCollider2D>();
		CalculateRaySpacing();
	}

	public void Move(Vector3 velocity) 
	{
		// Prepare for collisions
		UpdateRaycastOrigins();
		collisions.Reset();

		// Detect and resolve collisions
		if (velocity.x != 0) {
			HorizontalCollisions(ref velocity);
		}
		if (velocity.y != 0) {
			VerticalCollisions(ref velocity);
		}

		// Move player
		transform.Translate(velocity);
	}

	void HorizontalCollisions(ref Vector3 velocity) 
	{
		// Set the length and direction for the raycast
		float directionX = Mathf.Sign(velocity.x);
		float rayLength = Mathf.Abs(velocity.x) + skinWidth;
		
		// For each horizontal ray
		for (int i = 0; i < horizontalRayCount; i ++) {
			// Set the origin of the ray
			Vector2 rayOrigin = (directionX == -1) ? 
				raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
			rayOrigin += Vector2.up * (horizontalRaySpacing * i);

			// Cast the ray in the direction of horizontal movement
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

	// Calculates the positions of each corner of the collider
	void UpdateRaycastOrigins() 
	{
		// Get the size of the collider, contracted by twice the skin width
		Bounds bounds = collider2d.bounds;
		bounds.Expand(skinWidth * -2);

		// Set the values of raycastOrigins struct
		raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
		raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
		raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
		raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
	}

	// Calulates the information need to figure out where each collision ray should be positioned
	void CalculateRaySpacing() 
	{
		// Get the size of the collider, contracted by twice the skin width
		Bounds bounds = collider2d.bounds;
		bounds.Expand(skinWidth * -2);

		// Get ray count on each side
		horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
		verticalRayCount = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);

		// Get the spacing between each ray
		horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
		verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
	}

	// Represents the position of each corner of the bounding box
	struct RaycastOrigins 
	{
		public Vector2 topLeft, topRight;
		public Vector2 bottomLeft, bottomRight;
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
