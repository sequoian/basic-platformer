using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCam : MonoBehaviour
{
    public Controller2D target;
	public float verticalOffset;
	public float lookAheadDistanceX;
	public float lookSmoothTimeX;
	public float verticalSmoothTime;
	public Vector2 focusAreaSize;

	FocusArea focusArea;

	float currentLookAheadX;
	float targetLookAheadX;
	float lookAheadDirX;
	float smoothLookVelocityX;
	float smoothVelocityY;
	bool lookAheadStopped;

	void Start() 
	{
        // NOTE: Make sure this Start() occurrs AFTER Controller2D's Start()
		focusArea = new FocusArea (target.collider2d.bounds, focusAreaSize);
	}

	void LateUpdate() 
	{
		// Update the position of the focus area
		focusArea.Update(target.collider2d.bounds);

		// Get the focus area position and apply vertical offset
		Vector2 cameraPosition = focusArea.center + Vector2.up * verticalOffset;

		// Calculate how far ahead the camera will be looking if the focus area is moving
		if (focusArea.velocity.x != 0) 
		{
			lookAheadDirX = Mathf.Sign(focusArea.velocity.x);
			float xInput = Input.GetAxisRaw("Horizontal");
			if (Mathf.Sign(xInput) == Mathf.Sign(focusArea.velocity.x) && xInput != 0) 
			{
				// If the player is moving in the direction of their input,
				// set the look ahead target to the full distance
				lookAheadStopped = false;
				targetLookAheadX = lookAheadDirX * lookAheadDistanceX;
			}
			else 
			{
				// If the player is not moving in the direction of their input,
				// set the look ahead target to a fraction of its previous target so that the
				// camera does not go the full length and comes to a smooth stop
				if (!lookAheadStopped) 
				{
					lookAheadStopped = true;
					targetLookAheadX = currentLookAheadX + 
						(lookAheadDirX * lookAheadDistanceX - currentLookAheadX) / 4f;
				}
			}
		}

		// Interpolate along the x axis toward targetLookAheadX so that the camera
		// ajusts toward its target smoothly over time
		currentLookAheadX = Mathf.SmoothDamp(currentLookAheadX, 
			targetLookAheadX, ref smoothLookVelocityX, lookSmoothTimeX);
		cameraPosition += Vector2.right * currentLookAheadX;
		
		// Smooth out the camera's movement along the y axis
		cameraPosition.y = Mathf.SmoothDamp(transform.position.y, 
			cameraPosition.y, ref smoothVelocityY, verticalSmoothTime);

		// Set the final camera position and make sure it is behind its target
		transform.position = (Vector3)cameraPosition + Vector3.forward * -10;
	}

	// void OnDrawGizmos() 
	// {
	// 	// Draw the focus area
	// 	Gizmos.color = new Color(1, 0, 0, .5f);
	// 	Gizmos.DrawCube(focusArea.center, focusAreaSize);
	// }

	struct FocusArea 
	{
		public Vector2 center;
		public Vector2 velocity;
		float left, right, top, bottom;

		public FocusArea(Bounds targetBounds, Vector2 size) 
		{
			// Calculate the initial x and y extents of the focus area
			left = targetBounds.center.x - size.x / 2;
			right = targetBounds.center.x + size.x / 2;
			bottom = targetBounds.min.y;
			top = targetBounds.min.y + size.y;

			// Zero out initial velocity
			velocity = Vector2.zero;

			// Calculate the initial center of the focus area by using the extents
			center = new Vector2((left + right) / 2, (top + bottom) / 2);
		}

		public void Update(Bounds targetBounds) 
		{
			// If the player crosses the left or right extent, 
			// shift the focus area along the x axis to keep the player inside
			float shiftX = 0;
			if (targetBounds.min.x < left) 
			{
				shiftX = targetBounds.min.x - left;
			} 
			else if (targetBounds.max.x > right) 
			{
				shiftX = targetBounds.max.x - right;
			}
			left += shiftX;
			right += shiftX;

			// If the player crosses the top or bottom extent,
			// shift the focus area along the y axis to keep the player inside
			float shiftY = 0;
			if (targetBounds.min.y < bottom) 
			{
				shiftY = targetBounds.min.y - bottom;
			} 
			else if (targetBounds.max.y > top) 
			{
				shiftY = targetBounds.max.y - top;
			}
			top += shiftY;
			bottom += shiftY;

			// Adjust the center and velocity in case the focus area moved
			center = new Vector2((left + right) / 2, (top + bottom) / 2);
			velocity = new Vector2(shiftX, shiftY);
		}
	}
}
