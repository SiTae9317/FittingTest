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
//	public Dictionary<int, string> indexToName = new Dictionary<int, string>();
//	public Dictionary<string, int> nameToIndex = new Dictionary<string, int>();
//	public List<Matrix4x4> oriBoneMatrices = new List<Matrix4x4> ();
//	public Material mat;
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



public class NewMeshPartition : MonoBehaviour 
{
	public GameObject charObj;
	public GameObject clothObj;
//	public Material defaultMat;
	public bool isEnd = false;
//	private List<CustomSkinnedMesh> newCSMS = new List<CustomSkinnedMesh>();
//	private int partitionMeshCount = -1;
//	private List<Matrix4x4> oriBoneMatrices;

	public List<PartitionInformation> pis = null;

	private List<Thread> threads;

	// Use this for initialization
	void Start () 
	{		
//		startMP ();
	}

	IEnumerator checkThread()
	{
		while (threads [0].IsAlive) 
		{
			yield return null;
		}

		Debug.Log ("Thread End");

		List<Matrix4x4> bindPose = new List<Matrix4x4> ();
		List<Transform> bones = new List<Transform> ();
		Transform rootBone = null;

		SkinnedMeshRenderer smr = charObj.GetComponentInChildren<SkinnedMeshRenderer> ();
		bindPose.AddRange (smr.sharedMesh.bindposes);
		bones.AddRange (smr.bones);
		rootBone = smr.rootBone;

		GameObject combineMesh = new GameObject ();
		combineMesh.name = "Combine Mesh";

		for (int i = 0; i < pis.Count; i++) 
		{
			GameObject parentObj = new GameObject ();
			parentObj.name = "Mesh";
			parentObj.transform.parent = combineMesh.transform;

			List<CustomSkinnedMesh> csms = pis [i].csms;

			for (int j = 0; j < csms.Count; j++) 
			{
				GameObject newObj = new GameObject ();

				newObj.transform.parent = parentObj.transform;

				newObj.name = csms [j].name;

				SkinnedMeshRenderer newSmr = newObj.AddComponent<SkinnedMeshRenderer> ();

				newSmr.material = pis[i].mat;

				Mesh mesh = new Mesh ();

				mesh.vertices = csms [j].vertices.ToArray ();
				mesh.normals = csms [j].normals.ToArray ();
				mesh.uv = csms [j].uv.ToArray ();
				mesh.boneWeights = csms [j].boneWeights.ToArray ();
				mesh.triangles = csms [j].triangles.ToArray ();
				mesh.bindposes = bindPose.ToArray ();

				newSmr.bones = bones.ToArray ();
				newSmr.rootBone = rootBone;
				newSmr.sharedMesh = mesh;
			}

			parentObj.transform.localScale = Vector3.one;
		}

		foreach (SkinnedMeshRenderer removeSmr in charObj.GetComponentsInChildren<SkinnedMeshRenderer>()) 
		{
			Destroy (removeSmr.gameObject);
		}

		foreach (SkinnedMeshRenderer removeSmr in clothObj.GetComponentsInChildren<SkinnedMeshRenderer>()) 
		{
			Destroy (removeSmr.gameObject);
		}

		combineMesh.transform.parent = charObj.transform;

		Destroy (clothObj);
//
//		GameObject sourceObj = oriObj;
//
//		if (targetObj != null) 
//		{
//			sourceObj = targetObj;
//		}
//
//		SkinnedMeshRenderer sourceSmr = sourceObj.GetComponentInChildren<SkinnedMeshRenderer>();
//		bindPose.AddRange (sourceSmr.sharedMesh.bindposes);
//		bones.AddRange (sourceSmr.bones);
//		rootBone = sourceSmr.rootBone;
//
//		foreach (SkinnedMeshRenderer oriSmr in oriObj.GetComponentsInChildren<SkinnedMeshRenderer>()) 
//		{
//			Destroy (oriSmr.gameObject);
//		}
//
//		GameObject parentObj = new GameObject ();
//		parentObj.name = "Mesh";
//		parentObj.transform.parent = oriObj.transform;
//
//		for (int i = 0; i < newCSMS.Count; i++) 
//		{
//			GameObject newObj = new GameObject ();
//
//			newObj.transform.parent = parentObj.transform;
//
//			newObj.name = newCSMS [i].name;
//
//			SkinnedMeshRenderer smr = newObj.AddComponent<SkinnedMeshRenderer> ();
//
//			smr.material = defaultMat;
//
//			Mesh mesh = new Mesh ();
//
//			mesh.vertices = newCSMS [i].vertices.ToArray ();
//			mesh.normals = newCSMS [i].normals.ToArray ();
//			mesh.uv = newCSMS [i].uv.ToArray ();
//			mesh.boneWeights = newCSMS [i].boneWeights.ToArray ();
//			mesh.triangles = newCSMS [i].triangles.ToArray ();
//			mesh.bindposes = bindPose.ToArray ();
//			//				mesh.bindposes = newCSMS [i].boneMatrices.ToArray ();
//
//			smr.bones = bones.ToArray ();
//			smr.rootBone = rootBone;
//			smr.sharedMesh = mesh;
//		}
//
//		newCSMS.Clear ();
//		isEnd = true;
//
//		parentObj.transform.localScale = Vector3.one;

		Debug.Log ("End");
	}

	public void startMP()
	{
		initialize (0, charObj);
		initialize (1, clothObj);

		ParameterizedThreadStart pts = new ParameterizedThreadStart (settingPartition);
		Thread t = new Thread (pts);
		threads.Add (t);
		threads[0].Start (pis);

		StartCoroutine (checkThread ());
	}

	void settingPartition(object arg)
	{
		List<PartitionInformation> pis = (List<PartitionInformation>)arg;

		for (int i = 0; i < pis.Count; i++) 
		{
			ParameterizedThreadStart pts = new ParameterizedThreadStart (calcPartitionInfo);
			Thread t = new Thread (pts);
			threads.Add (t);
			threads[i + 1].Start (pis[i]);
		}

		if (threads [1].IsAlive) 
		{
			threads [1].Join ();
		}

		if (threads [2].IsAlive) 
		{
			threads [2].Join ();
		}

		List<CustomSkinnedMesh> csms = pis [1].csms;

		for (int i = 0; i < csms.Count; i++) 
		{
			List<BoneWeight> boneWeights = csms [i].boneWeights;
			for (int j = 0; j < boneWeights.Count; j++) 
			{
				BoneWeight bw = boneWeights [j];
				bw.boneIndex0 = pis [0].nameToIndex [pis [1].indexToName [bw.boneIndex0]];
				bw.boneIndex1 = pis [0].nameToIndex [pis [1].indexToName [bw.boneIndex1]];
				bw.boneIndex2 = pis [0].nameToIndex [pis [1].indexToName [bw.boneIndex2]];
				bw.boneIndex3 = pis [0].nameToIndex [pis [1].indexToName [bw.boneIndex3]];

				boneWeights [j] = bw;
			}
		}
	}

	// Update is called once per frame
	void Update () 
	{
		;
	}

	public void construct()
	{		
		threads = new List<Thread> ();

		pis = new List<PartitionInformation> ();

		PartitionInformation charPi = new PartitionInformation ();

		SkinnedMeshRenderer charSmr = charObj.GetComponentInChildren<SkinnedMeshRenderer> ();

		for (int i = 0; i < charSmr.bones.Length; i++) 
		{
			charPi.oriBoneMatrices.Add (charSmr.bones [i].localToWorldMatrix * charSmr.sharedMesh.bindposes [i]);
		}

		pis.Add (charPi);

		PartitionInformation clothPi = new PartitionInformation ();

		SkinnedMeshRenderer clothSmr = charObj.GetComponentInChildren<SkinnedMeshRenderer> ();

		for (int i = 0; i < clothSmr.bones.Length; i++) 
		{
			clothPi.oriBoneMatrices.Add (clothSmr.bones [i].localToWorldMatrix * clothSmr.sharedMesh.bindposes [i]);
		}

		pis.Add (clothPi);
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

				bm0 = pi.oriBoneMatrices [weight.boneIndex0];
				bm1 = pi.oriBoneMatrices [weight.boneIndex1];
				bm2 = pi.oriBoneMatrices [weight.boneIndex2];
				bm3 = pi.oriBoneMatrices [weight.boneIndex3];

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

		List<CustomSkinnedMesh> newCSMS = new List<CustomSkinnedMesh>();

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
				}
			}

			newVT.Clear ();
			newVT = null;

			newCSMS.Add (partCSM);
		}

		pi.csms.Clear ();
		pi.csms = newCSMS;

//		partitionMeshCount = newCSMS.Count;

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

	void initialize(int index, GameObject sourceObj)
	{		
		PartitionInformation pi = pis[index];

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

		Transform[] bones = sourceObj.GetComponentInChildren<SkinnedMeshRenderer> ().bones;
		pi.mat = sourceObj.GetComponentInChildren<SkinnedMeshRenderer> ().material;

		for (int i = 0; i < bones.Length; i++) 
		{
			pi.indexToName.Add (i, bones [i].name);
			pi.nameToIndex.Add (bones [i].name, i);
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
	}
}
