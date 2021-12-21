//#define HTKWAK
//#define STEP

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomRayCast : MonoBehaviour
{
//	public int index = 0;
//	public int vertIndex = 0;

//	int targetCount = 0;

	const float epsilon = 0.00001f;
	const float negEp = -0.00001f;
	const float oneEp = 1.0f + 0.00001f;

	long dt = 0;


	void calcReposition(object arg)
	{
		List<CustomSkinnedMesh> csms = (List<CustomSkinnedMesh>)arg;

		int sourceCount = csms [0].vertices.Count;
		int targetCount = csms [1].triangles.Count;
		List<Vector3> vertices = csms [1].vertices;
		List<Vector3> noramls = csms [1].normals;
		List<int> triangles = csms [1].triangles;

		List<Vector3> triVert = new List<Vector3> ();
		List<Vector3> centerPivot = new List<Vector3> ();
		List<Vector3> faceNormal = new List<Vector3> ();

		for (int i = 0; i < triangles.Count; i += 3) 
		{
			triVert.Add (vertices [triangles [i + 0]]);
			triVert.Add (vertices [triangles [i + 1]]);
			triVert.Add (vertices [triangles [i + 2]]);

			Vector3 cenPivot = triVert [i + 0] + triVert [i + 1] + triVert [i + 2];

			centerPivot.Add (cenPivot / 3.0f);
			centerPivot.Add (cenPivot / 3.0f);
			centerPivot.Add (cenPivot / 3.0f);

			Vector3 sumNormal = noramls [triangles [i + 0]] + noramls [triangles [i + 1]] + noramls [triangles [i + 2]];

			sumNormal.Normalize ();

			faceNormal.Add (sumNormal);
			faceNormal.Add (sumNormal);
			faceNormal.Add (sumNormal);
		}

		for (int i = 0; i < sourceCount; i++) 
		{
			bool noHit = false;

			Vector3 origin = csms [0].vertices [i];
			Vector3 direction = csms [0].normals [i];

			direction.Normalize ();

			//			Vector3 center = origin + direction / 2.0f;
			//			float r = Vector3.Distance (Vector3.zero, direction / 2.0f);

			//			Dictionary<Vector3, bool> isSkip = new Dictionary<Vector3, bool> ();

			for (int j = 0; j < targetCount; j += 3) 
			{
				Vector3 p0 = triVert[j + 0];
				Vector3 p1 = triVert[j + 1];
				Vector3 p2 = triVert[j + 2];
				Vector3 sp = centerPivot [j];

				if (dotProduct (faceNormal [j], direction) < 0.0f) 
				{
					continue;
				}

				if(dotProduct((sp - origin).normalized, direction) > 0.0f)
				{				
					Vector3 hitPosition = Vector3.zero;

					if (triangleRayCast (p0, p1, p2, origin, direction, ref hitPosition)) 
					{
						float dis = Vector3.Distance (origin, hitPosition);

						if (dis > 0.05f) 
						{
							csms [0].vertices [i] += direction * (0.005f);
						}
						else 
						{
							csms [0].vertices [i] += direction * (dis + 0.005f);
						}

						//					csms [0].vertices [i] = hitPosition;

						noHit = true;

						break;
					}
				}
			}

			if (!noHit) 
			{
				csms [0].vertices [i] += direction * (0.005f);
			}
		}
	}

	public static bool triangleRayCast(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 origin, Vector3 direction, ref Vector3 hitPosition)
	{
		Vector3 e1 = p1 - p0;
		Vector3 e2 = p2 - p0;

		Vector3 p = crossProduct (direction, e2);

		float a = dotProduct (e1, p);

		if (a == 0.0f) 
		{
			#if (HTKWAK)
			Debug.Log ((a > -epsilon) + " " + (a < epsilon) + " a = " + a);
			#else
			#endif
			return false;
		}

		float f = 1.0f / a;

		Vector3 s = origin - p0;

		float u = f * dotProduct (s, p);

		if(u < negEp || u > oneEp)
		{
			#if (HTKWAK)
			Debug.Log ((u < -epsilon) + " u = " + u);
			#else
			#endif
			return false;
		}

		Vector3 q = crossProduct (s, e1);

		float v = f * dotProduct (direction, q);

		if(v < negEp || v > oneEp)
		{
			#if (HTKWAK)
			Debug.Log ((v < -epsilon) + " v = " + v);
			#else
			#endif
			return false;
		}

		if((u + v) > oneEp)
		{
			#if (HTKWAK)
			Debug.Log ((v < 0.0f) + " " + ((u + v) > 1.0f) + " " + (v > epsilon) + " " + (u > 1.0f)  + " v = " + v + " u = " + u + " " + (u + v));
			#else
			#endif
			return false;
		}

		float t = f * dotProduct (e2, q);

		if(t < negEp)
		{
			#if (HTKWAK)
			Debug.Log ("t");
			#else
			#endif
			return false;
		}

		hitPosition = (1.0f - u - v) * p0 + u * p1 + v * p2;
		#if (HTKWAK)
		Debug.Log ("hit");
		#else
		#endif

		return true;
	}

	public static float dotProduct(Vector3 left, Vector3 right)
	{
		return left.x * right.x + left.y * right.y + left.z * right.z;
	}

	public static float circleCheck(Vector3 left, Vector3 right, float r)
	{
		return Mathf.Pow (left.x, 2.0f) + Mathf.Pow (left.y, 2.0f) + Mathf.Pow (left.z, 2.0f) +
			Mathf.Pow (right.x, 2.0f) + Mathf.Pow (right.y, 2.0f) + Mathf.Pow (right.z, 2.0f) -
			-2.0f * dotProduct (left, right) - Mathf.Pow (r, 2.0f);
		//		return left.x * right.x + left.y * right.y + left.z * right.z;
	}

	public static Vector3 crossProduct(Vector3 left, Vector3 right)
	{
		return new Vector3 (left.y * right.z - left.z * right.y,
			left.z * right.x - left.x * right.z,
			left.x * right.y - left.y * right.x);
	}
}
