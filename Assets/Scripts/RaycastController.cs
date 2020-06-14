using UnityEngine;

[RequireComponent (typeof (BoxCollider2D))]
public class RaycastController : MonoBehaviour {

	public LayerMask collisionMask;
	public const float skinWidth = .015f;
	public int horizontalRayCount = 4;
	public int verticalRayCount = 4;

	[HideInInspector]
	public float horizontalRaySpacing;
	[HideInInspector]
	public float verticalRaySpacing;
	[HideInInspector]
	public BoxCollider2D collider2d;
    [HideInInspector]
	public RaycastOrigins raycastOrigins;

	public virtual void Start() {
		collider2d = GetComponent<BoxCollider2D>();
		CalculateRaySpacing();
	}

    // Calculates the positions of each corner of the collider
	public void UpdateRaycastOrigins() 
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
	public void CalculateRaySpacing() 
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
	public struct RaycastOrigins 
	{
		public Vector2 topLeft, topRight;
		public Vector2 bottomLeft, bottomRight;
	}
}
