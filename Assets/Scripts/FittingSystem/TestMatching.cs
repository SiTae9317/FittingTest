//#define HTKWAK
//#define STEP

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class TestMatching : MonoBehaviour 
{
	public GameObject sourceObj;
	public GameObject targetObj;

	List<CustomSkinnedMesh> datas;

	public Vector3 ePos;
	public Vector3 eNor;

	public int index = 0;
	public int vertIndex = 0;

	int targetCount = 0;

	const float epsilon = 0.00001f;
	const float negEp = -0.00001f;
	const float oneEp = 1.0f + 0.00001f;

	private bool isDebuging = false;
	private bool isRunning = false;

	long dt = 0;

	// Use this for initialization
	void Start () 
	{	
//		Debug.Log ("matching");
//		datas = new List<CustomSkinnedMesh> ();
//		datas.Add (goToCSM (sourceObj));
//		datas.Add (goToCSM (targetObj));
//
//		targetCount = datas [1].triangles.Count;
	}
	
	// Update is called once per frame
	void Update () 
	{
		#if(STEP)
		if (isDebuging) 
		{
			Vector3 origin = datas [0].vertices [vertIndex];
			Vector3 direction = datas [0].normals [vertIndex];
			direction.Normalize ();

			Vector3 p0 = datas [1].vertices[datas [1].triangles [(index * 3) + 0]];
			Vector3 p1 = datas [1].vertices[datas [1].triangles [(index * 3) + 1]];
			Vector3 p2 = datas [1].vertices[datas [1].triangles [(index * 3) + 2]];

			float dot0 = Vector3.Dot ((p0 - origin).normalized, direction);
			float dot1 = Vector3.Dot ((p1 - origin).normalized, direction);
			float dot2 = Vector3.Dot ((p2 - origin).normalized, direction);

			if (Input.GetKeyDown (KeyCode.RightArrow)) 
			{
				Debug.Log ("next tri");
				index++;
				index %= (targetCount / 3);
			}

			if (Input.GetKeyDown (KeyCode.UpArrow)) 
			{
				Debug.Log ("next vert");
				vertIndex++;
				index = 0;
			}

			if (Input.GetKeyDown (KeyCode.DownArrow)) 
			{
				while (dot0 < 0.4f || dot1 < 0.4f || dot2 < 0.4f) 
				{
					index++;

					if ((index) > targetCount / 3 - 1) 
					{
						Debug.Log ("not Found");
						index = 0;
						vertIndex++;
						break;
					}

					p0 = datas [1].vertices[datas [1].triangles [(index * 3) + 0]];
					p1 = datas [1].vertices[datas [1].triangles [(index * 3) + 1]];
					p2 = datas [1].vertices[datas [1].triangles [(index * 3) + 2]];

					dot0 = Vector3.Dot ((p0 - origin).normalized, direction);
					dot1 = Vector3.Dot ((p1 - origin).normalized, direction);
					dot2 = Vector3.Dot ((p2 - origin).normalized, direction);
//					index %= targetCount;
				}
			}

			if (Input.GetKeyDown (KeyCode.LeftArrow)) 
			{
				bool isSearch = false;
				while ((index) < targetCount / 3 - 1) 
				{
					index++;

					p0 = datas [1].vertices[datas [1].triangles [(index * 3) + 0]];
					p1 = datas [1].vertices[datas [1].triangles [(index * 3) + 1]];
					p2 = datas [1].vertices[datas [1].triangles [(index * 3) + 2]];

					dot0 = Vector3.Dot ((p0 - origin).normalized, direction);
					dot1 = Vector3.Dot ((p1 - origin).normalized, direction);
					dot2 = Vector3.Dot ((p2 - origin).normalized, direction);

					Vector3 hitPosition = Vector3.zero;
					if (triangleRayCast (p0, p1, p2, origin, direction, ref hitPosition)) 
					{
						isSearch = true;
						break;
					}
				}
				if(!isSearch)
				{
					Debug.Log ("not Found");
				}
			}

//
//			if (dot0 < epsilon || dot1 < epsilon || dot2 < epsilon) 
//			{
//				Debug.Log ("?");
//			}

			Debug.DrawLine (p0, p1, Color.green);
			Debug.DrawLine (p0, p2, Color.green);
			Debug.DrawLine (p2, p1, Color.green);

			Debug.DrawRay (origin, direction, Color.yellow);

			if (dot0 < 0.4f || dot1 < 0.4f || dot2 < 0.4f) 
			{
				//				Debug.Log (index);
				index++;
				index %= (targetCount / 3);
				return;
			}
			else 
			{
				#if(HTKWAK)
				Debug.Log (dot0);
				Debug.Log (dot1);
				Debug.Log (dot2);
				#endif
				Vector3 hitPosition = Vector3.zero;

				if (triangleRayCast (p0, p1, p2, origin, direction, ref hitPosition)) 
				{
					;
				}
			}

//			Debug.Log (Vector3.Dot (origin - p0, direction));
//			Debug.Log (Vector3.Dot (origin - p1, direction));
//			Debug.Log (Vector3.Dot (origin - p2, direction));
//			Debug.Log (Vector3.Dot ((p0 - origin).normalized, direction));
//			Debug.Log (Vector3.Dot ((p1 - origin).normalized, direction));
//			Debug.Log (Vector3.Dot ((p2 - origin).normalized, direction));

//			Debug.Log (dot0);
//			Debug.Log (dot1);
//			Debug.Log (dot2);
//			Debug.Log (index);
//			if (dot0 < 0.3f || dot1 < 0.3f || dot2 < 0.3f) 
//			{
////				Debug.Log (index);
//				index += 3;
//				index %= targetCount;
//				return;
//			}
		}
		#else
//		if (isDebuging) 
//		{
//			Vector3 origin = datas [0].vertices [vertIndex];
//			Vector3 direction = datas [0].normals [vertIndex];
//			direction.Normalize ();
//
//			Debug.DrawRay (origin, direction, Color.green);
//		}
//
//		if (Input.GetKeyDown (KeyCode.UpArrow)) 
//		{
//			Debug.Log ("up");
//			vertIndex++;
//		}
////
		if (Input.GetKeyDown (KeyCode.Alpha1)) 
		{
			startMatching ();
		}

//		Debug.DrawRay (ePos, eNor, Color.green);
		#endif
	}

	void applyMesh(GameObject go, CustomSkinnedMesh csm)
	{
		Matrix4x4 ltw = go.transform.localToWorldMatrix;

		Mesh newMesh = new Mesh ();

		for (int i = 0; i < csm.vertices.Count; i++) 
		{
			csm.vertices [i] = ltw.inverse.MultiplyPoint3x4 (csm.vertices [i]);
			csm.normals [i] = ltw.inverse.MultiplyVector (csm.normals [i]);
		}

		newMesh.vertices = csm.vertices.ToArray ();
		newMesh.normals = csm.normals.ToArray ();
		newMesh.triangles = csm.triangles.ToArray ();
		newMesh.uv = csm.uv.ToArray ();

		sourceObj.GetComponent<MeshFilter> ().mesh = newMesh;
	}

	CustomSkinnedMesh goToCSM(GameObject go)
	{
		Matrix4x4 ltw = go.transform.localToWorldMatrix;
		Mesh mesh = go.GetComponent<MeshFilter> ().mesh;

		CustomSkinnedMesh csm = new CustomSkinnedMesh ();
		csm.vertices.AddRange (mesh.vertices);
		csm.normals.AddRange (mesh.normals);

		for (int i = 0; i < csm.vertices.Count; i++) 
		{
			csm.vertices [i] = ltw.MultiplyPoint3x4 (csm.vertices [i]);
			csm.normals [i] = ltw.MultiplyVector (csm.normals [i]);
		}
		csm.triangles.AddRange (mesh.triangles);
		csm.uv.AddRange (mesh.uv);

		return csm;
	}

	void calcReposition(object arg)
	{
		List<CustomSkinnedMesh> csms = (List<CustomSkinnedMesh>)arg;

		int sourceCount = csms [0].vertices.Count;
		int targetCount = csms [1].triangles.Count;

//		for (int k = 0; k < 5; k++) 
//		{
//			for (int i = 0; i < sourceCount; i++) 
//			{
//				bool noHit = false;
//
//				Vector3 origin = csms [0].vertices [i];
//				Vector3 direction = csms [0].normals [i];
//
//				direction.Normalize ();
//
//				for (int j = 0; j < targetCount; j += 3) 
//				{
//					Vector3 p0 = csms [1].vertices[csms [1].triangles [j + 0]];
//					Vector3 p1 = csms [1].vertices[csms [1].triangles [j + 1]];
//					Vector3 p2 = csms [1].vertices[csms [1].triangles [j + 2]];
//
//					//				Vector3 sp = p0 + p1 + p2;
//					//				sp /= 3.0f;
//					//				sp -= origin;
//					//				sp.Normalize ();
//					//				float dot = dotProduct(sp, direction);
//					//
//					//				if (dot < 0.4f) 
//					//				{
//					//					continue;
//					//				}
//
//					//				float dot0 = Vector3.Dot ((p0 - origin).normalized, direction);
//					//				float dot1 = Vector3.Dot ((p1 - origin).normalized, direction);
//					//				float dot2 = Vector3.Dot ((p2 - origin).normalized, direction);
//					//
//					//				if (dot0 < 0.4f || dot1 < 0.4f || dot2 < 0.4f) 
//					//				{
//					//					continue;
//					//				}
//
//					Vector3 hitPosition = Vector3.zero;
//
//					if (triangleRayCast (p0, p1, p2, origin, direction, ref hitPosition)) 
//					{
//						float dis = Vector3.Distance (origin, hitPosition);
//
//						if (dis > 0.05f) 
//						{
//							csms [0].vertices [i] += direction * (0.005f);
//						}
//						else 
//						{
//							csms [0].vertices [i] += direction * (dis + 0.005f);
//						}
//
//						//					csms [0].vertices [i] = hitPosition;
//
//						noHit = true;
//
//						break;
//					}
//				}
//
//				if (!noHit) 
//				{
//					csms [0].vertices [i] += direction * (0.005f);
//					//				ePos = origin;
//					//				eNor = direction;
//					//				Debug.Log ("no Hit : " + i);
//					//				break;
//				}
//			}
//			isRunning = false;
//		}

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

//				Vector3 p0 = vertices[triangles [j + 0]];
//				Vector3 p1 = vertices[triangles [j + 1]];
//				Vector3 p2 = vertices[triangles [j + 2]];
//				Vector3 p0 = csms [1].vertices[csms [1].triangles [j + 0]];
//				Vector3 p1 = csms [1].vertices[csms [1].triangles [j + 1]];
//				Vector3 p2 = csms [1].vertices[csms [1].triangles [j + 2]];

//				Vector3 sp = p0 + p1 + p2;
//				sp /= 3.0f;
//				sp -= origin;
//				sp.Normalize ();
//				float dot = dotProduct(sp, direction);
//
//				if (dot < 0.4f) 
//				{
//					continue;
//				}

//				float dot0 = Vector3.Dot ((p0 - origin).normalized, direction);
//				float dot1 = Vector3.Dot ((p1 - origin).normalized, direction);
//				float dot2 = Vector3.Dot ((p2 - origin).normalized, direction);
//
//				if (dot0 < 0.4f || dot1 < 0.4f || dot2 < 0.4f) 
//				{
//					continue;
//				}

//				Vector3 hitPosition = Vector3.zero;
//
//				if (triangleRayCast (p0, p1, p2, origin, direction, ref hitPosition)) 
//				{
//					float dis = Vector3.Distance (origin, hitPosition);
//
//					if (dis > 0.05f) 
//					{
//						csms [0].vertices [i] += direction * (0.005f);
//					}
//					else 
//					{
//						csms [0].vertices [i] += direction * (dis + 0.005f);
//					}
//
////					csms [0].vertices [i] = hitPosition;
//
//					noHit = true;
//
//					break;
//				}
			}

			if (!noHit) 
			{
				csms [0].vertices [i] += direction * (0.005f);
//				ePos = origin;
//				eNor = direction;
//				Debug.Log ("no Hit : " + i);
//				break;
			}

//			isSkip.Clear ();
//			isSkip = null;
		}
		isRunning = false;
	}

//	bool checkInner(Vector3 p, Vector3 center, Vector3 direction)
//	{
//		;
//	}

	public void startMatching()
	{
//		Debug.Log ("matching");
		dt = System.DateTime.Now.Ticks;
		datas = new List<CustomSkinnedMesh> ();
		datas.Add (goToCSM (sourceObj));
		datas.Add (goToCSM (targetObj));

		targetCount = datas [1].triangles.Count;

		ParameterizedThreadStart pts = new ParameterizedThreadStart (calcReposition);
		Thread t = new Thread (pts);
		t.Start (datas);
		isRunning = true;
		StartCoroutine (checkThreadEnd ());
//		t.Join ();
//		calcReposition (datas);

//		applyMesh (sourceObj, datas [0]);
	}

	IEnumerator checkThreadEnd()
	{
		while (isRunning) 
		{
			yield return null;
		}

		dt = System.DateTime.Now.Ticks - dt;

		applyMesh (sourceObj, datas [0]);

//		Debug.Log ("matching end " + (double)dt / 10000000.0);

		Destroy (gameObject);
	}

	public void startMatchingTest()
	{
		Debug.Log ("matching");
		datas = new List<CustomSkinnedMesh> ();
		datas.Add (goToCSM (sourceObj));
		datas.Add (goToCSM (targetObj));

		targetCount = datas [1].triangles.Count;
		isDebuging = true;

//		calcReposition (datas);

//		applyMesh (sourceObj, datas [0]);

//		Debug.Log ("end");
	}

//	bool triangleRayCast(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 origin, Vector3 direction, ref Vector3 hitPosition)
//	{
////		float epsilon = Mathf.Epsilon;
////		float epsilon = 0.00001f;
//
//		Vector3 e1 = p1 - p0;
//		Vector3 e2 = p2 - p0;
//
//		Vector3 p = Vector3.Cross (direction, e2);
//
//		float a = Vector3.Dot (e1, p);
//
//		if (a == 0.0f) 
////		if (a > -epsilon && a < epsilon) 
//		{
//			#if (HTKWAK)
//			Debug.Log ((a > -epsilon) + " " + (a < epsilon) + " a = " + a);
//			#else
//			#endif
//			return false;
//		}
//
//		float f = 1.0f / a;
//
//		Vector3 s = origin - p0;
//
//		float u = f * Vector3.Dot (s, p);
//
////		if(u < 0.0f || u > 1.0f)
////		if(u < (-epsilon) || u > (1.0f + epsilon))
//		if(u < negEp || u > oneEp)
//		{
//			#if (HTKWAK)
//			Debug.Log ((u < -epsilon) + " u = " + u);
//			#else
//			#endif
//			return false;
//		}
//
//		Vector3 q = Vector3.Cross (s, e1);
//
//		float v = f * Vector3.Dot (direction, q);
//
////		if(v < 0.0f || v > 1.0f)
////		if(v < 0.0f)
////		if(v < -epsilon || v > (1.0f + epsilon))
//		if(v < negEp || v > oneEp)
//		{
//			#if (HTKWAK)
//			Debug.Log ((v < -epsilon) + " v = " + v);
//			#else
//			#endif
//			return false;
//		}
//
////		if((u + v) > (1.0f + epsilon))// && v > epsilon)
//		if((u + v) > oneEp)// && v > epsilon)
//		{
//			#if (HTKWAK)
//			Debug.Log ((v < 0.0f) + " " + ((u + v) > 1.0f) + " " + (v > epsilon) + " " + (u > 1.0f)  + " v = " + v + " u = " + u + " " + (u + v));
//			#else
//			#endif
//			return false;
//		}
//
//		float t = f * Vector3.Dot (e2, q);
//
////		if(t < 0.0f)
//		if(t < negEp)
////		if(t < Mathf.Epsilon)
//		{
//			#if (HTKWAK)
//			Debug.Log ("t");
//			#else
//			#endif
//			return false;
//		}
//
//		hitPosition = (1.0f - u - v) * p0 + u * p1 + v * p2;
//		#if (HTKWAK)
//		Debug.Log ("hit");
//		#else
//		#endif
//
//		return true;
//	}

	bool triangleRayCast(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 origin, Vector3 direction, ref Vector3 hitPosition)
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

	float dotProduct(Vector3 left, Vector3 right)
	{
		return left.x * right.x + left.y * right.y + left.z * right.z;
	}

	float circleCheck(Vector3 left, Vector3 right, float r)
	{
		return Mathf.Pow (left.x, 2.0f) + Mathf.Pow (left.y, 2.0f) + Mathf.Pow (left.z, 2.0f) +
		Mathf.Pow (right.x, 2.0f) + Mathf.Pow (right.y, 2.0f) + Mathf.Pow (right.z, 2.0f) -
		-2.0f * dotProduct (left, right) - Mathf.Pow (r, 2.0f);
//		return left.x * right.x + left.y * right.y + left.z * right.z;
	}

	Vector3 crossProduct(Vector3 left, Vector3 right)
	{
		return new Vector3 (left.y * right.z - left.z * right.y,
							left.z * right.x - left.x * right.z,
							left.x * right.y - left.y * right.x);
	}
}
