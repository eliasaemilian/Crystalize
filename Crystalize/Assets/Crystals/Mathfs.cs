using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Mathfs 
{
    public const float PI = 3.14159265359f;
    public const float TAU = 6.28318530718f;

    public static Vector2 GetUnitVectorByAngle(float angRad)
    {
        return new Vector2(
                Mathf.Cos(angRad),
                Mathf.Sin(angRad)
                );
    }

	public static float GetAngleByUnitVector(Vector2 vector)
    {
		return Mathf.Atan2(vector.y, vector.x);
    }

    public static float Remap(float value, float start, float end, float mapToStart, float mapToEnd)
    {
        // low2 + (value - low1) * (high2 - low2) / (high1 - low1) thank u processing
        // Remaps a value on range to the according value on a different range
        return mapToStart + (value - start) * (mapToEnd - mapToStart) / (end - start);
    }

	public static bool LineLineIntersection(out Vector3 intersection, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
	{

		Vector3 lineVec3 = linePoint2 - linePoint1;
		Vector3 crossVec1and2 = Vector3.Cross(lineVec1, lineVec2);
		Vector3 crossVec3and2 = Vector3.Cross(lineVec3, lineVec2);

		float planarFactor = Vector3.Dot(lineVec3, crossVec1and2);

		//is coplanar, and not parrallel
		if (Mathf.Abs(planarFactor) < 0.0001f && crossVec1and2.sqrMagnitude > 0.0001f)
		{
			float s = Vector3.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude;
			intersection = linePoint1 + (lineVec1 * s);
			return true;
		}
		else
		{
			intersection = Vector3.zero;
			return false;
		}
	}

	//Two non-parallel lines which may or may not touch each other have a point on each line which are closest
	//to each other. This function finds those two points. If the lines are not parallel, the function 
	//outputs true, otherwise false.
	public static bool ClosestPointsOnTwoLines(out Vector3 closestPointLine1, out Vector3 closestPointLine2, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
	{

		closestPointLine1 = Vector3.zero;
		closestPointLine2 = Vector3.zero;

		float a = Vector3.Dot(lineVec1, lineVec1);
		float b = Vector3.Dot(lineVec1, lineVec2);
		float e = Vector3.Dot(lineVec2, lineVec2);

		float d = a * e - b * b;

		//lines are not parallel
		if (d != 0.0f)
		{

			Vector3 r = linePoint1 - linePoint2;
			float c = Vector3.Dot(lineVec1, r);
			float f = Vector3.Dot(lineVec2, r);

			float s = (b * f - c * e) / d;
			float t = (a * f - c * b) / d;

			closestPointLine1 = linePoint1 + lineVec1 * s;
			closestPointLine2 = linePoint2 + lineVec2 * t;

			return true;
		}

		else
		{
			return false;
		}
	}


	public static Vector3 GetBezierPointWithOneCP(Vector3 start, Vector3 CP, Vector3 end, float t)
	{

		// 2D tho Vector3 p = ((1 - t) * 2 * start) + (2 * (1 - t) * t * CP) + (t * 2 * end);

        Vector3 a = Vector3.Lerp(start, CP, t);
        Vector3 b = Vector3.Lerp(CP, end, t);

        Vector3 pos = Vector3.Lerp(a, b, t);
      //  Vector3 tangent = (a - b).normalized;

        return pos;
	}

	public static Vector3 NearestPointOnFiniteLine(out float distance, Vector3 start, Vector3 end, Vector3 point)
	{
		var line = (end - start);
		var len = line.magnitude;
		line.Normalize();

		var v = point - start;
		var d = Vector3.Dot(v, line);
		d = Mathf.Clamp(d, 0f, len);
		var perp = (start + line * d);
		//distance = ((Vector2)v0 - cut).magnitude;
		//distance = (v - perp).magnitude;
		distance = Vector3.Distance(point, perp);
		return perp;
	}

	public static Vector3 GetBezierPointWithTwoCP(Vector3 start, Vector3 p1, Vector3 p2, Vector3 end, float t)
	{
		Vector3 a = Vector3.Lerp(start, p1, t);
		Vector3 b = Vector3.Lerp(p1, p2, t);
		Vector3 c = Vector3.Lerp(p2, end, t);

		Vector3 d = Vector3.Lerp(a, b, t);
		Vector3 e = Vector3.Lerp(b, c, t);

		Vector3 pos = Vector3.Lerp(d, e, t);
		//Vector3 tangent = (e - d).normalized;

		//return new OrientedPoint(pos, tangent);
		return pos;
	}


	//This function finds out on which side of a line segment the point is located.
	//The point is assumed to be on a line created by linePoint1 and linePoint2. If the point is not on
	//the line segment, project it on the line using ProjectPointOnLine() first.
	//Returns 0 if point is on the line segment.
	//Returns 1 if point is outside of the line segment and located on the side of linePoint1.
	//Returns 2 if point is outside of the line segment and located on the side of linePoint2.
	public static int PointOnWhichSideOfLineSegment(Vector3 linePoint1, Vector3 linePoint2, Vector3 point)
	{

		Vector3 lineVec = linePoint2 - linePoint1;
		Vector3 pointVec = point - linePoint1;

		float dot = Vector3.Dot(pointVec, lineVec);

		//point is on side of linePoint2, compared to linePoint1
		if (dot > 0)
		{

			//point is on the line segment
			if (pointVec.magnitude <= lineVec.magnitude)
			{

				return 0;
			}

			//point is not on the line segment and it is on the side of linePoint2
			else
			{

				return 2;
			}
		}

		//Point is not on side of linePoint2, compared to linePoint1.
		//Point is not on the line segment and it is on the side of linePoint1.
		else
		{

			return 1;
		}
	}

	//Returns the pixel distance from the mouse pointer to a line.
	//Alternative for HandleUtility.DistanceToLine(). Works both in Editor mode and Play mode.
	//Do not call this function from OnGUI() as the mouse position will be wrong.
	public static float MouseDistanceToLine(out Vector3 projectedPoint, Vector3 linePoint1, Vector3 linePoint2, Vector3 mousePosition) 
	{

		Camera currentCamera;
		//Vector3 mousePosition;

#if UNITY_EDITOR
		if (Camera.current != null)
		{

			currentCamera = Camera.current;
		}

		else
		{

			currentCamera = Camera.main;
		}

		//convert format because y is flipped
		//mousePosition = new Vector3(Event.current.mousePosition.x, currentCamera.pixelHeight - Event.current.mousePosition.y, 0f);
		mousePosition = new Vector3(mousePosition.x, currentCamera.pixelHeight - mousePosition.y, 0f);

#else
		currentCamera = Camera.main;
		mousePosition = Input.mousePosition;
#endif

		Vector3 screenPos1 = currentCamera.WorldToScreenPoint(linePoint1);
		Vector3 screenPos2 = currentCamera.WorldToScreenPoint(linePoint2);
		projectedPoint = ProjectPointOnLineSegment(screenPos1, screenPos2, mousePosition);

		//set z to zero
		Vector3 newprojectedPoint = new Vector3(projectedPoint.x, projectedPoint.y, 0f);

		Vector3 vector = newprojectedPoint - mousePosition;
		return vector.magnitude;
	}

	//This function returns a point which is a projection from a point to a line segment.
	//If the projected point lies outside of the line segment, the projected point will 
	//be clamped to the appropriate line edge.
	//If the line is infinite instead of a segment, use ProjectPointOnLine() instead.
	public static Vector3 ProjectPointOnLineSegment(Vector3 linePoint1, Vector3 linePoint2, Vector3 point)
	{

		Vector3 vector = linePoint2 - linePoint1;

		Vector3 projectedPoint = ProjectPointOnLine(linePoint1, vector.normalized, point);

		int side = PointOnWhichSideOfLineSegment(linePoint1, linePoint2, projectedPoint);

		//The projected point is on the line segment
		if (side == 0)
		{

			return projectedPoint;
		}

		if (side == 1)
		{

			return linePoint1;
		}

		if (side == 2)
		{

			return linePoint2;
		}

		//output is invalid
		return Vector3.zero;
	}

	//This function returns a point which is a projection from a point to a line.
	//The line is regarded infinite. If the line is finite, use ProjectPointOnLineSegment() instead.
	public static Vector3 ProjectPointOnLine(Vector3 linePoint, Vector3 lineVec, Vector3 point)
	{

		//get vector from point on line to point in space
		Vector3 linePointToPoint = point - linePoint;

		float t = Vector3.Dot(linePointToPoint, lineVec);

		return linePoint + lineVec * t;
	}

}
