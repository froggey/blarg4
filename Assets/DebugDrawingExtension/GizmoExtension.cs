using UnityEngine;
using System.Collections;
using System.Reflection;

/// <summary>
/// Gizmos Extension
/// 	- Static class that extends Unity's debugging functionallity.
/// 	- Attempts to mimic Unity's existing debugging behaviour for ease-of-use.
/// 	- Includes gizmo drawing methods for less memory-intensive debug visualization.
/// </summary>

public static class GizmosExtension
{
	#region GizmosDrawFunctions

	/// <summary>
	/// 	- Gizmoss a point.
	/// </summary>
	/// <param name='position'>
	/// 	- The point to debug.
	/// </param>
	/// <param name='color'>
	/// 	- The color of the point.
	/// </param>
	/// <param name='scale'>
	/// 	- The size of the point.
	/// </param>
	/// <param name='duration'>
	/// 	- How long to draw the point.
	/// </param>
	/// <param name='depthTest'>
	/// 	- Whether or not this point should be faded when behind other objects.
	/// </param>
	public static void GizmosPoint(Vector3 position, float scale = 1.0f)
	{
		Gizmos.DrawRay(position+(Vector3.up*(scale*0.5f)), -Vector3.up*scale);
		Gizmos.DrawRay(position+(Vector3.right*(scale*0.5f)), -Vector3.right*scale);
		Gizmos.DrawRay(position+(Vector3.forward*(scale*0.5f)), -Vector3.forward*scale);
	}

	/// <summary>
	/// 	- Gizmoss an axis-aligned bounding box.
	/// </summary>
	/// <param name='bounds'>
	/// 	- The bounds to debug.
	/// </param>
	/// <param name='color'>
	/// 	- The color of the bounds.
	/// </param>
	/// <param name='duration'>
	/// 	- How long to draw the bounds.
	/// </param>
	/// <param name='depthTest'>
	/// 	- Whether or not the bounds should be faded when behind other objects.
	/// </param>
	public static void GizmosBounds(Bounds bounds)
	{
		Vector3 center = bounds.center;

		float x = bounds.extents.x;
		float y = bounds.extents.y;
		float z = bounds.extents.z;

		Vector3 ruf = center+new Vector3(x,y,z);
		Vector3 rub = center+new Vector3(x,y,-z);
		Vector3 luf = center+new Vector3(-x,y,z);
		Vector3 lub = center+new Vector3(-x,y,-z);

		Vector3 rdf = center+new Vector3(x,-y,z);
		Vector3 rdb = center+new Vector3(x,-y,-z);
		Vector3 lfd = center+new Vector3(-x,-y,z);
		Vector3 lbd = center+new Vector3(-x,-y,-z);

		Gizmos.DrawLine(ruf, luf);
		Gizmos.DrawLine(ruf, rub);
		Gizmos.DrawLine(luf, lub);
		Gizmos.DrawLine(rub, lub);

		Gizmos.DrawLine(ruf, rdf);
		Gizmos.DrawLine(rub, rdb);
		Gizmos.DrawLine(luf, lfd);
		Gizmos.DrawLine(lub, lbd);

		Gizmos.DrawLine(rdf, lfd);
		Gizmos.DrawLine(rdf, rdb);
		Gizmos.DrawLine(lfd, lbd);
		Gizmos.DrawLine(lbd, rdb);
	}

	/// <summary>
	/// 	- Gizmoss a local cube.
	/// </summary>
	/// <param name='transform'>
	/// 	- The transform that the cube will be local to.
	/// </param>
	/// <param name='size'>
	/// 	- The size of the cube.
	/// </param>
	/// <param name='center'>
	/// 	- The position (relative to transform) where the cube will be debugged.
	/// </param>
	/// <param name='duration'>
	/// 	- How long to draw the cube.
	/// </param>
	/// <param name='depthTest'>
	/// 	- Whether or not the cube should be faded when behind other objects.
	/// </param>
	public static void GizmosLocalCube(Transform transform, Vector3 size, Vector3 center = default(Vector3))
	{
		Vector3 lbb = transform.TransformPoint(center+((-size)*0.5f));
		Vector3 rbb = transform.TransformPoint(center+(new Vector3(size.x, -size.y, -size.z)*0.5f));

		Vector3 lbf = transform.TransformPoint(center+(new Vector3(size.x, -size.y, size.z)*0.5f));
		Vector3 rbf = transform.TransformPoint(center+(new Vector3(-size.x, -size.y, size.z)*0.5f));

		Vector3 lub = transform.TransformPoint(center+(new Vector3(-size.x, size.y, -size.z)*0.5f));
		Vector3 rub = transform.TransformPoint(center+(new Vector3(size.x, size.y, -size.z)*0.5f));

		Vector3 luf = transform.TransformPoint(center+((size)*0.5f));
		Vector3 ruf = transform.TransformPoint(center+(new Vector3(-size.x, size.y, size.z)*0.5f));

		Gizmos.DrawLine(lbb, rbb);
		Gizmos.DrawLine(rbb, lbf);
		Gizmos.DrawLine(lbf, rbf);
		Gizmos.DrawLine(rbf, lbb);

		Gizmos.DrawLine(lub, rub);
		Gizmos.DrawLine(rub, luf);
		Gizmos.DrawLine(luf, ruf);
		Gizmos.DrawLine(ruf, lub);

		Gizmos.DrawLine(lbb, lub);
		Gizmos.DrawLine(rbb, rub);
		Gizmos.DrawLine(lbf, luf);
		Gizmos.DrawLine(rbf, ruf);
	}

	/// <summary>
	/// 	- Gizmoss a local cube.
	/// </summary>
	/// <param name='space'>
	/// 	- The space the cube will be local to.
	/// </param>
	/// <param name='size'>
	///		- The size of the cube.
	/// </param>
	/// <param name='color'>
	/// 	- Color of the cube.
	/// </param>
	/// <param name='center'>
	/// 	- The position (relative to transform) where the cube will be debugged.
	/// </param>
	/// <param name='duration'>
	/// 	- How long to draw the cube.
	/// </param>
	/// <param name='depthTest'>
	/// 	- Whether or not the cube should be faded when behind other objects.
	/// </param>
	public static void GizmosLocalCube(Matrix4x4 space, Vector3 size, Vector3 center = default(Vector3))
	{
		Vector3 lbb = space.MultiplyPoint3x4(center+((-size)*0.5f));
		Vector3 rbb = space.MultiplyPoint3x4(center+(new Vector3(size.x, -size.y, -size.z)*0.5f));

		Vector3 lbf = space.MultiplyPoint3x4(center+(new Vector3(size.x, -size.y, size.z)*0.5f));
		Vector3 rbf = space.MultiplyPoint3x4(center+(new Vector3(-size.x, -size.y, size.z)*0.5f));

		Vector3 lub = space.MultiplyPoint3x4(center+(new Vector3(-size.x, size.y, -size.z)*0.5f));
		Vector3 rub = space.MultiplyPoint3x4(center+(new Vector3(size.x, size.y, -size.z)*0.5f));

		Vector3 luf = space.MultiplyPoint3x4(center+((size)*0.5f));
		Vector3 ruf = space.MultiplyPoint3x4(center+(new Vector3(-size.x, size.y, size.z)*0.5f));

		Gizmos.DrawLine(lbb, rbb);
		Gizmos.DrawLine(rbb, lbf);
		Gizmos.DrawLine(lbf, rbf);
		Gizmos.DrawLine(rbf, lbb);

		Gizmos.DrawLine(lub, rub);
		Gizmos.DrawLine(rub, luf);
		Gizmos.DrawLine(luf, ruf);
		Gizmos.DrawLine(ruf, lub);

		Gizmos.DrawLine(lbb, lub);
		Gizmos.DrawLine(rbb, rub);
		Gizmos.DrawLine(lbf, luf);
		Gizmos.DrawLine(rbf, ruf);
	}

	/// <summary>
	/// 	- Gizmoss a circle.
	/// </summary>
	/// <param name='position'>
	/// 	- Where the center of the circle will be positioned.
	/// </param>
	/// <param name='up'>
	/// 	- The direction perpendicular to the surface of the circle.
	/// </param>
	/// <param name='color'>
	/// 	- The color of the circle.
	/// </param>
	/// <param name='radius'>
	/// 	- The radius of the circle.
	/// </param>
	/// <param name='duration'>
	/// 	- How long to draw the circle.
	/// </param>
	/// <param name='depthTest'>
	/// 	- Whether or not the circle should be faded when behind other objects.
	/// </param>
	public static void GizmosCircle(Vector3 position, Vector3 up, float radius = 1.0f)
	{
		Vector3 _up = up.normalized * radius;
		Vector3 _forward = Vector3.Slerp(_up, -_up, 0.5f);
		Vector3 _right = Vector3.Cross(_up, _forward).normalized*radius;

		Matrix4x4 matrix = new Matrix4x4();

		matrix[0] = _right.x;
		matrix[1] = _right.y;
		matrix[2] = _right.z;

		matrix[4] = _up.x;
		matrix[5] = _up.y;
		matrix[6] = _up.z;

		matrix[8] = _forward.x;
		matrix[9] = _forward.y;
		matrix[10] = _forward.z;

		Vector3 _lastPoint = position + matrix.MultiplyPoint3x4(new Vector3(Mathf.Cos(0), 0, Mathf.Sin(0)));
		Vector3 _nextPoint = Vector3.zero;

		for(var i = 0; i < 91; i++){
			_nextPoint.x = Mathf.Cos((i*4)*Mathf.Deg2Rad);
			_nextPoint.z = Mathf.Sin((i*4)*Mathf.Deg2Rad);
			_nextPoint.y = 0;

			_nextPoint = position + matrix.MultiplyPoint3x4(_nextPoint);

			Gizmos.DrawLine(_lastPoint, _nextPoint);
			_lastPoint = _nextPoint;
		}
	}

	/// <summary>
	/// 	- Gizmoss a circle.
	/// </summary>
	/// <param name='position'>
	/// 	- Where the center of the circle will be positioned.
	/// </param>
	/// <param name='radius'>
	/// 	- The radius of the circle.
	/// </param>
	/// <param name='duration'>
	/// 	- How long to draw the circle.
	/// </param>
	/// <param name='depthTest'>
	/// 	- Whether or not the circle should be faded when behind other objects.
	/// </param>
	public static void GizmosCircle(Vector3 position, float radius = 1.0f)
	{
		GizmosCircle(position, Vector3.up, radius);
	}

	/// <summary>
	/// 	- Gizmoss a wire sphere.
	/// </summary>
	/// <param name='position'>
	/// 	- The position of the center of the sphere.
	/// </param>
	/// <param name='color'>
	/// 	- The color of the sphere.
	/// </param>
	/// <param name='radius'>
	/// 	- The radius of the sphere.
	/// </param>
	/// <param name='duration'>
	/// 	- How long to draw the sphere.
	/// </param>
	/// <param name='depthTest'>
	/// 	- Whether or not the sphere should be faded when behind other objects.
	/// </param>
	public static void GizmosWireSphere(Vector3 position, float radius = 1.0f)
	{
		float angle = 10.0f;

		Vector3 x = new Vector3(position.x, position.y + radius * Mathf.Sin(0), position.z + radius * Mathf.Cos(0));
		Vector3 y = new Vector3(position.x + radius * Mathf.Cos(0), position.y, position.z + radius * Mathf.Sin(0));
		Vector3 z = new Vector3(position.x + radius * Mathf.Cos(0), position.y + radius * Mathf.Sin(0), position.z);

		Vector3 new_x;
		Vector3 new_y;
		Vector3 new_z;

		for(int i = 1; i < 37; i++){

			new_x = new Vector3(position.x, position.y + radius * Mathf.Sin(angle*i*Mathf.Deg2Rad), position.z + radius * Mathf.Cos(angle*i*Mathf.Deg2Rad));
			new_y = new Vector3(position.x + radius * Mathf.Cos(angle*i*Mathf.Deg2Rad), position.y, position.z + radius * Mathf.Sin(angle*i*Mathf.Deg2Rad));
			new_z = new Vector3(position.x + radius * Mathf.Cos(angle*i*Mathf.Deg2Rad), position.y + radius * Mathf.Sin(angle*i*Mathf.Deg2Rad), position.z);

			Gizmos.DrawLine(x, new_x);
			Gizmos.DrawLine(y, new_y);
			Gizmos.DrawLine(z, new_z);

			x = new_x;
			y = new_y;
			z = new_z;
		}
	}

	/// <summary>
	/// 	- Gizmoss a cylinder.
	/// </summary>
	/// <param name='start'>
	/// 	- The position of one end of the cylinder.
	/// </param>
	/// <param name='end'>
	/// 	- The position of the other end of the cylinder.
	/// </param>
	/// <param name='color'>
	/// 	- The color of the cylinder.
	/// </param>
	/// <param name='radius'>
	/// 	- The radius of the cylinder.
	/// </param>
	/// <param name='duration'>
	/// 	- How long to draw the cylinder.
	/// </param>
	/// <param name='depthTest'>
	/// 	- Whether or not the cylinder should be faded when behind other objects.
	/// </param>
	public static void GizmosCylinder(Vector3 start, Vector3 end, float radius = 1)
	{
		Vector3 up = (end-start).normalized*radius;
		Vector3 forward = Vector3.Slerp(up, -up, 0.5f);
		Vector3 right = Vector3.Cross(up, forward).normalized*radius;

		//Radial circles
		GizmosExtension.GizmosCircle(start, up, radius);
		GizmosExtension.GizmosCircle(end, -up, radius);
		GizmosExtension.GizmosCircle((start+end)*0.5f, up, radius);

		//Side lines
		Gizmos.DrawLine(start+right, end+right);
		Gizmos.DrawLine(start-right, end-right);

		Gizmos.DrawLine(start+forward, end+forward);
		Gizmos.DrawLine(start-forward, end-forward);

		//Start endcap
		Gizmos.DrawLine(start-right, start+right);
		Gizmos.DrawLine(start-forward, start+forward);

		//End endcap
		Gizmos.DrawLine(end-right, end+right);
		Gizmos.DrawLine(end-forward, end+forward);
	}

	/// <summary>
	/// 	- Gizmoss a cone.
	/// </summary>
	/// <param name='position'>
	/// 	- The position for the tip of the cone.
	/// </param>
	/// <param name='direction'>
	/// 	- The direction for the cone gets wider in.
	/// </param>
	/// <param name='angle'>
	/// 	- The angle of the cone.
	/// </param>
	/// <param name='color'>
	/// 	- The color of the cone.
	/// </param>
	/// <param name='duration'>
	/// 	- How long to draw the cone.
	/// </param>
	/// <param name='depthTest'>
	/// 	- Whether or not the cone should be faded when behind other objects.
	/// </param>
	public static void GizmosCone(Vector3 position, Vector3 direction, float angle = 45)
	{
		float length = direction.magnitude;

		Vector3 _forward = direction;
		Vector3 _up = Vector3.Slerp(_forward, -_forward, 0.5f);
		Vector3 _right = Vector3.Cross(_forward, _up).normalized*length;

		direction = direction.normalized;

		Vector3 slerpedVector = Vector3.Slerp(_forward, _up, angle/90.0f);

		float dist;
		var farPlane = new Plane(-direction, position+_forward);
		var distRay = new Ray(position, slerpedVector);

		farPlane.Raycast(distRay, out dist);

		Gizmos.DrawRay(position, slerpedVector.normalized*dist);
		Gizmos.DrawRay(position, Vector3.Slerp(_forward, -_up, angle/90.0f).normalized*dist);
		Gizmos.DrawRay(position, Vector3.Slerp(_forward, _right, angle/90.0f).normalized*dist);
		Gizmos.DrawRay(position, Vector3.Slerp(_forward, -_right, angle/90.0f).normalized*dist);

		GizmosExtension.GizmosCircle(position+_forward, direction, (_forward-(slerpedVector.normalized*dist)).magnitude);
		GizmosExtension.GizmosCircle(position+(_forward*0.5f), direction, ((_forward*0.5f)-(slerpedVector.normalized*(dist*0.5f))).magnitude);
	}

	/// <summary>
	/// 	- Gizmoss a cone.
	/// </summary>
	/// <param name='position'>
	/// 	- The position for the tip of the cone.
	/// </param>
	/// <param name='angle'>
	/// 	- The angle of the cone.
	/// </param>
	/// <param name='color'>
	/// 	- The color of the cone.
	/// </param>
	/// <param name='duration'>
	/// 	- How long to draw the cone.
	/// </param>
	/// <param name='depthTest'>
	/// 	- Whether or not the cone should be faded when behind other objects.
	/// </param>
	public static void GizmosCone(Vector3 position, float angle = 45)
	{
		GizmosCone(position, Vector3.up, angle);
	}

	#endregion
/*
	/// <summary>
	/// 	- Gizmoss an arrow.
	/// </summary>
	/// <param name='position'>
	/// 	- The start position of the arrow.
	/// </param>
	/// <param name='direction'>
	/// 	- The direction the arrow will point in.
	/// </param>
	/// <param name='color'>
	/// 	- The color of the arrow.
	/// </param>
	/// <param name='duration'>
	/// 	- How long to draw the arrow.
	/// </param>
	/// <param name='depthTest'>
	/// 	- Whether or not the arrow should be faded when behind other objects.
	/// </param>
	public static void GizmosArrow(Vector3 position, Vector3 direction, Color color, float duration = 0, bool depthTest = true)
	{
		Gizmos.DrawRay(position, direction, color, duration, depthTest);
		GizmosExtension.GizmosCone(position+direction, -direction*0.333f, color, 15, duration, depthTest);
	}

	/// <summary>
	/// 	- Gizmoss an arrow.
	/// </summary>
	/// <param name='position'>
	/// 	- The start position of the arrow.
	/// </param>
	/// <param name='direction'>
	/// 	- The direction the arrow will point in.
	/// </param>
	/// <param name='duration'>
	/// 	- How long to draw the arrow.
	/// </param>
	/// <param name='depthTest'>
	/// 	- Whether or not the arrow should be faded when behind other objects.
	/// </param>
	public static void GizmosArrow(Vector3 position, Vector3 direction, float duration = 0, bool depthTest = true)
	{
		GizmosArrow(position, direction, Color.white, duration, depthTest);
	}

	/// <summary>
	/// 	- Gizmoss a capsule.
	/// </summary>
	/// <param name='start'>
	/// 	- The position of one end of the capsule.
	/// </param>
	/// <param name='end'>
	/// 	- The position of the other end of the capsule.
	/// </param>
	/// <param name='color'>
	/// 	- The color of the capsule.
	/// </param>
	/// <param name='radius'>
	/// 	- The radius of the capsule.
	/// </param>
	/// <param name='duration'>
	/// 	- How long to draw the capsule.
	/// </param>
	/// <param name='depthTest'>
	/// 	- Whether or not the capsule should be faded when behind other objects.
	/// </param>
	public static void GizmosCapsule(Vector3 start, Vector3 end, Color color, float radius = 1, float duration = 0, bool depthTest = true)
	{
		Vector3 up = (end-start).normalized*radius;
		Vector3 forward = Vector3.Slerp(up, -up, 0.5f);
		Vector3 right = Vector3.Cross(up, forward).normalized*radius;

		float height = (start-end).magnitude;
		float sideLength = Mathf.Max(0, (height*0.5f)-radius);
		Vector3 middle = (end+start)*0.5f;

		start = middle+((start-middle).normalized*sideLength);
		end = middle+((end-middle).normalized*sideLength);

		//Radial circles
		GizmosExtension.GizmosCircle(start, up, color, radius, duration, depthTest);
		GizmosExtension.GizmosCircle(end, -up, color, radius, duration, depthTest);

		//Side lines
		Gizmos.DrawLine(start+right, end+right, color, duration, depthTest);
		Gizmos.DrawLine(start-right, end-right, color, duration, depthTest);

		Gizmos.DrawLine(start+forward, end+forward, color, duration, depthTest);
		Gizmos.DrawLine(start-forward, end-forward, color, duration, depthTest);

		for(int i = 1; i < 26; i++){

			//Start endcap
			Gizmos.DrawLine(Vector3.Slerp(right, -up, i/25.0f)+start, Vector3.Slerp(right, -up, (i-1)/25.0f)+start, color, duration, depthTest);
			Gizmos.DrawLine(Vector3.Slerp(-right, -up, i/25.0f)+start, Vector3.Slerp(-right, -up, (i-1)/25.0f)+start, color, duration, depthTest);
			Gizmos.DrawLine(Vector3.Slerp(forward, -up, i/25.0f)+start, Vector3.Slerp(forward, -up, (i-1)/25.0f)+start, color, duration, depthTest);
			Gizmos.DrawLine(Vector3.Slerp(-forward, -up, i/25.0f)+start, Vector3.Slerp(-forward, -up, (i-1)/25.0f)+start, color, duration, depthTest);

			//End endcap
			Gizmos.DrawLine(Vector3.Slerp(right, up, i/25.0f)+end, Vector3.Slerp(right, up, (i-1)/25.0f)+end, color, duration, depthTest);
			Gizmos.DrawLine(Vector3.Slerp(-right, up, i/25.0f)+end, Vector3.Slerp(-right, up, (i-1)/25.0f)+end, color, duration, depthTest);
			Gizmos.DrawLine(Vector3.Slerp(forward, up, i/25.0f)+end, Vector3.Slerp(forward, up, (i-1)/25.0f)+end, color, duration, depthTest);
			Gizmos.DrawLine(Vector3.Slerp(-forward, up, i/25.0f)+end, Vector3.Slerp(-forward, up, (i-1)/25.0f)+end, color, duration, depthTest);
		}
	}

	/// <summary>
	/// 	- Gizmoss a capsule.
	/// </summary>
	/// <param name='start'>
	/// 	- The position of one end of the capsule.
	/// </param>
	/// <param name='end'>
	/// 	- The position of the other end of the capsule.
	/// </param>
	/// <param name='radius'>
	/// 	- The radius of the capsule.
	/// </param>
	/// <param name='duration'>
	/// 	- How long to draw the capsule.
	/// </param>
	/// <param name='depthTest'>
	/// 	- Whether or not the capsule should be faded when behind other objects.
	/// </param>
	public static void GizmosCapsule(Vector3 start, Vector3 end, float radius = 1, float duration = 0, bool depthTest = true)
	{
		GizmosCapsule(start, end, Color.white, radius, duration, depthTest);
	}

	#endregion

	#region GizmosDrawFunctions

	/// <summary>
	/// 	- Draws a point.
	/// </summary>
	/// <param name='position'>
	/// 	- The point to draw.
	/// </param>
	///  <param name='color'>
	/// 	- The color of the drawn point.
	/// </param>
	/// <param name='scale'>
	/// 	- The size of the drawn point.
	/// </param>
	public static void DrawPoint(Vector3 position, Color color, float scale = 1.0f)
	{
		Color oldColor = Gizmoss.color;

		Gizmoss.color = color;
		Gizmoss.DrawRay(position+(Vector3.up*(scale*0.5f)), -Vector3.up*scale);
		Gizmoss.DrawRay(position+(Vector3.right*(scale*0.5f)), -Vector3.right*scale);
		Gizmoss.DrawRay(position+(Vector3.forward*(scale*0.5f)), -Vector3.forward*scale);

		Gizmoss.color = oldColor;
	}

	/// <summary>
	/// 	- Draws a point.
	/// </summary>
	/// <param name='position'>
	/// 	- The point to draw.
	/// </param>
	/// <param name='scale'>
	/// 	- The size of the drawn point.
	/// </param>
	public static void DrawPoint(Vector3 position, float scale = 1.0f)
	{
		DrawPoint(position, Color.white, scale);
	}

	/// <summary>
	/// 	- Draws an axis-aligned bounding box.
	/// </summary>
	/// <param name='bounds'>
	/// 	- The bounds to draw.
	/// </param>
	/// <param name='color'>
	/// 	- The color of the bounds.
	/// </param>
	public static void DrawBounds(Bounds bounds, Color color)
	{
		Vector3 center = bounds.center;

		float x = bounds.extents.x;
		float y = bounds.extents.y;
		float z = bounds.extents.z;

		Vector3 ruf = center+new Vector3(x,y,z);
		Vector3 rub = center+new Vector3(x,y,-z);
		Vector3 luf = center+new Vector3(-x,y,z);
		Vector3 lub = center+new Vector3(-x,y,-z);

		Vector3 rdf = center+new Vector3(x,-y,z);
		Vector3 rdb = center+new Vector3(x,-y,-z);
		Vector3 lfd = center+new Vector3(-x,-y,z);
		Vector3 lbd = center+new Vector3(-x,-y,-z);

		Color oldColor = Gizmoss.color;
		Gizmoss.color = color;

		Gizmoss.DrawLine(ruf, luf);
		Gizmoss.DrawLine(ruf, rub);
		Gizmoss.DrawLine(luf, lub);
		Gizmoss.DrawLine(rub, lub);

		Gizmoss.DrawLine(ruf, rdf);
		Gizmoss.DrawLine(rub, rdb);
		Gizmoss.DrawLine(luf, lfd);
		Gizmoss.DrawLine(lub, lbd);

		Gizmoss.DrawLine(rdf, lfd);
		Gizmoss.DrawLine(rdf, rdb);
		Gizmoss.DrawLine(lfd, lbd);
		Gizmoss.DrawLine(lbd, rdb);

		Gizmoss.color = oldColor;
	}

	/// <summary>
	/// 	- Draws an axis-aligned bounding box.
	/// </summary>
	/// <param name='bounds'>
	/// 	- The bounds to draw.
	/// </param>
	public static void DrawBounds(Bounds bounds)
	{
		DrawBounds(bounds, Color.white);
	}

	/// <summary>
	/// 	- Draws a local cube.
	/// </summary>
	/// <param name='transform'>
	/// 	- The transform the cube will be local to.
	/// </param>
	/// <param name='size'>
	/// 	- The local size of the cube.
	/// </param>
	/// <param name='center'>
	///		- The local position of the cube.
	/// </param>
	/// <param name='color'>
	/// 	- The color of the cube.
	/// </param>
	public static void DrawLocalCube(Transform transform, Vector3 size, Color color, Vector3 center = default(Vector3))
	{
		Color oldColor = Gizmoss.color;
		Gizmoss.color = color;

		Vector3 lbb = transform.TransformPoint(center+((-size)*0.5f));
		Vector3 rbb = transform.TransformPoint(center+(new Vector3(size.x, -size.y, -size.z)*0.5f));

		Vector3 lbf = transform.TransformPoint(center+(new Vector3(size.x, -size.y, size.z)*0.5f));
		Vector3 rbf = transform.TransformPoint(center+(new Vector3(-size.x, -size.y, size.z)*0.5f));

		Vector3 lub = transform.TransformPoint(center+(new Vector3(-size.x, size.y, -size.z)*0.5f));
		Vector3 rub = transform.TransformPoint(center+(new Vector3(size.x, size.y, -size.z)*0.5f));

		Vector3 luf = transform.TransformPoint(center+((size)*0.5f));
		Vector3 ruf = transform.TransformPoint(center+(new Vector3(-size.x, size.y, size.z)*0.5f));

		Gizmoss.DrawLine(lbb, rbb);
		Gizmoss.DrawLine(rbb, lbf);
		Gizmoss.DrawLine(lbf, rbf);
		Gizmoss.DrawLine(rbf, lbb);

		Gizmoss.DrawLine(lub, rub);
		Gizmoss.DrawLine(rub, luf);
		Gizmoss.DrawLine(luf, ruf);
		Gizmoss.DrawLine(ruf, lub);

		Gizmoss.DrawLine(lbb, lub);
		Gizmoss.DrawLine(rbb, rub);
		Gizmoss.DrawLine(lbf, luf);
		Gizmoss.DrawLine(rbf, ruf);

		Gizmoss.color = oldColor;
	}

	/// <summary>
	/// 	- Draws a local cube.
	/// </summary>
	/// <param name='transform'>
	/// 	- The transform the cube will be local to.
	/// </param>
	/// <param name='size'>
	/// 	- The local size of the cube.
	/// </param>
	/// <param name='center'>
	///		- The local position of the cube.
	/// </param>
	public static void DrawLocalCube(Transform transform, Vector3 size, Vector3 center = default(Vector3))
	{
		DrawLocalCube(transform, size, Color.white, center);
	}

	/// <summary>
	/// 	- Draws a local cube.
	/// </summary>
	/// <param name='space'>
	/// 	- The space the cube will be local to.
	/// </param>
	/// <param name='size'>
	/// 	- The local size of the cube.
	/// </param>
	/// <param name='center'>
	/// 	- The local position of the cube.
	/// </param>
	/// <param name='color'>
	/// 	- The color of the cube.
	/// </param>
	public static void DrawLocalCube(Matrix4x4 space, Vector3 size, Color color, Vector3 center = default(Vector3))
	{
		Color oldColor = Gizmoss.color;
		Gizmoss.color = color;

		Vector3 lbb = space.MultiplyPoint3x4(center+((-size)*0.5f));
		Vector3 rbb = space.MultiplyPoint3x4(center+(new Vector3(size.x, -size.y, -size.z)*0.5f));

		Vector3 lbf = space.MultiplyPoint3x4(center+(new Vector3(size.x, -size.y, size.z)*0.5f));
		Vector3 rbf = space.MultiplyPoint3x4(center+(new Vector3(-size.x, -size.y, size.z)*0.5f));

		Vector3 lub = space.MultiplyPoint3x4(center+(new Vector3(-size.x, size.y, -size.z)*0.5f));
		Vector3 rub = space.MultiplyPoint3x4(center+(new Vector3(size.x, size.y, -size.z)*0.5f));

		Vector3 luf = space.MultiplyPoint3x4(center+((size)*0.5f));
		Vector3 ruf = space.MultiplyPoint3x4(center+(new Vector3(-size.x, size.y, size.z)*0.5f));

		Gizmoss.DrawLine(lbb, rbb);
		Gizmoss.DrawLine(rbb, lbf);
		Gizmoss.DrawLine(lbf, rbf);
		Gizmoss.DrawLine(rbf, lbb);

		Gizmoss.DrawLine(lub, rub);
		Gizmoss.DrawLine(rub, luf);
		Gizmoss.DrawLine(luf, ruf);
		Gizmoss.DrawLine(ruf, lub);

		Gizmoss.DrawLine(lbb, lub);
		Gizmoss.DrawLine(rbb, rub);
		Gizmoss.DrawLine(lbf, luf);
		Gizmoss.DrawLine(rbf, ruf);

		Gizmoss.color = oldColor;
	}

	/// <summary>
	/// 	- Draws a local cube.
	/// </summary>
	/// <param name='space'>
	/// 	- The space the cube will be local to.
	/// </param>
	/// <param name='size'>
	/// 	- The local size of the cube.
	/// </param>
	/// <param name='center'>
	/// 	- The local position of the cube.
	/// </param>
	public static void DrawLocalCube(Matrix4x4 space, Vector3 size, Vector3 center = default(Vector3))
	{
		DrawLocalCube(space, size, Color.white, center);
	}

	/// <summary>
	/// 	- Draws a circle.
	/// </summary>
	/// <param name='position'>
	/// 	- Where the center of the circle will be positioned.
	/// </param>
	/// <param name='up'>
	/// 	- The direction perpendicular to the surface of the circle.
	/// </param>
	/// <param name='color'>
	/// 	- The color of the circle.
	/// </param>
	/// <param name='radius'>
	/// 	- The radius of the circle.
	/// </param>
	public static void DrawCircle(Vector3 position, Vector3 up, Color color, float radius = 1.0f)
	{
		up = ((up == Vector3.zero) ? Vector3.up : up).normalized * radius;
		Vector3 _forward = Vector3.Slerp(up, -up, 0.5f);
		Vector3 _right = Vector3.Cross(up, _forward).normalized*radius;

		Matrix4x4 matrix = new Matrix4x4();

		matrix[0] = _right.x;
		matrix[1] = _right.y;
		matrix[2] = _right.z;

		matrix[4] = up.x;
		matrix[5] = up.y;
		matrix[6] = up.z;

		matrix[8] = _forward.x;
		matrix[9] = _forward.y;
		matrix[10] = _forward.z;

		Vector3 _lastPoint = position + matrix.MultiplyPoint3x4(new Vector3(Mathf.Cos(0), 0, Mathf.Sin(0)));
		Vector3 _nextPoint = Vector3.zero;

		Color oldColor = Gizmoss.color;
		Gizmoss.color = (color == default(Color)) ? Color.white : color;

		for(var i = 0; i < 91; i++){
			_nextPoint.x = Mathf.Cos((i*4)*Mathf.Deg2Rad);
			_nextPoint.z = Mathf.Sin((i*4)*Mathf.Deg2Rad);
			_nextPoint.y = 0;

			_nextPoint = position + matrix.MultiplyPoint3x4(_nextPoint);

			Gizmoss.DrawLine(_lastPoint, _nextPoint);
			_lastPoint = _nextPoint;
		}

		Gizmoss.color = oldColor;
	}

	/// <summary>
	/// 	- Draws a circle.
	/// </summary>
	/// <param name='position'>
	/// 	- Where the center of the circle will be positioned.
	/// </param>
	/// <param name='color'>
	/// 	- The color of the circle.
	/// </param>
	/// <param name='radius'>
	/// 	- The radius of the circle.
	/// </param>
	public static void DrawCircle(Vector3 position, Color color, float radius = 1.0f)
	{
		DrawCircle(position, Vector3.up, color, radius);
	}

	/// <summary>
	/// 	- Draws a circle.
	/// </summary>
	/// <param name='position'>
	/// 	- Where the center of the circle will be positioned.
	/// </param>
	/// <param name='up'>
	/// 	- The direction perpendicular to the surface of the circle.
	/// </param>
	/// <param name='radius'>
	/// 	- The radius of the circle.
	/// </param>
	public static void DrawCircle(Vector3 position, Vector3 up, float radius = 1.0f)
	{
		DrawCircle(position, position, Color.white, radius);
	}

	/// <summary>
	/// 	- Draws a circle.
	/// </summary>
	/// <param name='position'>
	/// 	- Where the center of the circle will be positioned.
	/// </param>
	/// <param name='radius'>
	/// 	- The radius of the circle.
	/// </param>
	public static void DrawCircle(Vector3 position, float radius = 1.0f)
	{
		DrawCircle(position, Vector3.up, Color.white, radius);
	}

	//Wiresphere already exists

	/// <summary>
	/// 	- Draws a cylinder.
	/// </summary>
	/// <param name='start'>
	/// 	- The position of one end of the cylinder.
	/// </param>
	/// <param name='end'>
	/// 	- The position of the other end of the cylinder.
	/// </param>
	/// <param name='color'>
	/// 	- The color of the cylinder.
	/// </param>
	/// <param name='radius'>
	/// 	- The radius of the cylinder.
	/// </param>
	public static void DrawCylinder(Vector3 start, Vector3 end, Color color, float radius = 1.0f){
		Vector3 up = (end-start).normalized*radius;
		Vector3 forward = Vector3.Slerp(up, -up, 0.5f);
		Vector3 right = Vector3.Cross(up, forward).normalized*radius;

		//Radial circles
		GizmosExtension.DrawCircle(start, up, color, radius);
		GizmosExtension.DrawCircle(end, -up, color, radius);
		GizmosExtension.DrawCircle((start+end)*0.5f, up, color, radius);

		Color oldColor = Gizmoss.color;
		Gizmoss.color = color;

		//Side lines
		Gizmoss.DrawLine(start+right, end+right);
		Gizmoss.DrawLine(start-right, end-right);

		Gizmoss.DrawLine(start+forward, end+forward);
		Gizmoss.DrawLine(start-forward, end-forward);

		//Start endcap
		Gizmoss.DrawLine(start-right, start+right);
		Gizmoss.DrawLine(start-forward, start+forward);

		//End endcap
		Gizmoss.DrawLine(end-right, end+right);
		Gizmoss.DrawLine(end-forward, end+forward);

		Gizmoss.color = oldColor;
	}

	/// <summary>
	/// 	- Draws a cylinder.
	/// </summary>
	/// <param name='start'>
	/// 	- The position of one end of the cylinder.
	/// </param>
	/// <param name='end'>
	/// 	- The position of the other end of the cylinder.
	/// </param>
	/// <param name='radius'>
	/// 	- The radius of the cylinder.
	/// </param>
	public static void DrawCylinder(Vector3 start, Vector3 end, float radius = 1.0f)
	{
		DrawCylinder(start, end, Color.white, radius);
	}

	/// <summary>
	/// 	- Draws a cone.
	/// </summary>
	/// <param name='position'>
	/// 	- The position for the tip of the cone.
	/// </param>
	/// <param name='direction'>
	/// 	- The direction for the cone to get wider in.
	/// </param>
	/// <param name='color'>
	/// 	- The color of the cone.
	/// </param>
	/// <param name='angle'>
	/// 	- The angle of the cone.
	/// </param>
	public static void DrawCone(Vector3 position, Vector3 direction, Color color, float angle = 45)
	{
		float length = direction.magnitude;

		Vector3 _forward = direction;
		Vector3 _up = Vector3.Slerp(_forward, -_forward, 0.5f);
		Vector3 _right = Vector3.Cross(_forward, _up).normalized*length;

		direction = direction.normalized;

		Vector3 slerpedVector = Vector3.Slerp(_forward, _up, angle/90.0f);

		float dist;
		var farPlane = new Plane(-direction, position+_forward);
		var distRay = new Ray(position, slerpedVector);

		farPlane.Raycast(distRay, out dist);

		Color oldColor = Gizmoss.color;
		Gizmoss.color = color;

		Gizmoss.DrawRay(position, slerpedVector.normalized*dist);
		Gizmoss.DrawRay(position, Vector3.Slerp(_forward, -_up, angle/90.0f).normalized*dist);
		Gizmoss.DrawRay(position, Vector3.Slerp(_forward, _right, angle/90.0f).normalized*dist);
		Gizmoss.DrawRay(position, Vector3.Slerp(_forward, -_right, angle/90.0f).normalized*dist);

		GizmosExtension.DrawCircle(position+_forward, direction, color, (_forward-(slerpedVector.normalized*dist)).magnitude);
		GizmosExtension.DrawCircle(position+(_forward*0.5f), direction, color, ((_forward*0.5f)-(slerpedVector.normalized*(dist*0.5f))).magnitude);

		Gizmoss.color = oldColor;
	}

	/// <summary>
	/// 	- Draws a cone.
	/// </summary>
	/// <param name='position'>
	/// 	- The position for the tip of the cone.
	/// </param>
	/// <param name='direction'>
	/// 	- The direction for the cone to get wider in.
	/// </param>
	/// <param name='angle'>
	/// 	- The angle of the cone.
	/// </param>
	public static void DrawCone(Vector3 position, Vector3 direction, float angle = 45)
	{
		DrawCone(position, direction, Color.white, angle);
	}

	/// <summary>
	/// 	- Draws a cone.
	/// </summary>
	/// <param name='position'>
	/// 	- The position for the tip of the cone.
	/// </param>
	/// <param name='color'>
	/// 	- The color of the cone.
	/// </param>
	/// <param name='angle'>
	/// 	- The angle of the cone.
	/// </param>
	public static void DrawCone(Vector3 position, Color color, float angle = 45)
	{
		DrawCone(position, Vector3.up, color, angle);
	}

	/// <summary>
	/// 	- Draws a cone.
	/// </summary>
	/// <param name='position'>
	/// 	- The position for the tip of the cone.
	/// </param>
	/// <param name='angle'>
	/// 	- The angle of the cone.
	/// </param>
	public static void DrawCone(Vector3 position, float angle = 45)
	{
		DrawCone(position, Vector3.up, Color.white, angle);
	}

	/// <summary>
	/// 	- Draws an arrow.
	/// </summary>
	/// <param name='position'>
	/// 	- The start position of the arrow.
	/// </param>
	/// <param name='direction'>
	/// 	- The direction the arrow will point in.
	/// </param>
	/// <param name='color'>
	/// 	- The color of the arrow.
	/// </param>
	public static void DrawArrow(Vector3 position, Vector3 direction, Color color)
	{
		Color oldColor = Gizmoss.color;
		Gizmoss.color = color;

		Gizmoss.DrawRay(position, direction);
		GizmosExtension.DrawCone(position+direction, -direction*0.333f, color, 15);

		Gizmoss.color = oldColor;
	}

	/// <summary>
	/// 	- Draws an arrow.
	/// </summary>
	/// <param name='position'>
	/// 	- The start position of the arrow.
	/// </param>
	/// <param name='direction'>
	/// 	- The direction the arrow will point in.
	/// </param>
	public static void DrawArrow(Vector3 position, Vector3 direction)
	{
		DrawArrow(position, direction, Color.white);
	}

	/// <summary>
	/// 	- Draws a capsule.
	/// </summary>
	/// <param name='start'>
	/// 	- The position of one end of the capsule.
	/// </param>
	/// <param name='end'>
	/// 	- The position of the other end of the capsule.
	/// </param>
	/// <param name='color'>
	/// 	- The color of the capsule.
	/// </param>
	/// <param name='radius'>
	/// 	- The radius of the capsule.
	/// </param>
	public static void DrawCapsule(Vector3 start, Vector3 end, Color color, float radius = 1)
	{
		Vector3 up = (end-start).normalized*radius;
		Vector3 forward = Vector3.Slerp(up, -up, 0.5f);
		Vector3 right = Vector3.Cross(up, forward).normalized*radius;

		Color oldColor = Gizmoss.color;
		Gizmoss.color = color;

		float height = (start-end).magnitude;
		float sideLength = Mathf.Max(0, (height*0.5f)-radius);
		Vector3 middle = (end+start)*0.5f;

		start = middle+((start-middle).normalized*sideLength);
		end = middle+((end-middle).normalized*sideLength);

		//Radial circles
		GizmosExtension.DrawCircle(start, up, color, radius);
		GizmosExtension.DrawCircle(end, -up, color, radius);

		//Side lines
		Gizmoss.DrawLine(start+right, end+right);
		Gizmoss.DrawLine(start-right, end-right);

		Gizmoss.DrawLine(start+forward, end+forward);
		Gizmoss.DrawLine(start-forward, end-forward);

		for(int i = 1; i < 26; i++){

			//Start endcap
			Gizmoss.DrawLine(Vector3.Slerp(right, -up, i/25.0f)+start, Vector3.Slerp(right, -up, (i-1)/25.0f)+start);
			Gizmoss.DrawLine(Vector3.Slerp(-right, -up, i/25.0f)+start, Vector3.Slerp(-right, -up, (i-1)/25.0f)+start);
			Gizmoss.DrawLine(Vector3.Slerp(forward, -up, i/25.0f)+start, Vector3.Slerp(forward, -up, (i-1)/25.0f)+start);
			Gizmoss.DrawLine(Vector3.Slerp(-forward, -up, i/25.0f)+start, Vector3.Slerp(-forward, -up, (i-1)/25.0f)+start);

			//End endcap
			Gizmoss.DrawLine(Vector3.Slerp(right, up, i/25.0f)+end, Vector3.Slerp(right, up, (i-1)/25.0f)+end);
			Gizmoss.DrawLine(Vector3.Slerp(-right, up, i/25.0f)+end, Vector3.Slerp(-right, up, (i-1)/25.0f)+end);
			Gizmoss.DrawLine(Vector3.Slerp(forward, up, i/25.0f)+end, Vector3.Slerp(forward, up, (i-1)/25.0f)+end);
			Gizmoss.DrawLine(Vector3.Slerp(-forward, up, i/25.0f)+end, Vector3.Slerp(-forward, up, (i-1)/25.0f)+end);
		}

		Gizmoss.color = oldColor;
	}

	/// <summary>
	/// 	- Draws a capsule.
	/// </summary>
	/// <param name='start'>
	/// 	- The position of one end of the capsule.
	/// </param>
	/// <param name='end'>
	/// 	- The position of the other end of the capsule.
	/// </param>
	/// <param name='radius'>
	/// 	- The radius of the capsule.
	/// </param>
	public static void DrawCapsule(Vector3 start, Vector3 end, float radius = 1)
	{
		DrawCapsule(start, end, Color.white, radius);
	}

	#endregion

	#region GizmosFunctions

	/// <summary>
	/// 	- Gets the methods of an object.
	/// </summary>
	/// <returns>
	/// 	- A list of methods accessible from this object.
	/// </returns>
	/// <param name='obj'>
	/// 	- The object to get the methods of.
	/// </param>
	/// <param name='includeInfo'>
	/// 	- Whether or not to include each method's method info in the list.
	/// </param>
	public static string MethodsOfObject(System.Object obj, bool includeInfo = false){
		string methods = "";
		MethodInfo[] methodInfos = obj.GetType().GetMethods();
		for(int i = 0; i < methodInfos.Length; i++){
			if(includeInfo){
				methods += methodInfos[i]+"\n";
			}

			else{
				methods += methodInfos[i].Name+"\n";
			}
		}

		return (methods);
	}

	/// <summary>
	/// 	- Gets the methods of a type.
	/// </summary>
	/// <returns>
	/// 	- A list of methods accessible from this type.
	/// </returns>
	/// <param name='type'>
	/// 	- The type to get the methods of.
	/// </param>
	/// <param name='includeInfo'>
	/// 	- Whether or not to include each method's method info in the list.
	/// </param>
	public static string MethodsOfType(System.Type type, bool includeInfo = false){
		string methods = "";
		MethodInfo[] methodInfos = type.GetMethods();
		for(var i = 0; i < methodInfos.Length; i++){
			if(includeInfo){
				methods += methodInfos[i]+"\n";
			}

			else{
				methods += methodInfos[i].Name+"\n";
			}
		}

		return (methods);
	}

	#endregion
    */
}
