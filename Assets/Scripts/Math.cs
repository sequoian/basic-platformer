using UnityEngine;

public class Math
{
    public static float Approach(float value, float target, float maxMove) 
	{
		// Return value +/- maxMove, capping value to target
		return value > target ? 
			Mathf.Max(value - maxMove, target) : Mathf.Min(value + maxMove, target);
	}
}
