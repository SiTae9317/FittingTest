using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;
using System.IO;

//public class PartitionInformation
//{
//	public Dictionary<string, Vector3> jointPosition = new Dictionary<string, Vector3>();
//	public List<CustomSkinnedMesh> csms = new List<CustomSkinnedMesh>();
//	public Dictionary<int, List<int>> vertexToTriangle = new Dictionary<int, List<int>>();
//}
//
//public class CustomSkinnedMesh
//{
//	public string name = "";
//	public List<Vector3> vertices = new List<Vector3> ();
//	public List<Vector3> normals = new List<Vector3> ();
//	public List<Vector2> uv = new List<Vector2> ();
//	public List<int> triangles = new List<int> ();
//	public List<BoneWeight> boneWeights = new List<BoneWeight> ();
//	public List<Matrix4x4> boneMatrices = new List<Matrix4x4>();
//}
//
////public class customSkinnedMesh2
////{
////	public Dictionary<string, Vector3> jointPosition;
////
////	public List<Vector3> vertices = new List<Vector3> ();
////	public List<Vector3> worldVertices = new List<Vector3> ();
////	public List<Vector3> normals = new List<Vector3> ();
////	public List<Vector2> uv = new List<Vector2> ();
////	public List<int> triangles = new List<int> ();
////	public List<BoneWeight> boneWeights = new List<BoneWeight> ();
////
////	public Dictionary<int, List<int>> vertexToTriangle = new Dictionary<int, List<int>>();
////}

public class MeshPartition : MonoBehaviour 
{
	public GameObject oriObj;
	public GameObject targetObj;
	public Material defaultMat;
	public bool isEnd = false;
	private List<CustomSkinnedMesh> newCSMS = new List<CustomSkinnedMesh>();
	private int partitionMeshCount = -1;
	private List<Matrix4x4> oriBoneMatrices;

	// Use this for initialization
	void Start () 
	{		

//		startMP ();
	}

	public void startMP()
	{
		PartitionInformation pi = initialize (oriObj);

		ParameterizedThreadStart pts = new ParameterizedThreadStart (calcPartitionInfo);
		Thread t = new Thread (pts);
		t.Start (pi);
	}

	// Update is called once per frame
	void Update () 
	{
		if (newCSMS.Count == partitionMeshCount) 
		{
			List<Matrix4x4> bindPose = new List<Matrix4x4> ();
			List<Transform> bones = new List<Transform> ();
			Transform rootBone = null;

			GameObject sourceObj = oriObj;

			if (targetObj != null) 
			{
				sourceObj = targetObj;
			}

			SkinnedMeshRenderer sourceSmr = sourceObj.GetComponentInChildren<SkinnedMeshRenderer>();
			bindPose.AddRange (sourceSmr.sharedMesh.bindposes);
			bones.AddRange (sourceSmr.bones);
			rootBone = sourceSmr.rootBone;

			foreach (SkinnedMeshRenderer oriSmr in oriObj.GetComponentsInChildren<SkinnedMeshRenderer>()) 
			{
				Destroy (oriSmr.gameObject);
			}

			GameObject parentObj = new GameObject ();
			parentObj.name = "Mesh";
			parentObj.transform.parent = oriObj.transform;

			for (int i = 0; i < newCSMS.Count; i++) 
			{
				GameObject newObj = new GameObject ();

				newObj.transform.parent = parentObj.transform;

				newObj.name = newCSMS [i].name;

//				if (newObj.name.Equals ("JtSpineA")) 
//				{
//					MeshFilter mf = newObj.AddComponent<MeshFilter> ();
//					MeshRenderer mr = newObj.AddComponent<MeshRenderer> ();
//
//					mr.material = defaultMat;
//
//					Mesh mesh = new Mesh ();
//
//					mesh.vertices = newCSMS [i].vertices.ToArray ();
//					mesh.normals = newCSMS [i].normals.ToArray ();
//					mesh.uv = newCSMS [i].uv.ToArray ();
//					mesh.triangles = newCSMS [i].triangles.ToArray ();
//
//					mf.mesh = mesh;
//				}
//				else 
//				{
//					Destroy (newObj);
//				}

//				MeshFilter mf = newObj.AddComponent<MeshFilter> ();
//				MeshRenderer mr = newObj.AddComponent<MeshRenderer> ();
//
//				mr.material = defaultMat;
//
//				Mesh mesh = new Mesh ();
//
//				mesh.vertices = newCSMS [i].vertices.ToArray ();
//				mesh.normals = newCSMS [i].normals.ToArray ();
//				mesh.uv = newCSMS [i].uv.ToArray ();
//				mesh.triangles = newCSMS [i].triangles.ToArray ();
//
//				mf.mesh = mesh;

				SkinnedMeshRenderer smr = newObj.AddComponent<SkinnedMeshRenderer> ();

				smr.material = defaultMat;

				Mesh mesh = new Mesh ();

				mesh.vertices = newCSMS [i].vertices.ToArray ();
				mesh.normals = newCSMS [i].normals.ToArray ();
				mesh.uv = newCSMS [i].uv.ToArray ();
				mesh.boneWeights = newCSMS [i].boneWeights.ToArray ();
				mesh.triangles = newCSMS [i].triangles.ToArray ();
				mesh.bindposes = bindPose.ToArray ();
//				mesh.bindposes = newCSMS [i].boneMatrices.ToArray ();

				smr.bones = bones.ToArray ();
				smr.rootBone = rootBone;
				smr.sharedMesh = mesh;
			}
			
			newCSMS.Clear ();
			isEnd = true;

			parentObj.transform.localScale = Vector3.one;
		}
	}

	public void construct(GameObject setObj)
	{		
		oriObj = setObj;

		defaultMat = oriObj.GetComponentInChildren<SkinnedMeshRenderer> ().material;

		oriBoneMatrices = new List<Matrix4x4> ();

		SkinnedMeshRenderer smr = oriObj.GetComponentInChildren<SkinnedMeshRenderer> ();

		for (int i = 0; i < smr.bones.Length; i++) 
		{
			oriBoneMatrices.Add (smr.bones [i].localToWorldMatrix * smr.sharedMesh.bindposes [i]);
		}
	}

	void calcPartitionInfo(object arg)
	{
		PartitionInformation pi = (PartitionInformation) arg;

		Debug.Log (pi.csms.Count + " " + pi.jointPosition.Count);

		List<CustomSkinnedMesh> csms = pi.csms;

		List<Vector3> vertices = new List<Vector3> ();
		List<Vector3> worldVertices = new List<Vector3> ();
		List<Vector3> normals = new List<Vector3> ();
		List<Vector2> uv = new List<Vector2> ();
		List<int> triangles = new List<int> ();
		List<BoneWeight> boneWeights = new List<BoneWeight> ();

		for (int i = 0; i < csms.Count; i++) 
		{
			CustomSkinnedMesh csm = csms [i];
			
			int beforeVertexCount = vertices.Count;
			int beforeTriangleIndex = triangles.Count;
			int vertexCount = csm.vertices.Count;

			List<int> keepTriangles = new List<int> ();
			keepTriangles.AddRange (csm.triangles);

			for (int j = 0; j < keepTriangles.Count; j++) 
			{
				keepTriangles [j] += beforeVertexCount;
			}

			for (int j = 0; j < keepTriangles.Count; j++) 
			{
				int vertexIndex = keepTriangles [j];

				if (pi.vertexToTriangle.ContainsKey (vertexIndex)) 
				{
					pi.vertexToTriangle [vertexIndex].Add (beforeTriangleIndex + j);
				}
				else 
				{
					List<int> containTriangle = new List<int> ();
					containTriangle.Add(beforeTriangleIndex + j);
					pi.vertexToTriangle .Add(vertexIndex, containTriangle);
				}
			}

			List<BoneWeight> keepBoneWeights = new List<BoneWeight> ();
			keepBoneWeights.AddRange (csm.boneWeights);

			List<Vector3> keepVertices = new List<Vector3> ();
			keepVertices.AddRange (csm.vertices);

			for (int j = 0; j < keepVertices.Count; j++) 
			{
				BoneWeight weight = keepBoneWeights[j];

				Matrix4x4 bm0 = csm.boneMatrices[weight.boneIndex0];
				Matrix4x4 bm1 = csm.boneMatrices[weight.boneIndex1];
				Matrix4x4 bm2 = csm.boneMatrices[weight.boneIndex2];
				Matrix4x4 bm3 = csm.boneMatrices[weight.boneIndex3];

				Matrix4x4 vertexMatrix = new Matrix4x4();

				for (int n = 0; n < 16; n++)
				{
					vertexMatrix[n] =
						bm0[n] * weight.weight0 +
						bm1[n] * weight.weight1 +
						bm2[n] * weight.weight2 +
						bm3[n] * weight.weight3;
				}

				worldVertices.Add(vertexMatrix.MultiplyPoint3x4(keepVertices[j]));

				keepVertices[j] = vertexMatrix.MultiplyPoint3x4(keepVertices[j]);

				bm0 = oriBoneMatrices [weight.boneIndex0];
				bm1 = oriBoneMatrices [weight.boneIndex1];
				bm2 = oriBoneMatrices [weight.boneIndex2];
				bm3 = oriBoneMatrices [weight.boneIndex3];

				for (int n = 0; n < 16; n++)
				{
					vertexMatrix[n] =
						bm0[n] * weight.weight0 +
						bm1[n] * weight.weight1 +
						bm2[n] * weight.weight2 +
						bm3[n] * weight.weight3;
				}

				keepVertices[j] = vertexMatrix.inverse.MultiplyPoint3x4(keepVertices[j]);
			}

			vertices.AddRange (keepVertices);
			normals.AddRange (csm.normals);
			uv.AddRange (csm.uv);
			boneWeights.AddRange (keepBoneWeights);
			triangles.AddRange (keepTriangles);
		}

		Debug.Log ("Vertex = " + vertices.Count);
		Debug.Log ("worldVertices = " + worldVertices.Count);
		Debug.Log ("normals = " + normals.Count);
		Debug.Log ("uv = " + uv.Count);
		Debug.Log ("boneWeights = " + boneWeights.Count);
		Debug.Log ("triangles = " + triangles.Count);
		Debug.Log ("vertexToTriangle = " + pi.vertexToTriangle.Count);

		Dictionary<string, List<int>> jointVertexIndex = calcPartition (worldVertices, pi.jointPosition);

		Dictionary<string, List<int>>.Enumerator jointVertexIndexEnum = jointVertexIndex.GetEnumerator ();

//		List<CustomSkinnedMesh> newCSMS = new List<CustomSkinnedMesh> ();

		int totalVertexCount = 0;
		int totalTriangleCount = 0;

		while (jointVertexIndexEnum.MoveNext ()) 
		{
			Debug.Log (jointVertexIndexEnum.Current.Key + " " + jointVertexIndexEnum.Current.Value.Count);
			CustomSkinnedMesh partCSM = new CustomSkinnedMesh ();
			partCSM.name = jointVertexIndexEnum.Current.Key;
			List<int> vertexIndices = jointVertexIndexEnum.Current.Value;

			int newTriIndex = 0;

			Dictionary<int, int> newVT = new Dictionary<int, int> ();

			for (int i = 0; i < vertexIndices.Count; i++) 
			{
				int vertexIndex = vertexIndices [i];

				List<int> vtIndices = pi.vertexToTriangle [vertexIndex];

				for (int j = 0; j < vtIndices.Count; j++) 
				{
					int triangleIndex = vtIndices [j];
					triangleIndex /= 3;
					triangleIndex *= 3;

					for (int k = 0; k < 3; k++) 
					{
						int vi = triangles [triangleIndex + k];

						if (!newVT.ContainsKey (vi)) 
						{
							newVT.Add (vi, newTriIndex);

							partCSM.vertices.Add(vertices [vi]);
							partCSM.normals.Add(normals [vi]);
							partCSM.uv.Add(uv [vi]);
							partCSM.boneWeights.Add(boneWeights [vi]);
							partCSM.triangles.Add (newTriIndex);
							newTriIndex++;

							totalVertexCount++;
						}
						else 
						{
							partCSM.triangles.Add (newVT[vi]);
						}

						totalTriangleCount++;
					}

//					int vi0 = triangles [triangleIndex + 0];
//					int vi1 = triangles [triangleIndex + 1];
//					int vi2 = triangles [triangleIndex + 2];
//
//					partCSM.vertices.Add(vertices [vi0]);
//					partCSM.vertices.Add(vertices [vi1]);
//					partCSM.vertices.Add(vertices [vi2]);
//
//					partCSM.normals.Add(normals [vi0]);
//					partCSM.normals.Add(normals [vi1]);
//					partCSM.normals.Add(normals [vi2]);
//
//					partCSM.uv.Add(uv [vi0]);
//					partCSM.uv.Add(uv [vi1]);
//					partCSM.uv.Add(uv [vi2]);
//
////					partCSM.boneWeights.Add(uv [vi0]);
////					partCSM.boneWeights.Add(uv [vi1]);
////					partCSM.boneWeights.Add(uv [vi2]);
//
//					partCSM.triangles.Add (newTriIndex + 0);
//					partCSM.triangles.Add (newTriIndex + 1);
//					partCSM.triangles.Add (newTriIndex + 2);
//
//					newTriIndex += 3;
				}
			}

			newVT.Clear ();
			newVT = null;

			newCSMS.Add (partCSM);
		}

		partitionMeshCount = newCSMS.Count;

		Debug.Log (newCSMS.Count);

		Debug.Log (totalVertexCount);
		Debug.Log (totalTriangleCount / 3);
	}

	Dictionary<string, List<int>> calcPartition(List<Vector3> worldVertices, Dictionary<string, Vector3> jointPosition)
	{	
		Dictionary<string, List<int>> jointVertexIndex = new Dictionary<string, List<int>> ();	
		
		for (int i = 0; i < worldVertices.Count; i++) 
		{
			Vector3 targetPostion = worldVertices [i];

			Dictionary<string, Vector3>.Enumerator jointPositionEnum = jointPosition.GetEnumerator ();

			string minimumName = "";
			float minimumDis = float.MaxValue;

			string secondName = "";
			float seCondMinimumDis = float.MaxValue;

			string thirdName = "";
			float thirdMinimumDis = float.MaxValue;

			string forthName = "";
			float forthMinimumDis = float.MaxValue;

			while(jointPositionEnum.MoveNext())
			{
				string name = jointPositionEnum.Current.Key;

				Vector3 position = jointPositionEnum.Current.Value;

				float dis = Vector3.Distance (targetPostion, position);

				dis = Mathf.Abs (dis);

				if (minimumDis > dis) 
				{
					forthName = thirdName;
					forthMinimumDis = thirdMinimumDis;

					thirdName = secondName;
					thirdMinimumDis = seCondMinimumDis;

					secondName = minimumName;
					seCondMinimumDis = minimumDis;
					
					minimumName = name;
					minimumDis = dis;
				}
			}

//			if (minimumName.Contains ("Spine")) 
//			{
//				if (minimumName.Contains ("Spine1")) 
//				{
//					minimumName = "Spine1";
//				}
//				else 
//				{
//					minimumName = "Spine";
//				}
//			}
//
//			if (minimumName.Contains ("UpLeg")) 
//			{
//				minimumName = "UpLeg";
//			}
//
//			if (minimumName.Contains ("Shoulder")) 
//			{
//				minimumName = "Shoulder";
//			}
//
////			if (minimumName.Contains ("LeftFoot")) 
////			{
////				minimumName = "LeftLowerLeg";
////			}
////
////			if (minimumName.Contains ("RightFoot")) 
////			{
////				minimumName = "RightLowerLeg";
////			}
////
////			if (minimumName.Contains ("LeftUpperArm")) 
////			{
////				minimumName = "LeftArm";
////			}
////
////			if (minimumName.Contains ("RightUpperArm")) 
////			{
////				minimumName = "RightArm";
//			}

			if (!jointVertexIndex.ContainsKey (minimumName)) 
			{
				List<int> newIndexs = new List<int> ();

				newIndexs.Add (i);

				jointVertexIndex.Add (minimumName, newIndexs);
			}
			else 
			{
				jointVertexIndex [minimumName].Add (i);
			}
		}

		return jointVertexIndex;
	}

	CustomSkinnedMesh generateCSM(SkinnedMeshRenderer smr)
	{
		Mesh mesh = smr.sharedMesh;

		CustomSkinnedMesh csm = new CustomSkinnedMesh ();

		csm.vertices.AddRange (mesh.vertices);
		csm.normals.AddRange (mesh.normals);
		csm.uv.AddRange (mesh.uv);
		csm.triangles.AddRange (mesh.triangles);
		csm.boneWeights.AddRange (mesh.boneWeights);

		for (int i = 0; i < smr.bones.Length; i++) 
		{
			csm.boneMatrices.Add (smr.bones [i].localToWorldMatrix * mesh.bindposes [i]);
		}

		return csm;
	}

	PartitionInformation initialize(GameObject sourceObj)
	{		
		PartitionInformation pi = new PartitionInformation ();

		foreach (SkinnedMeshRenderer smr in sourceObj.GetComponentsInChildren<SkinnedMeshRenderer> ()) 
		{
			CustomSkinnedMesh csm = generateCSM (smr);
			pi.csms.Add (csm);
		}

		GameObject hips = null;

		for (int i = 0; i < sourceObj.transform.childCount; i++) 
		{
			hips = sourceObj.transform.GetChild (i).gameObject;
			if (hips.name.Equals ("Hips")) 
			{
				break;
			}
		}

		foreach (Transform ts in hips.GetComponentsInChildren<Transform> ()) 
		{
			pi.jointPosition.Add (ts.name, ts.position);
		}

//		pi.jointPosition.Remove ("Hips");
//
//		float leftPoint = pi.jointPosition ["LeftShoulder"].x + pi.jointPosition ["LeftArm"].x;
//		leftPoint /= 2.0f;
//
//		float rightPoint = pi.jointPosition ["RightShoulder"].x + pi.jointPosition ["RightArm"].x;
//		rightPoint /= 2.0f;
//
//		pi.jointPosition.Add ("Spine_L", new Vector3 (leftPoint, pi.jointPosition ["Spine"].y, pi.jointPosition ["Spine"].z));
//		pi.jointPosition.Add ("Spine_R", new Vector3 (rightPoint, pi.jointPosition ["Spine"].y, pi.jointPosition ["Spine"].z));
//
//		pi.jointPosition.Add ("Spine1_L", new Vector3 (leftPoint, pi.jointPosition ["Spine1"].y, pi.jointPosition ["Spine1"].z));
//		pi.jointPosition.Add ("Spine1_R", new Vector3 (rightPoint, pi.jointPosition ["Spine1"].y, pi.jointPosition ["Spine1"].z));
//
//		Vector3 leftUpperArm = pi.jointPosition ["LeftArm"] + pi.jointPosition ["LeftForeArm"];
//		leftUpperArm /= 2.0f;
//
//		Vector3 leftLowerArm = pi.jointPosition ["LeftForeArm"] + pi.jointPosition ["LeftHand"];
//		leftLowerArm /= 2.0f;
//
//		pi.jointPosition.Add ("LeftUpperArm", leftUpperArm);
//		pi.jointPosition.Add ("LeftLowerArm", leftLowerArm);
//
//		Vector3 rightUpperArm = pi.jointPosition ["RightArm"] + pi.jointPosition ["RightForeArm"];
//		rightUpperArm /= 2.0f;
//
//		Vector3 rightLowerArm = pi.jointPosition ["RightForeArm"] + pi.jointPosition ["RightHand"];
//		rightLowerArm /= 2.0f;
//
//		pi.jointPosition.Add ("RightUpperArm", rightUpperArm);
//		pi.jointPosition.Add ("RightLowerArm", rightLowerArm);
//
//		Vector3 oriLeftArm = pi.jointPosition ["LeftArm"];
//		Vector3 oriRightArm = pi.jointPosition ["RightArm"];
//
//		oriLeftArm.x *= 1.7f;
//		oriRightArm.x *= 1.7f;
//
//		pi.jointPosition ["LeftArm"] = oriLeftArm;
//		pi.jointPosition ["RightArm"] = oriRightArm;
//
//		Vector3 oriLeftUpperArm = pi.jointPosition ["LeftUpperArm"];
//		Vector3 oriRightUpperArm = pi.jointPosition ["RightUpperArm"];
//
//		oriLeftUpperArm.x *= 1.2f;
//		oriRightUpperArm.x *= 1.2f;
//
//		pi.jointPosition ["LeftUpperArm"] = oriLeftUpperArm;
//		pi.jointPosition ["RightUpperArm"] = oriRightUpperArm;
//
//		Vector3 oriLeftForeArm = pi.jointPosition ["LeftForeArm"];
//		Vector3 oriRightForeArm = pi.jointPosition ["RightForeArm"];
//
//		oriLeftForeArm.x *= 1.1f;
//		oriRightForeArm.x *= 1.1f;
//
//		pi.jointPosition ["LeftForeArm"] = oriLeftForeArm;
//		pi.jointPosition ["RightForeArm"] = oriRightForeArm;
//
//		Vector3 LeftUpperLeg = pi.jointPosition ["LeftUpLeg"] + pi.jointPosition ["LeftLeg"];
//		LeftUpperLeg /= 2.0f;
//
//		Vector3 LeftLowerLeg = pi.jointPosition ["LeftLeg"] + pi.jointPosition ["LeftFoot"];
//		LeftLowerLeg /= 2.0f;
//
//		pi.jointPosition.Add ("LeftUpperLeg", LeftUpperLeg);
//		pi.jointPosition.Add ("LeftLowerLeg", LeftLowerLeg);
//
//		Vector3 rightUpperLeg = pi.jointPosition ["RightUpLeg"] + pi.jointPosition ["RightLeg"];
//		rightUpperLeg /= 2.0f;
//
//		Vector3 rightLowerLeg = pi.jointPosition ["RightLeg"] + pi.jointPosition ["RightFoot"];
//		rightLowerLeg /= 2.0f;
//
//		pi.jointPosition.Add ("RightUpperLeg", rightUpperLeg);
//		pi.jointPosition.Add ("RightLowerLeg", rightLowerLeg);
//
//		pi.jointPosition.Remove ("LeftHand");
//		pi.jointPosition.Remove ("RightHand");
//
////		pi.jointPosition.Remove ("LeftForeArm");
////		pi.jointPosition.Remove ("RightForeArm");
////		pi.jointPosition.Remove ("LeftHand");
////		pi.jointPosition.Remove ("RightHand");
////
////		pi.jointPosition.Remove ("LeftLeg");
////		pi.jointPosition.Remove ("RightLeg");

		return pi;
	}
}
