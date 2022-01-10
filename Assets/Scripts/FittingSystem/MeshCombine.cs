using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;

public class MeshCombine : MonoBehaviour 
{
	public GameObject charObj;
	public GameObject clothObj;

    public GameObject hair;

	public List<PartitionInformation> pis = null;

	private List<Thread> threads;

	IEnumerator checkThread(System.Action endCallback)
	{
        GameObject keepChar = null;
        GameObject keepCloth = null;

		while (threads [0].IsAlive) 
		{
			yield return null;
		}

		List<Matrix4x4> bindPose = new List<Matrix4x4> ();
		List<Transform> bones = new List<Transform> ();
		Transform rootBone = null;

        SkinnedMeshRenderer smr = null;//

        foreach(SkinnedMeshRenderer curSmr in charObj.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            if(curSmr.name == "mesh")
            {
                smr = curSmr;
            }

            if(curSmr.name.Contains("hair"))
            {
                hair = curSmr.gameObject;
            }
        }
		bindPose.AddRange (smr.sharedMesh.bindposes);
		bones.AddRange (smr.bones);
		rootBone = smr.rootBone;

        GameObject combineMesh = new GameObject ();
		combineMesh.name = "Combine Mesh";

		for (int i = 0; i < pis.Count; i++) 
		{
			GameObject parentObj = new GameObject ();
			if (i == 0) 
			{
				parentObj.name = "Char";
                parentObj.AddComponent<MeshDataCombine>();
                keepChar = parentObj;
			} 
			else if (i == 1) 
			{
				parentObj.name = "Cloth";
                keepCloth = parentObj;
            }

			parentObj.transform.parent = combineMesh.transform;

			List<CustomSkinnedMesh> csms = pis [i].csms;

			for (int j = 0; j < csms.Count; j++) 
			{
				GameObject newObj = new GameObject ();

				newObj.transform.parent = parentObj.transform;

				newObj.name = csms [j].name;

				SkinnedMeshRenderer newSmr = newObj.AddComponent<SkinnedMeshRenderer> ();

				if (i == 1) 
				{
					newSmr.enabled = false;
				}

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

        Transform removeCharTrs = null;

        foreach(SkinnedMeshRenderer curSmr in charObj.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            if(curSmr.name == "mesh")
            {
                removeCharTrs = curSmr.transform;
            }
        }
        
		while (removeCharTrs != charObj.transform) 
		{
			GameObject destroyObj = removeCharTrs.gameObject;
			removeCharTrs = removeCharTrs.parent;
			Destroy (destroyObj);
        }

        Transform removeClothTrs = null;

        foreach (SkinnedMeshRenderer curSmr in clothObj.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            if (curSmr.name == "mesh")
            {
                removeClothTrs = curSmr.transform;
            }
        }

		while (removeClothTrs != clothObj.transform) 
		{
			GameObject destroyObj = removeClothTrs.gameObject;
			removeClothTrs = removeClothTrs.parent;
			Destroy (destroyObj);
		}

//		foreach (SkinnedMeshRenderer removeSmr in charObj.GetComponentsInChildren<SkinnedMeshRenderer>()) 
//		{
//			Destroy (removeSmr.gameObject);
//		}
//
//		foreach (SkinnedMeshRenderer removeSmr in clothObj.GetComponentsInChildren<SkinnedMeshRenderer>()) 
//		{
//			Destroy (removeSmr.gameObject);
//		}

		combineMesh.transform.parent = charObj.transform;
		combineMesh.transform.localScale = Vector3.one;

		Destroy (clothObj);

		Debug.Log ("End");

        charObj = keepChar;
        clothObj = keepCloth;

        endCallback ();
	}

	public void startMP(System.Action endCallback)
	{
		initialize (0, charObj);
		initialize (1, clothObj);

		ParameterizedThreadStart pts = new ParameterizedThreadStart (settingPartition);
		Thread t = new Thread (pts);
		threads.Add (t);
		threads[0].Start (pis);

		StartCoroutine (checkThread (endCallback));
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

	public void construct()
	{		
		threads = new List<Thread> ();

		pis = new List<PartitionInformation> ();

		PartitionInformation charPi = new PartitionInformation ();

        SkinnedMeshRenderer charSmr = null;//charObj.GetComponentInChildren<SkinnedMeshRenderer> ();

        foreach(SkinnedMeshRenderer curSmr in charObj.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            if(curSmr.name == "mesh")
            {
                charSmr = curSmr;
            }
        }

		for (int i = 0; i < charSmr.bones.Length; i++)
        {
            charPi.oriBoneMatrices.Add (charSmr.bones [i].localToWorldMatrix * charSmr.sharedMesh.bindposes [i]);
		}

		pis.Add (charPi);

		PartitionInformation clothPi = new PartitionInformation ();


        SkinnedMeshRenderer clothSmr = null;//charObj.GetComponentInChildren<SkinnedMeshRenderer> ();

        foreach (SkinnedMeshRenderer curSmr in clothObj.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            if (curSmr.name == "mesh")
            {
                clothSmr = curSmr;
            }
        }

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

		for (int i = 0; i < csms.Count; i++) 
		{
			List<Vector3> vertices = new List<Vector3> ();
//			List<Vector3> worldVertices = new List<Vector3> ();
			List<Vector3> normals = new List<Vector3> ();
			List<Vector2> uv = new List<Vector2> ();
			List<int> triangles = new List<int> ();
			List<BoneWeight> boneWeights = new List<BoneWeight> ();

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

			List<Vector3> keepNormals = new List<Vector3> ();
			keepNormals.AddRange (csm.normals);

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

//				worldVertices.Add(vertexMatrix.MultiplyPoint3x4(keepVertices[j]));

				keepVertices[j] = vertexMatrix.MultiplyPoint3x4(keepVertices[j]);
				keepNormals [j] = vertexMatrix.MultiplyVector (keepNormals [j]);

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
				keepNormals[j] = vertexMatrix.inverse.MultiplyVector(keepNormals[j]);
			}

			vertices.AddRange (keepVertices);
			normals.AddRange (keepNormals);
			uv.AddRange (csm.uv);
			boneWeights.AddRange (keepBoneWeights);
			triangles.AddRange (keepTriangles);

			CustomSkinnedMesh newCsm = new CustomSkinnedMesh();
			newCsm.name = csm.name;
			newCsm.vertices.AddRange (vertices);
			newCsm.normals.AddRange (normals);
			newCsm.uv.AddRange (uv);
			newCsm.boneWeights.AddRange (boneWeights);
			newCsm.triangles.AddRange (triangles);
			newCsm.boneMatrices.AddRange (csm.boneMatrices);

			csms [i] = newCsm;
		}
	}

//	Dictionary<string, List<int>> calcPartition(List<Vector3> worldVertices, Dictionary<string, Vector3> jointPosition)
//	{	
//		Dictionary<string, List<int>> jointVertexIndex = new Dictionary<string, List<int>> ();	
//
//		for (int i = 0; i < worldVertices.Count; i++) 
//		{
//			Vector3 targetPostion = worldVertices [i];
//
//			Dictionary<string, Vector3>.Enumerator jointPositionEnum = jointPosition.GetEnumerator ();
//
//			string minimumName = "";
//			float minimumDis = float.MaxValue;
//
//			string secondName = "";
//			float seCondMinimumDis = float.MaxValue;
//
//			string thirdName = "";
//			float thirdMinimumDis = float.MaxValue;
//
//			string forthName = "";
//			float forthMinimumDis = float.MaxValue;
//
//			while(jointPositionEnum.MoveNext())
//			{
//				string name = jointPositionEnum.Current.Key;
//
//				Vector3 position = jointPositionEnum.Current.Value;
//
//				float dis = Vector3.Distance (targetPostion, position);
//
//				dis = Mathf.Abs (dis);
//
//				if (minimumDis > dis) 
//				{
//					forthName = thirdName;
//					forthMinimumDis = thirdMinimumDis;
//
//					thirdName = secondName;
//					thirdMinimumDis = seCondMinimumDis;
//
//					secondName = minimumName;
//					seCondMinimumDis = minimumDis;
//
//					minimumName = name;
//					minimumDis = dis;
//				}
//			}
//
//			if (!jointVertexIndex.ContainsKey (minimumName)) 
//			{
//				List<int> newIndexs = new List<int> ();
//
//				newIndexs.Add (i);
//
//				jointVertexIndex.Add (minimumName, newIndexs);
//			}
//			else 
//			{
//				jointVertexIndex [minimumName].Add (i);
//			}
//		}
//
//		return jointVertexIndex;
//	}

	CustomSkinnedMesh generateCSM(SkinnedMeshRenderer smr)
	{
		Mesh mesh = smr.sharedMesh;

		CustomSkinnedMesh csm = new CustomSkinnedMesh ();

		csm.name = mesh.name;

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

        SkinnedMeshRenderer meshSmr = null;

		foreach (SkinnedMeshRenderer smr in sourceObj.GetComponentsInChildren<SkinnedMeshRenderer> ()) 
		{
            if(smr.name == "mesh")
            { 
                CustomSkinnedMesh csm = generateCSM(smr);
                pi.csms.Add(csm);

                meshSmr = smr;
            }    
		}

		GameObject hips = null;

		for (int i = 0; i < sourceObj.transform.childCount; i++) 
		{
			hips = sourceObj.transform.GetChild (i).gameObject;
			if (hips.name.Equals ("skeleton")) 
			{
				break;
			}
		}

		Transform[] bones = meshSmr.bones;
		pi.mat = meshSmr.material;

		for (int i = 0; i < bones.Length; i++) 
		{
			pi.indexToName.Add (i, bones [i].name);
			pi.nameToIndex.Add (bones [i].name, i);
		}

		foreach (Transform ts in hips.GetComponentsInChildren<Transform> ()) 
		{
			pi.jointPosition.Add (ts.name, ts.position);
		}
	}
}
