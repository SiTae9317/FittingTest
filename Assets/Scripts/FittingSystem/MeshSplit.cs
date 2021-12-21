using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;
using System.IO;

public class MeshSplit : MonoBehaviour 
{
	public GameObject charObj;
	public GameObject clothObj;

	public List<PartitionInformation> pis = null;

	private List<Thread> threads;
	private CustomRayCast crc;

	public Dictionary<string, List<String>> closedRigData = null;
	public Dictionary<string, List<String>> standardJointData = null;

	IEnumerator checkThread(Action endCallback)
	{
		while (threads [0].IsAlive) 
		{
			yield return null;
		}

		List<Matrix4x4> bindPose = new List<Matrix4x4> ();
		List<Transform> bones = new List<Transform> ();
		Transform rootBone = null;

		SkinnedMeshRenderer smr = charObj.GetComponentInChildren<SkinnedMeshRenderer> ();
		bindPose.AddRange (smr.sharedMesh.bindposes);
		bones.AddRange (smr.bones);
		rootBone = smr.rootBone;

		List<List<GameObject>> newObjs = new List<List<GameObject>> ();

		for (int i = 0; i < pis.Count; i++) 
		{
			GameObject parentObj = null;

			if (i == 0) 
			{
				parentObj = charObj;
			} 
			else if (i == 1) 
			{
				parentObj = clothObj;
			}

			List<CustomSkinnedMesh> csms = pis [i].csms;

			List<GameObject> objs = new List<GameObject> ();

			for (int j = 0; j < csms.Count; j++) 
			{
				GameObject newObj = new GameObject ();

				newObj.name = csms [j].name + "_Mesh";

				SkinnedMeshRenderer newSmr = newObj.AddComponent<SkinnedMeshRenderer> ();

				newSmr.material = pis[i].mat;

				Mesh mesh = new Mesh ();

				List<Vector3> vertices = csms [j].vertices;
				List<BoneWeight> boneWeights = csms [j].boneWeights;
				List<Matrix4x4> boneMatrices = csms [j].boneMatrices;

				for (int k = 0; k < vertices.Count; k++) 
				{
					BoneWeight weight = boneWeights[k];

					Matrix4x4 bm0 = boneMatrices[weight.boneIndex0];
					Matrix4x4 bm1 = boneMatrices[weight.boneIndex1];
					Matrix4x4 bm2 = boneMatrices[weight.boneIndex2];
					Matrix4x4 bm3 = boneMatrices[weight.boneIndex3];

					Matrix4x4 vertexMatrix = new Matrix4x4();

					for (int n = 0; n < 16; n++)
					{
						vertexMatrix[n] =
							bm0[n] * weight.weight0 +
							bm1[n] * weight.weight1 +
							bm2[n] * weight.weight2 +
							bm3[n] * weight.weight3;
					}

					vertices [k] = vertexMatrix.inverse.MultiplyPoint3x4 (vertices [k]);
				}

				mesh.vertices = csms [j].vertices.ToArray ();
				mesh.normals = csms [j].normals.ToArray ();
				mesh.uv = csms [j].uv.ToArray ();
				mesh.boneWeights = csms [j].boneWeights.ToArray ();
				mesh.triangles = csms [j].triangles.ToArray ();
				mesh.bindposes = bindPose.ToArray ();

				mesh.RecalculateNormals ();

				newSmr.bones = bones.ToArray ();
				newSmr.rootBone = rootBone;
				newSmr.sharedMesh = mesh;

				objs.Add (newObj);
			}

			newObjs.Add (objs);
		}

		foreach (SkinnedMeshRenderer removeSmr in charObj.GetComponentsInChildren<SkinnedMeshRenderer>()) 
		{
			Destroy (removeSmr.gameObject);
		}

		foreach (SkinnedMeshRenderer removeSmr in clothObj.GetComponentsInChildren<SkinnedMeshRenderer>()) 
		{
			Destroy (removeSmr.gameObject);
		}

		for (int i = 0; i < newObjs.Count; i++) 
		{
			GameObject parentObj = null;

			if (i == 0) 
			{
				parentObj = charObj;
			}
			else if (i == 1) 
			{
				parentObj = clothObj;
			}
			
			for (int j = 0; j < newObjs [i].Count; j++) 
			{
				newObjs [i] [j].transform.parent = parentObj.transform;
				newObjs [i] [j].transform.localScale = Vector3.one;
			}
		}

		Destroy (crc);

		endCallback ();
	}

	void exportBinaryCRD()
	{
		using (FileStream fs = new FileStream ("C://RigData//ClosedRigData.bytes", FileMode.OpenOrCreate, FileAccess.Write)) 
		{
			using(BinaryWriter bw = new BinaryWriter(fs))
			{
				Dictionary<string, List<string>>.Enumerator crdEnum = closedRigData.GetEnumerator ();

				bw.Write (closedRigData.Count);

				while (crdEnum.MoveNext ()) 
				{
					List<string> values = crdEnum.Current.Value;

					bw.Write (values.Count);

					for (int i = 0; i < values.Count; i++) 
					{
						bw.Write (values[i]);
					}
				}
				bw.Close();
			}
			fs.Close();
		}
	}

	void importBinaryCRD()
	{
		closedRigData = new Dictionary<string, List<string>> ();

		TextAsset ta = Resources.Load ("ClosedRigData") as TextAsset;

		using (MemoryStream ms = new MemoryStream (ta.bytes)) 
		{
			using (BinaryReader br = new BinaryReader (ms)) 
			{
				int jointCount = br.ReadInt32 ();

				for (int i = 0; i < jointCount; i++) 
				{
					int lineCount = br.ReadInt32 ();

					List<string> datas = new List<string> ();

					for (int j = 0; j < lineCount; j++) 
					{
						string data = br.ReadString ();
						datas.Add (data);
					}
					closedRigData.Add (datas [0], datas);
				}
				br.Close ();
			}
			ms.Close ();
		}
	}

	void setClosedRigData()
	{
		closedRigData = new Dictionary<string, List<string>> ();

		string data = File.ReadAllText ("C:\\RigData\\ClosedRigData.txt");

		string[] crds = data.Split (new char[]{ '\r', '\n' },StringSplitOptions.RemoveEmptyEntries);

		for (int i = 0; i < crds.Length; i++) 
		{
			string[] crd = crds [i].Split (' ');

			List<string> closedData = new List<string> ();

			for (int j = 0; j < crd.Length; j++) 
			{
				closedData.Add (crd [j]);
			}

			closedRigData.Add (crd [0], closedData);
		}

		Dictionary<string, List<string>>.Enumerator crdEnum = closedRigData.GetEnumerator ();

		while (crdEnum.MoveNext ()) 
		{
			string deb = crdEnum.Current.Key;
			deb += " = {";

			for (int i = 0; i < crdEnum.Current.Value.Count; i++) 
			{
				if (!closedRigData.ContainsKey (crdEnum.Current.Value [i])) 
				{
					Debug.Log ("error");
				}
				deb += crdEnum.Current.Value [i];
				deb += " ";
			}
			deb += "}";
			Debug.Log (deb);
		}
	}

	void setStandardJointData()
	{
		standardJointData = new Dictionary<string, List<string>> ();

		string data = File.ReadAllText ("C:\\RigData\\StandardJointData.txt");

		string[] sjds = data.Split (new char[]{ '\r', '\n' },StringSplitOptions.RemoveEmptyEntries);

		for (int i = 0; i < sjds.Length; i++) 
		{
			string[] sjd = sjds [i].Split (' ');

			List<string> standardData = new List<string> ();

			for (int j = 0; j < sjd.Length; j++) 
			{
				standardData.Add (sjd [j]);
			}

			standardJointData.Add (sjd [0], standardData);
		}

		Dictionary<string, List<string>>.Enumerator sjdEnum = standardJointData.GetEnumerator ();

		while (sjdEnum.MoveNext ()) 
		{
			string deb = sjdEnum.Current.Key;
			deb += " = {";

			for (int i = 0; i < sjdEnum.Current.Value.Count; i++) 
			{
				if (!closedRigData.ContainsKey (sjdEnum.Current.Value [i])) 
				{
					Debug.Log ("error");
				}
				deb += sjdEnum.Current.Value [i];
				deb += " ";
			}
			deb += "}";
			Debug.Log (deb);
		}
	}

	void exportBinarySJD()
	{
		using (FileStream fs = new FileStream ("C://RigData//StandardJointData.bytes", FileMode.OpenOrCreate, FileAccess.Write)) 
		{
			using(BinaryWriter bw = new BinaryWriter(fs))
			{
				Dictionary<string, List<string>>.Enumerator sjdEnum = standardJointData.GetEnumerator ();

				bw.Write (standardJointData.Count);

				while (sjdEnum.MoveNext ()) 
				{
					List<string> values = sjdEnum.Current.Value;

					bw.Write (values.Count);

					for (int i = 0; i < values.Count; i++) 
					{
						bw.Write (values[i]);
					}
				}
				bw.Close();
			}
			fs.Close();
		}
	}

	void importBinarySJD()
	{
		standardJointData = new Dictionary<string, List<string>> ();

		TextAsset ta = Resources.Load ("StandardJointData") as TextAsset;

		using (MemoryStream ms = new MemoryStream (ta.bytes)) 
		{
			using (BinaryReader br = new BinaryReader (ms)) 
			{
				int jointCount = br.ReadInt32 ();

				for (int i = 0; i < jointCount; i++) 
				{
					int lineCount = br.ReadInt32 ();

					List<string> datas = new List<string> ();

					for (int j = 0; j < lineCount; j++) 
					{
						string data = br.ReadString ();
						datas.Add (data);
					}
					standardJointData.Add (datas [0], datas);
				}
				br.Close ();
			}
			ms.Close ();
		}
	}

	public void startMeshSplit(Action endCallback)
	{
		crc = gameObject.AddComponent<CustomRayCast> ();
		ParameterizedThreadStart pts = new ParameterizedThreadStart (settingPartition);
		Thread t = new Thread (pts);
		threads.Add (t);
		threads[0].Start (pis);
		StartCoroutine (checkThread (endCallback));
	}

	void settingPartition(object arg)
	{
		List<PartitionInformation> pis = (List<PartitionInformation>)arg;

		PartitionInformation charPis = pis [0];
		PartitionInformation clothPis = pis [1];

		ParameterizedThreadStart charPts = new ParameterizedThreadStart (calcPartitionInfo);
		Thread charT = new Thread (charPts);
		threads.Add (charT);
		threads[1].Start (charPis);

		ParameterizedThreadStart clothPts = new ParameterizedThreadStart (clothPartitionInfo);
		Thread clothT = new Thread (clothPts);
		threads.Add (clothT);
		threads[2].Start (clothPis);

		for (int i = 1; i < threads.Count; i++) 
		{
			if (threads [i].IsAlive) 
			{
				threads [i].Join ();
			}
		}

//		if (threads [1].IsAlive) 
//		{
//			threads [1].Join ();
//		}
//
//		if (threads [2].IsAlive) 
//		{
//			threads [2].Join ();
//		}

		threads [1] = null;
		threads [2] = null;

		threads.RemoveAt (2);
		threads.RemoveAt (1);

		Dictionary<string, bool> removeJoint = new Dictionary<string, bool> ();

		Dictionary<string, List<List<int>>> jvi = clothPis.jointVertexIndex;

		Dictionary<string, List<List<int>>>.Enumerator jviEnum = jvi.GetEnumerator ();

		List<CustomRaycastData> crds = new List<CustomRaycastData> ();

		bool isPant = false;

		if (jvi.ContainsKey ("LeftFoot") && jvi.ContainsKey ("RightFoot")) 
		{
			isPant = true;
		}

		Debug.Log ("isPant = " + isPant);

		while (jviEnum.MoveNext ()) 
		{
			string key = jviEnum.Current.Key;
			List<List<int>> value = jviEnum.Current.Value;

			CustomRaycastData crd = new CustomRaycastData ();

			crd.name = key;
			crd.isPant = isPant;

			for (int i = 0; i < value.Count; i++) 
			{
				for (int j = 0; j < value [i].Count; j++) 
				{
					Vector3 vertPos = clothPis.csms [i].vertices [value [i] [j]];
					crd.vertices.Add (vertPos);
					crd.normals.Add (clothPis.csms [i] .normals[value[i][j]]);
					crd.calcNormals.Add (clothPis.csms [i].calcNormals [value [i] [j]]);
					crd.boneWeights.Add (clothPis.csms [i] .boneWeights[value[i][j]]);
					List<Vector3> cvList = new List<Vector3> ();

					using (HashSet<Vector3>.Enumerator closeVertEnum = clothPis.vertexToCloseVertexs [vertPos].GetEnumerator ()) 
					{
						while (closeVertEnum.MoveNext ()) 
						{
							cvList.Add (closeVertEnum.Current);
						}
					}

					if (!crd.vertexToCloseVertices.ContainsKey (vertPos)) 
					{
						crd.vertexToCloseVertices.Add (vertPos, cvList);
					}
				}
			}

			List<string> crdJointName = closedRigData [key];

			bool isRemove = true;

			for (int i = 0; i < crdJointName.Count; i++) 
			{
				for (int j = 0; j < charPis.csms.Count; j++) 
				{
					if (crdJointName[i].Equals (charPis.csms [j].name)) 
					{
						crd.csms.Add (charPis.csms [j]);
						break;
					}
				}

				if (!jvi.ContainsKey (crdJointName [i])) 
				{
					isRemove = false;
				}
			}

			removeJoint.Add (key, isRemove);

			crds.Add (crd);

			ParameterizedThreadStart crdPts = new ParameterizedThreadStart (calculateCustomRayCast);
			Thread crdT = new Thread (crdPts);
			threads.Add (crdT);

			crdT.Start (crd);
		}

		while (true) 
		{
			for (int i = 1; i < threads.Count; i++) 
			{
				if (threads [i].IsAlive) 
				{
					i--;
					continue;
				}
			}

			break;
		}

		for (int i = threads.Count - 1; i > 0; i--) 
		{
			threads [i] = null;

			threads.RemoveAt (i);
		}

		jviEnum = jvi.GetEnumerator ();

		int k = 0;

		while (jviEnum.MoveNext ()) 
		{
			List<List<int>> value = jviEnum.Current.Value;

			int l = 0;

			for (int i = 0; i < value.Count; i++) 
			{
				for (int j = 0; j < value [i].Count; j++) 
				{
					clothPis.csms [i].vertices [value[i][j]] = crds[k].vertices[l];
					clothPis.csms [i] .normals[value[i][j]] = crds[k].normals[l];
					clothPis.csms [i] .boneWeights[value[i][j]] = crds[k].boneWeights[l];
					l++;
				}
			}
			k++;
		}

		List<int> removeIndex = new List<int> ();

		for (int i = 0; i < charPis.csms.Count; i++) 
		{
			string jointName = charPis.csms [i].name;

			if (jointName.Contains ("Hand") || jointName.Contains("Neck") || jointName.Contains("Foot")) 
			{
				continue;
			}

			if(removeJoint.ContainsKey(jointName))
			{
				if (removeJoint [jointName]) 
				{
					removeIndex.Add (i);
				}
			}
		}

		removeIndex.Sort ();
		removeIndex.Reverse ();

		for (int i = 0; i < removeIndex.Count; i++) 
		{
			charPis.csms.RemoveAt(removeIndex[i]);
		}

		Debug.Log (threads.Count);

		Debug.Log ("thread end");
	}

//	void calculateCustomRayCast(object arg)
//	{
//		CustomRaycastData crd = (CustomRaycastData)arg;
//
//		List<Vector3> clothVertices = crd.vertices;
//		List<Vector3> clothNormals = crd.normals;
//		List<BoneWeight> clothBoneWeight = crd.boneWeights;
//
//		List<Vector3> charVertices = new List<Vector3> ();
//		List<Vector3> charNormals = new List<Vector3> ();
//		List<BoneWeight> charBoneWeights = new List<BoneWeight> ();
//
//		for (int i = 0; i < crd.csms.Count; i++) 
//		{
//			for (int j = 0; j < crd.csms [i].triangles.Count; j++) 
//			{
//				int triIdnex = crd.csms [i].triangles [j];
//
//				charVertices.Add (crd.csms [i].vertices [triIdnex]);
//				charNormals.Add (crd.csms [i].normals [triIdnex]);
//				charBoneWeights.Add (crd.csms [i].boneWeights [triIdnex]);
//			}
//			break;
//		}
//
//		int hitCount = 0;
//		int nohitCount = 0;
//
//		for (int i = 0; i < clothVertices.Count; i++) 
//		{
//			Vector3 position = clothVertices [i];
//			Vector3 normal = clothNormals [i];
//			normal.Normalize ();
//
//			bool isHit = false;
//
//			for (int j = 0; j < charVertices.Count; j += 3) 
//			{
//				Vector3 hitPosition = Vector3.zero;
//
//				if (CustomRayCast.triangleRayCast (charVertices[j + 0], charVertices[j + 1], charVertices[j + 2], position, normal, ref hitPosition)) 
//				{
//					float dis = Vector3.Distance (position, hitPosition);
//
//					if (dis > 0.05f) 
//					{
//						clothVertices [i] += normal * (0.005f);
//					}
//					else 
//					{
//						clothVertices [i] += normal * (dis + 0.005f);
//					}
//
//					clothBoneWeight [i] = charBoneWeights [j];
//
////					clothVertices [i] = hitPosition;
//
//					isHit = true;
//
//					break;
//				}
//					
//			}
//
//			if (isHit) 
//			{
//				hitCount++;
//			}
//			else 
//			{
//				clothVertices [i] += normal * (0.005f);
//				nohitCount++;
//			}
////			clothVertices [i] = Vector3.zero;
//		}
//
//		Debug.Log (clothVertices.Count + " " + hitCount + " " + nohitCount);
//
//		charVertices.Clear ();
//		charNormals.Clear ();
//		charBoneWeights.Clear ();
//
//		charVertices = null;
//		charNormals = null;
//		charBoneWeights = null;
//	}

//	void calculateCustomRayCast(object arg)
//	{
//		CustomRaycastData crd = (CustomRaycastData)arg;
//
//		List<Vector3> clothVertices = crd.vertices;
//		List<Vector3> clothNormals = crd.normals;
//		List<BoneWeight> clothBoneWeight = crd.boneWeights;
//
//		List<Vector3> charVertices = new List<Vector3> ();
//		List<Vector3> charNormals = new List<Vector3> ();
//		List<BoneWeight> charBoneWeights = new List<BoneWeight> ();
//
//		Dictionary<Vector3, bool> vertIsHit = new Dictionary<Vector3, bool> ();
//
//		using (Dictionary<Vector3, List<Vector3>>.Enumerator vertCloseEnum = crd.vertexToCloseVertices.GetEnumerator ()) 
//		{
//			while (vertCloseEnum.MoveNext ()) 
//			{
//				vertIsHit.Add (vertCloseEnum.Current.Key, false);
//			}
//		}
//
//		for (int i = 0; i < crd.csms.Count; i++) 
//		{
//			for (int j = 0; j < crd.csms [i].triangles.Count; j++) 
//			{
//				int triIdnex = crd.csms [i].triangles [j];
//
//				charVertices.Add (crd.csms [i].vertices [triIdnex]);
//				charNormals.Add (crd.csms [i].normals [triIdnex]);
//				charBoneWeights.Add (crd.csms [i].boneWeights [triIdnex]);
//			}
//			break;
//		}
//
//		int hitCount = 0;
//		int nohitCount = 0;
//
//		for (int i = 0; i < clothVertices.Count; i++) 
//		{
//			Vector3 position = clothVertices [i];
//			Vector3 normal = clothNormals [i];
//			normal.Normalize ();
//
//			bool isHit = false;
//
//			float minDis = float.MaxValue;
//			Vector3 minVec = Vector3.zero;
//			int minBoneWeight = -1;
//			float offset = 0.005f;
//
//			for (int j = 0; j < charVertices.Count; j += 3) 
//			{
//				Vector3 hitPosition = Vector3.zero;
//
//				if (CustomRayCast.triangleRayCast (charVertices[j + 0], charVertices[j + 1], charVertices[j + 2], position, normal, ref hitPosition)) 
//				{
//					Vector3 charNormal = charNormals [j + 0] + charNormals [j + 1] + charNormals [j + 2];
//					charNormal.Normalize ();
//
//					if (Vector3.Dot (normal, charNormal) < 0) 
//					{
//						continue;
//					}
//
//					float dis = Vector3.Distance (position, hitPosition);
//
//					if (minDis > dis) 
//					{
//						minDis = dis;
//						minVec = hitPosition;
//						minBoneWeight = j;
//						offset = 0.005f;
//					}
//
//					isHit = true;
//					vertIsHit [position] = true;
//
//					break;
//				}
//
//				if (CustomRayCast.triangleRayCast (charVertices[j + 0], charVertices[j + 1], charVertices[j + 2], position, -normal, ref hitPosition)) 
//				{
//					Vector3 charNormal = charNormals [j + 0] + charNormals [j + 1] + charNormals [j + 2];
//					charNormal.Normalize ();
//
//					if (Vector3.Dot (-normal, charNormal) > 0) 
//					{
//						continue;
//					}
//
//					float dis = Vector3.Distance (position, hitPosition);
//
//					if (minDis > dis) 
//					{
//						minDis = dis;
//						minVec = hitPosition;
//						minBoneWeight = j;
//						offset = -0.01f;
//					}
//
//					isHit = true;
//					vertIsHit [position] = true;
//
//					break;
//				}
//			}
//
//			if (isHit) 
//			{
//				hitCount++;
//
//				float dis = Vector3.Distance (position, minVec);
//
//				Vector3 moveVec = minVec - position;
//
//				moveVec.Normalize ();
//
//				if (dis > 0.05f) 
//				{
////					clothVertices [i] = Vector3.zero;
////					clothVertices [i] += moveVec * (offset);
//				}
//				else 
//				{
//					clothVertices [i] += moveVec * (dis + offset);
//				}
//
//				clothBoneWeight [i] = charBoneWeights[minBoneWeight];
//			}
//			else 
//			{
////				clothVertices [i] = Vector3.zero;
////				clothVertices [i] += normal * (0.1f);
//				nohitCount++;
//			}
////			clothVertices [i] = Vector3.zero;
//		}
//
//		Debug.Log (clothVertices.Count + " " + hitCount + " " + nohitCount);
//
//		charVertices.Clear ();
//		charNormals.Clear ();
//		charBoneWeights.Clear ();
//		vertIsHit.Clear ();
//
//		charVertices = null;
//		charNormals = null;
//		charBoneWeights = null;
//		vertIsHit = null;
//	}

	void calculateCustomRayCast(object arg)
	{
		CustomRaycastData crd = (CustomRaycastData)arg;

		bool isPant = crd.isPant;
		bool isCenter = false;
		bool isOriNor = false;

		if (crd.name.Contains ("Center")) 
		{
			isCenter = true;
		}

//		if (crd.name.Contains ("tArm")) 
//		{
//			isCenter = true;
//		}

//		if (crd.name.Contains ("UpperArm")) 
//		{
//			isCenter = true;
//		}

		if (crd.name.Contains ("Neck")) 
		{
			isOriNor = true;
			isCenter = true;
		}

		List<Vector3> clothVertices = crd.vertices;
		List<Vector3> clothNormals = crd.normals;
		List<Vector3> clothCalcNormals = crd.calcNormals;
		List<BoneWeight> clothBoneWeight = crd.boneWeights;

		List<Vector3> charVertices = new List<Vector3> ();
		List<Vector3> charNormals = new List<Vector3> ();
		List<BoneWeight> charBoneWeights = new List<BoneWeight> ();

		Dictionary<Vector3, bool> vertIsHit = new Dictionary<Vector3, bool> ();
		Dictionary<Vector3, List<Vector3>> vertexToCloseVertices = crd.vertexToCloseVertices;
		Dictionary<Vector3, Vector3> beforeAfterPosition = new Dictionary<Vector3, Vector3> ();

		using (Dictionary<Vector3, List<Vector3>>.Enumerator vertCloseEnum = vertexToCloseVertices.GetEnumerator ()) 
		{
			while (vertCloseEnum.MoveNext ()) 
			{
				vertIsHit.Add (vertCloseEnum.Current.Key, false);
			}
		}

		for (int i = 0; i < crd.csms.Count; i++) 
		{
			for (int j = 0; j < crd.csms [i].triangles.Count; j++) 
			{
				int triIdnex = crd.csms [i].triangles [j];

				charVertices.Add (crd.csms [i].vertices [triIdnex]);
				charNormals.Add (crd.csms [i].normals [triIdnex]);
				charBoneWeights.Add (crd.csms [i].boneWeights [triIdnex]);
			}
			break;
		}

		int hitCount = 0;

		List<int> noHitIndexs = new List<int> ();

		for (int i = 0; i < clothVertices.Count; i++) 
		{
			Vector3 position = clothVertices [i];

			Vector3 normal = clothNormals [i].normalized + clothCalcNormals[i].normalized;

			if (isOriNor) 
			{
				normal = clothCalcNormals [i].normalized;
			}

//			if (isCenter) 
//			{
//				normal = clothCalcNormals [i].normalized;// - clothCalcNormals [i].normalized;
//				if (!isPant) 
//				{
//					normal.x = normal.y = 0.0f;
//				}
//			}
//			if (!isPant) 
//			{
//				if (isCenter) 
//				{
//					normal = clothNormals [i].normalized;
//				}
//			}

//			Vector3 normal = clothNormals [i].normalized * 0.2f + clothCalcNormals[i].normalized * 1.8f;
//			Vector3 normal = clothCalcNormals[i].normalized;
			normal.Normalize ();

			bool isHit = false;

			float minDis = float.MaxValue;
			Vector3 minVec = Vector3.zero;
			int minBoneWeight = -1;
			float offset = 0.005f;

			for (int j = 0; j < charVertices.Count; j += 3) 
			{
				Vector3 hitPosition = Vector3.zero;

				if (CustomRayCast.triangleRayCast (charVertices[j + 0], charVertices[j + 1], charVertices[j + 2], position, normal, ref hitPosition)) 
				{
					Vector3 charNormal = charNormals [j + 0] + charNormals [j + 1] + charNormals [j + 2];
					charNormal.Normalize ();

					if (Vector3.Dot (normal, charNormal) < 0.0f) 
//					if (Vector3.Dot (normal, charNormal) < 0.5f) 
					{
						continue;
					}

					float dis = Vector3.Distance (position, hitPosition);

//					if (dis < 0.1f) 
					{
						if (minDis > dis) 
						{
							minDis = dis;
							minVec = hitPosition;
							minBoneWeight = j;
							offset = 0.0055f;
//							offset = 0.008f;
						}

						isHit = true;
						vertIsHit [position] = true;
					}

//					break;
				}

				if (CustomRayCast.triangleRayCast (charVertices[j + 0], charVertices[j + 1], charVertices[j + 2], position, -normal, ref hitPosition)) 
				{
					Vector3 charNormal = charNormals [j + 0] + charNormals [j + 1] + charNormals [j + 2];
					charNormal.Normalize ();

					if (Vector3.Dot (-normal, charNormal) > 0.0f) 
//					if (Vector3.Dot (-normal, charNormal) > -0.5f) 
					{
						continue;
					}

					float dis = Vector3.Distance (position, hitPosition);

					if (dis < 0.1f) 
					{
						if (minDis > dis) 
						{
							minDis = dis;
							minVec = hitPosition;
							minBoneWeight = j;
//							offset = -0.01f;
							offset = -1.0f;
						}

						isHit = true;
						vertIsHit [position] = true;
					}

//					break;
				}
			}

			if (isHit) 
			{
				hitCount++;

				float dis = Vector3.Distance (position, minVec);

				Vector3 moveVec = minVec - position;

				moveVec.Normalize ();

				if (offset == -1.0f) 
				{
					clothVertices [i] += moveVec * (dis * 0.6f - 0.0055f);
//					clothVertices [i] += moveVec * (dis * 0.6f - 0.008f);
					if (!isCenter) 
					{
						clothBoneWeight [i] = charBoneWeights[minBoneWeight];
					}
				}
				else 
				{
					clothVertices [i] += moveVec * (dis + offset);
					if (!isCenter) 
					{
						clothBoneWeight [i] = charBoneWeights[minBoneWeight];
					}
				}

				if(!beforeAfterPosition.ContainsKey(position))
				{
					beforeAfterPosition.Add (position, clothVertices [i]);
				}
			}
			else 
			{
				noHitIndexs.Add (i);
			}
		}

		Debug.Log (crd.name + " " + clothVertices.Count + " " + hitCount + " " + noHitIndexs.Count);

		int nohitCount = 0;

		int loopCount = 10;

		do 
		{
			nohitCount = noHitIndexs.Count;

			for (int i = noHitIndexs.Count - 1; i >= 0; i--) 
			{
				int noHitClothIndex = noHitIndexs [i];
				Vector3 position = clothVertices [noHitClothIndex];
				List<Vector3> closeVertices = vertexToCloseVertices [position];

//			clothVertices [noHitClothIndex] -= clothCalcNormals [noHitClothIndex].normalized * 0.05f;

				for (int j = 0; j < closeVertices.Count; j++) 
				{
					Vector3 closeVert = closeVertices [j];

					if (!vertIsHit.ContainsKey (closeVert))
					{
						continue;
					}
					
					if (vertIsHit [closeVert]) 
					{
						Vector3 diffNor = beforeAfterPosition[closeVert] - closeVert;

//						Vector3 newPosition = beforeAfterPosition[closeVert] + diffNor;

						Vector3 newPosition = position + diffNor;

						if(Vector3.Distance(position, newPosition) > 1.0f)
						{
							newPosition = position;
						}

						clothVertices [noHitClothIndex] = newPosition;
						noHitIndexs.RemoveAt (i);
						vertIsHit[position] = true;
						if(!beforeAfterPosition.ContainsKey(position))
						{
							beforeAfterPosition.Add(position, newPosition);
						}
						break;

//						Vector3 diffNor = position - closeVert;
//
//						Vector3 newPosition = beforeAfterPosition[closeVert] + diffNor;
//
//						if(Vector3.Distance(position, newPosition) > 1.0f)
//						{
//							newPosition = position;
//						}
//
//						clothVertices [noHitClothIndex] = newPosition;
//						noHitIndexs.RemoveAt (i);
//						vertIsHit[position] = true;
//						if(!beforeAfterPosition.ContainsKey(position))
//						{
//							beforeAfterPosition.Add(position, newPosition);
//						}
//						break;
					}
				}
//				clothVertices[noHitClothIndex] = closeVertices[0];
			}
//			nohitCount = 0;
			loopCount--;
		} while(nohitCount != 0 && loopCount > 0);

		Debug.Log (nohitCount);

		if (!isPant) 
		{
			for (int i = noHitIndexs.Count - 1; i >= 0; i--) 
			{
				int noHitClothIndex = noHitIndexs [i];
				clothVertices [noHitClothIndex] -= clothCalcNormals [noHitClothIndex].normalized * 0.05f;
			}
		}

		beforeAfterPosition.Clear ();
		charVertices.Clear ();
		charNormals.Clear ();
		charBoneWeights.Clear ();
		vertIsHit.Clear ();
		noHitIndexs.Clear ();

		beforeAfterPosition = null;
		charVertices = null;
		charNormals = null;
		charBoneWeights = null;
		vertIsHit = null;
		noHitIndexs = null;
	}

	public void construct(GameObject sourceObj)
	{		
		threads = new List<Thread> ();

//		setClosedRigData ();
//
//		exportBinaryCRD ();

		importBinaryCRD ();

//		setStandardJointData ();

//		exportBinarySJD ();

		importBinarySJD ();

		pis = new List<PartitionInformation> ();

		int childCount = sourceObj.transform.childCount;

		for (int i = 0; i < childCount; i++) 
		{
			if (sourceObj.transform.GetChild (i).name.Equals ("Combine Mesh")) 
			{
				sourceObj = sourceObj.transform.GetChild (i).gameObject;
			}
		}

		childCount = sourceObj.transform.childCount;

		for (int i = 0; i < childCount; i++) 
		{
			if (sourceObj.transform.GetChild (i).name.Equals ("Char")) 
			{
				charObj = sourceObj.transform.GetChild (i).gameObject;
			}
			else if (sourceObj.transform.GetChild (i).name.Equals ("Cloth")) 
			{
				clothObj = sourceObj.transform.GetChild (i).gameObject;
			}
		}

		pis.Add (initialize (charObj));
		pis.Add (initialize (clothObj));
	}

	void clothPartitionInfo(object arg)
	{
		PartitionInformation pi = (PartitionInformation) arg;

		List<CustomSkinnedMesh> csms = pi.csms;

//		List<Vector3> vertices = new List<Vector3> ();
//		List<Vector3> normals = new List<Vector3> ();
//		List<BoneWeight> boneWeights = new List<BoneWeight> ();

		Dictionary<string, List<List<int>>> jointVertexIndex = pi.jointVertexIndex;

		Dictionary<Vector3, HashSet<Vector3>> vertexToCloseVertexs = pi.vertexToCloseVertexs;

		for (int i = 0; i < csms.Count; i++) 
		{
			CustomSkinnedMesh csm = csms [i];

			List<Vector3> vertices = csm.vertices;
			List<Vector3> normals = csm.normals;
			List<Vector3> calcNormals = csm.calcNormals;
			List<BoneWeight> boneWeights = csm.boneWeights;
			List<Matrix4x4> boneMatrices = csm.boneMatrices;
			List<int> triangles = csm.triangles;

			for (int j = 0; j < vertices.Count; j++) 
			{
				BoneWeight weight = boneWeights[j];

				Matrix4x4 bm0 = boneMatrices[weight.boneIndex0];
				Matrix4x4 bm1 = boneMatrices[weight.boneIndex1];
				Matrix4x4 bm2 = boneMatrices[weight.boneIndex2];
				Matrix4x4 bm3 = boneMatrices[weight.boneIndex3];

				Matrix4x4 vertexMatrix = new Matrix4x4();

				for (int n = 0; n < 16; n++)
				{
					vertexMatrix[n] =
						bm0[n] * weight.weight0 +
						bm1[n] * weight.weight1 +
						bm2[n] * weight.weight2 +
						bm3[n] * weight.weight3;
				}

				Vector3 position = vertexMatrix.MultiplyPoint3x4 (vertices [j]);
				Vector3 normal = vertexMatrix.MultiplyVector (normals [j]);

				string minJointName = minJointDetect (position, pi.jointPosition);

				if (jointVertexIndex.ContainsKey (minJointName)) 
				{
					jointVertexIndex [minJointName] [i].Add (j);
				}
				else 
				{
					List<List<int>> csmIndexs = new List<List<int>> ();

					for (int k = 0; k < csms.Count; k++) 
					{
						List<int> csmVertices = new List<int> ();
						csmIndexs.Add (csmVertices);
					}

					csmIndexs [i].Add (j);
					jointVertexIndex.Add (minJointName, csmIndexs);
				}

//				List<string> crdData = closedRigData [minJointName];
//				Vector3 jointPosition1 = pi.jointPosition [crdData[0]];
//				Vector3 jointPosition2 = pi.jointPosition [crdData[1]];

				List<string> stdData = standardJointData [minJointName];
				Vector3 jointPosition1 = pi.jointPosition [stdData[1]];
				Vector3 jointPosition2 = pi.jointPosition [stdData[2]];

				// calc normal vec
				normal.Normalize();

				calcNormals.Add(-1.0f * normalCalcFromJoint(position, jointPosition1, jointPosition2));
//				normal -= normalCalcFromJoint(position, jointPosition1, jointPosition2);
//				normal = normalCalcFromJoint(position, jointPosition1, jointPosition2);
//				normal.Normalize ();

				vertices [j] = position;
				normals [j] = normal;
			}

			Dictionary<int, HashSet<int>> vertCloseIndex = new Dictionary<int, HashSet<int>> ();

			for (int j = 0; j < triangles.Count; j++) 
			{
				int vertIndex = triangles [j];

				int triIndex = j / 3;
				triIndex *= 3;

				if (vertCloseIndex.ContainsKey (vertIndex)) 
				{
					vertCloseIndex [vertIndex].Add (triIndex + 0);
					vertCloseIndex [vertIndex].Add (triIndex + 1);
					vertCloseIndex [vertIndex].Add (triIndex + 2);
				}
				else 
				{
					HashSet<int> closeIndexs = new HashSet<int> ();

					closeIndexs.Add (triIndex + 0);
					closeIndexs.Add (triIndex + 1);
					closeIndexs.Add (triIndex + 2);

					vertCloseIndex.Add (vertIndex, closeIndexs);
				}
			}

			using (Dictionary<int, HashSet<int>>.Enumerator vciEnum = vertCloseIndex.GetEnumerator ()) 
			{
				while (vciEnum.MoveNext ()) 
				{
					int vertIndex = vciEnum.Current.Key;

					Vector3 vertPos = vertices [vertIndex];

					if (!vertexToCloseVertexs.ContainsKey (vertPos)) 
					{
						HashSet<Vector3> closeVertPos = new HashSet<Vector3> ();
						vertexToCloseVertexs.Add (vertPos, closeVertPos);
					}

					using (HashSet<int>.Enumerator closeIndexsEnum = vciEnum.Current.Value.GetEnumerator ()) 
					{
						while (closeIndexsEnum.MoveNext ()) 
						{
							int triIndex = closeIndexsEnum.Current;

							Vector3 triVertPos = vertices[triangles [triIndex]];
							vertexToCloseVertexs [vertPos].Add (triVertPos);
						}
					}
				}
			}

			vertCloseIndex.Clear ();
		}
	}

	string minJointDetect(Vector3 worldVertices, Dictionary<string, Vector3> jointPosition)
	{	
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

			float dis = Vector3.Distance (worldVertices, position);

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

		return minimumName;
	}

	void charPartitionInfo(object arg)
	{
		PartitionInformation pi = (PartitionInformation) arg;

		List<CustomSkinnedMesh> csms = pi.csms;

		List<Vector3> vertices = new List<Vector3> ();
	}

//	void calcPartitionInfo(object arg)
//	{
//		PartitionInformation pi = (PartitionInformation) arg;
//
//		Debug.Log (pi.csms.Count + " " + pi.jointPosition.Count);
//
//		List<CustomSkinnedMesh> csms = pi.csms;
//
//		List<Vector3> vertices = new List<Vector3> ();
//		List<Vector3> worldVertices = new List<Vector3> ();
//		List<Vector3> normals = new List<Vector3> ();
//		List<Vector2> uv = new List<Vector2> ();
//		List<int> triangles = new List<int> ();
//		List<BoneWeight> boneWeights = new List<BoneWeight> ();
//
//		for (int i = 0; i < csms.Count; i++) 
//		{
//			CustomSkinnedMesh csm = csms [i];
//
//			int beforeVertexCount = vertices.Count;
//			int beforeTriangleIndex = triangles.Count;
//			int vertexCount = csm.vertices.Count;
//
//			List<int> keepTriangles = new List<int> ();
//			keepTriangles.AddRange (csm.triangles);
//
//			for (int j = 0; j < keepTriangles.Count; j++) 
//			{
//				keepTriangles [j] += beforeVertexCount;
//			}
//
//			for (int j = 0; j < keepTriangles.Count; j++) 
//			{
//				int vertexIndex = keepTriangles [j];
//
//				if (pi.vertexToTriangle.ContainsKey (vertexIndex)) 
//				{
//					pi.vertexToTriangle [vertexIndex].Add (beforeTriangleIndex + j);
//				}
//				else 
//				{
//					List<int> containTriangle = new List<int> ();
//					containTriangle.Add(beforeTriangleIndex + j);
//					pi.vertexToTriangle .Add(vertexIndex, containTriangle);
//				}
//			}
//
//			List<BoneWeight> keepBoneWeights = new List<BoneWeight> ();
//			keepBoneWeights.AddRange (csm.boneWeights);
//
//			List<Vector3> keepVertices = new List<Vector3> ();
//			keepVertices.AddRange (csm.vertices);
//
//			List<Vector3> keepNormals = new List<Vector3> ();
//			keepNormals.AddRange (csm.normals);
//
//			for (int j = 0; j < keepVertices.Count; j++) 
//			{
//				BoneWeight weight = keepBoneWeights[j];
//
//				Matrix4x4 bm0 = csm.boneMatrices[weight.boneIndex0];
//				Matrix4x4 bm1 = csm.boneMatrices[weight.boneIndex1];
//				Matrix4x4 bm2 = csm.boneMatrices[weight.boneIndex2];
//				Matrix4x4 bm3 = csm.boneMatrices[weight.boneIndex3];
//
//				Matrix4x4 vertexMatrix = new Matrix4x4();
//
//				for (int n = 0; n < 16; n++)
//				{
//					vertexMatrix[n] =
//						bm0[n] * weight.weight0 +
//						bm1[n] * weight.weight1 +
//						bm2[n] * weight.weight2 +
//						bm3[n] * weight.weight3;
//				}
//
//				Vector3 position = vertexMatrix.MultiplyPoint3x4 (keepVertices [j]);
//				Vector3 normal = vertexMatrix.MultiplyVector (keepNormals [j]);
//
//				worldVertices.Add(position);
//
//				keepVertices [j] = position;
//				keepNormals [j] = normal;
//			}
//
//			vertices.AddRange (keepVertices);
//			normals.AddRange (csm.normals);
//			uv.AddRange (csm.uv);
//			boneWeights.AddRange (keepBoneWeights);
//			triangles.AddRange (keepTriangles);
//		}
//
//		Debug.Log ("Vertex = " + vertices.Count);
//		Debug.Log ("worldVertices = " + worldVertices.Count);
//		Debug.Log ("normals = " + normals.Count);
//		Debug.Log ("uv = " + uv.Count);
//		Debug.Log ("boneWeights = " + boneWeights.Count);
//		Debug.Log ("triangles = " + triangles.Count);
//		Debug.Log ("vertexToTriangle = " + pi.vertexToTriangle.Count);
//
//		Dictionary<string, List<int>> jointVertexIndex = calcPartition (worldVertices, pi.jointPosition);
//
//		Dictionary<string, List<int>>.Enumerator jointVertexIndexEnum = jointVertexIndex.GetEnumerator ();
//
//		int totalVertexCount = 0;
//		int totalTriangleCount = 0;
//
//		List<CustomSkinnedMesh> newCSMS = new List<CustomSkinnedMesh>();
//
//		while (jointVertexIndexEnum.MoveNext ()) 
//		{
//			Debug.Log (jointVertexIndexEnum.Current.Key + " " + jointVertexIndexEnum.Current.Value.Count);
//			CustomSkinnedMesh partCSM = new CustomSkinnedMesh ();
//			partCSM.name = jointVertexIndexEnum.Current.Key;
//			partCSM.boneMatrices.AddRange(pi.csms [0].boneMatrices);
//			List<int> vertexIndices = jointVertexIndexEnum.Current.Value;
//
//			int newTriIndex = 0;
//
//			Dictionary<int, int> newVT = new Dictionary<int, int> ();
//
//			for (int i = 0; i < vertexIndices.Count; i++) 
//			{
//				int vertexIndex = vertexIndices [i];
//
//				List<int> vtIndices = pi.vertexToTriangle [vertexIndex];
//
//				for (int j = 0; j < vtIndices.Count; j++) 
//				{
//					int triangleIndex = vtIndices [j];
//					triangleIndex /= 3;
//					triangleIndex *= 3;
//
//					for (int k = 0; k < 3; k++) 
//					{
//						int vi = triangles [triangleIndex + k];
//
//						if (!newVT.ContainsKey (vi)) 
//						{
//							newVT.Add (vi, newTriIndex);
//
//							partCSM.vertices.Add(vertices [vi]);
//							partCSM.normals.Add(normals [vi]);
//							partCSM.uv.Add(uv [vi]);
//							partCSM.boneWeights.Add(boneWeights [vi]);
//							partCSM.triangles.Add (newTriIndex);
//							newTriIndex++;
//
//							totalVertexCount++;
//						}
//						else 
//						{
//							partCSM.triangles.Add (newVT[vi]);
//						}
//
//						totalTriangleCount++;
//					}
//				}
//			}
//
//			newVT.Clear ();
//			newVT = null;
//
//			newCSMS.Add (partCSM);
//		}
//
//		pi.csms.Clear ();
//		pi.csms = newCSMS;
//
//		//		partitionMeshCount = newCSMS.Count;
//
//		Debug.Log (newCSMS.Count);
//
//		Debug.Log (totalVertexCount);
//		Debug.Log (totalTriangleCount / 3);
//	}


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

				Vector3 position = vertexMatrix.MultiplyPoint3x4 (keepVertices [j]);
				Vector3 normal = vertexMatrix.MultiplyVector (keepNormals [j]);

				worldVertices.Add(position);

				keepVertices [j] = position;
				keepNormals [j] = normal;
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

		int totalVertexCount = 0;
		int totalTriangleCount = 0;

		List<CustomSkinnedMesh> newCSMS = new List<CustomSkinnedMesh>();

		while (jointVertexIndexEnum.MoveNext ()) 
		{
			Debug.Log (jointVertexIndexEnum.Current.Key + " " + jointVertexIndexEnum.Current.Value.Count);
			CustomSkinnedMesh partCSM = new CustomSkinnedMesh ();
			partCSM.name = jointVertexIndexEnum.Current.Key;
			partCSM.boneMatrices.AddRange(pi.csms [0].boneMatrices);
			List<int> vertexIndices = jointVertexIndexEnum.Current.Value;

//			int newTriIndex = 0;

			Dictionary<int, int> newVT = new Dictionary<int, int> ();

			HashSet<Vector3> addtionalTri = new HashSet<Vector3> ();

			for (int i = 0; i < vertexIndices.Count; i++) 
			{
				int vertexIndex = vertexIndices [i];

				List<int> vtIndices = pi.vertexToTriangle [vertexIndex];

				for (int j = 0; j < vtIndices.Count; j++) 
				{
					int triangleIndex = vtIndices [j];
					triangleIndex /= 3;
					triangleIndex *= 3;

					Vector3 triVec = new Vector3 (triangles [triangleIndex + 0], triangles [triangleIndex + 1], triangles [triangleIndex + 2]);

					addtionalTri.Add (triVec);

//					for (int k = 0; k < 3; k++) 
//					{
//						int vi = triangles [triangleIndex + k];
//
//						if (!newVT.ContainsKey (vi)) 
//						{
//							newVT.Add (vi, newTriIndex);
//
//							partCSM.vertices.Add(vertices [vi]);
//							partCSM.normals.Add(normals [vi]);
//							partCSM.uv.Add(uv [vi]);
//							partCSM.boneWeights.Add(boneWeights [vi]);
//							partCSM.triangles.Add (newTriIndex);
//							newTriIndex++;
//
//							totalVertexCount++;
//						}
//						else 
//						{
//							partCSM.triangles.Add (newVT[vi]);
//						}
//
//						totalTriangleCount++;
//					}
				}
			}

//			HashSet<Vector3>.Enumerator hashEnum = addtionalTri.GetEnumerator ();
//
//			HashSet<Vector3> addVertexs = new HashSet<Vector3> ();
//
//			while (hashEnum.MoveNext ()) 
//			{
//				Vector3 vertIndex = hashEnum.Current;
//
//				int[] indexs = new int[3];
//				indexs[0] = (int)vertIndex.x;
//				indexs[1] = (int)vertIndex.y;
//				indexs[2] = (int)vertIndex.z;
//
//				for (int k = 0; k < 3; k++) 
//				{
//					int vi = indexs [k];
//
//					List<int> vtIndices = pi.vertexToTriangle [vi];
//
//					for (int i = 0; i < vtIndices.Count; i++) 
//					{
//						int nvi = vtIndices [i];
//						nvi /= 3;
//						nvi *= 3;
//
//						Vector3 nvp = new Vector3 (triangles [nvi + 0], triangles [nvi + 1], triangles [nvi + 2]);
//
//						if (!addtionalTri.Contains (nvp)) 
//						{
//							addVertexs.Add (nvp);
//						}
//					}
//				}
//			}
//
//			List<Vector3> newADV = new List<Vector3> ();
//
//			for (int m = 0; m < 3; m++) 
//			{
//				HashSet<Vector3>.Enumerator currentEnum = addVertexs.GetEnumerator ();
//
//				HashSet<Vector3> currentAddVertexs = new HashSet<Vector3> ();
//
//				while (currentEnum.MoveNext ()) 
//				{
//					Vector3 vertIndex = currentEnum.Current;
//
//					int[] indexs = new int[3];
//					indexs[0] = (int)vertIndex.x;
//					indexs[1] = (int)vertIndex.y;
//					indexs[2] = (int)vertIndex.z;
//
//					for (int k = 0; k < 3; k++) 
//					{
//						int vi = indexs [k];
//
//						List<int> vtIndices = pi.vertexToTriangle [vi];
//
//						for (int i = 0; i < vtIndices.Count; i++) 
//						{
//							int nvi = vtIndices [i];
//							nvi /= 3;
//							nvi *= 3;
//
//							Vector3 nvp = new Vector3 (triangles [nvi + 0], triangles [nvi + 1], triangles [nvi + 2]);
//
//							if (!addVertexs.Contains (nvp)) 
//							{
//								currentAddVertexs.Add (nvp);
//							}
//						}
//					}
//
//					if (!addtionalTri.Contains (vertIndex)) 
//					{
//						newADV.Add (vertIndex);
//					}
//				}
//
//				addVertexs.Clear ();
//				addVertexs = currentAddVertexs;
//			}
//
////			for (int i = 0; i < addVertexs.Count; i++) 
////			{
////				addtionalTri.Add (addVertexs [i]);
////			}
//
//			for (int i = 0; i < newADV.Count; i++) 
//			{
//				addtionalTri.Add (newADV [i]);
//			}

			for (int m = 0; m < 0; m++) 
			{
				using (HashSet<Vector3>.Enumerator hashEnum = addtionalTri.GetEnumerator ()) 
				{
					List<Vector3> addVertexs = new List<Vector3> ();

					while (hashEnum.MoveNext ()) 
					{
						Vector3 vertIndex = hashEnum.Current;

						int[] indexs = new int[3];
						indexs[0] = (int)vertIndex.x;
						indexs[1] = (int)vertIndex.y;
						indexs[2] = (int)vertIndex.z;

						for (int k = 0; k < 3; k++) 
						{
							int vi = indexs [k];

							List<int> vtIndices = pi.vertexToTriangle [vi];

							for (int i = 0; i < vtIndices.Count; i++) 
							{
								int nvi = vtIndices [i];
								nvi /= 3;
								nvi *= 3;

								Vector3 nvp = new Vector3 (triangles [nvi + 0], triangles [nvi + 1], triangles [nvi + 2]);

								if (!addtionalTri.Contains (nvp)) 
								{
									addVertexs.Add (nvp);
								}
							}
						}
					}

					for (int i = 0; i < addVertexs.Count; i++) 
					{
						addtionalTri.Add (addVertexs [i]);
					}
				}
			}

			using (HashSet<Vector3>.Enumerator addMeshEnum = addtionalTri.GetEnumerator ()) 
			{
				int newTriIndex = 0;

				while (addMeshEnum.MoveNext ()) 
				{
					Vector3 vertIndex = addMeshEnum.Current;

					int[] indexs = new int[3];
					indexs[0] = (int)vertIndex.x;
					indexs[1] = (int)vertIndex.y;
					indexs[2] = (int)vertIndex.z;

					for (int k = 0; k < 3; k++) 
					{
						int vi = indexs [k];

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
					}
				}
			}

			addtionalTri.Clear ();
			addtionalTri = null;


//			HashSet<Vector3>.Enumerator hashEnum = addtionalTri.GetEnumerator ();
//
//			while (hashEnum.MoveNext ()) 
//			{
//				Vector3 vertIndex = hashEnum.Current;
//				int[] indexs = new int[3];
//				indexs[0] = (int)vertIndex.x;
//				indexs[1] = (int)vertIndex.y;
//				indexs[2] = (int)vertIndex.z;
//
//				for (int k = 0; k < 3; k++) 
//				{
//					int vi = indexs [k];
//
//					if (!newVT.ContainsKey (vi)) 
//					{
//						newVT.Add (vi, newTriIndex);
//
//						partCSM.vertices.Add(vertices [vi]);
//						partCSM.normals.Add(normals [vi]);
//						partCSM.uv.Add(uv [vi]);
//						partCSM.boneWeights.Add(boneWeights [vi]);
//						partCSM.triangles.Add (newTriIndex);
//						newTriIndex++;
//
//						totalVertexCount++;
//					}
//					else 
//					{
//						partCSM.triangles.Add (newVT[vi]);
//					}
//
//					totalTriangleCount++;
//				}
//			}

//			Dictionary<int, int>.Enumerator newVTEnum = newVT.GetEnumerator ();
//
//			List<Vector3> addPosVec = new List<Vector3> ();
//			List<Vector3> addNorVec = new List<Vector3> ();
//
//			HashSet<Vector3> addtionalTri = new HashSet<Vector3> ();
//
//			int addCount = 0;
//
//			while (newVTEnum.MoveNext ()) 
//			{
//				int vertexIndex = newVTEnum.Current.Key;
//
//				List<int> vtIndices = pi.vertexToTriangle [vertexIndex];
//
//				for (int j = 0; j < vtIndices.Count; j++) 
//				{
//					int triangleIndex = vtIndices [j];
//					triangleIndex /= 3;
//					triangleIndex *= 3;
//
//					bool isCheck = false;
//
//					for (int k = 0; k < 3; k++) 
//					{
//						int vi = triangles [triangleIndex + k];
//
//						if (!newVT.ContainsKey (vi)) 
//						{
//							isCheck = true;
//							break;
//						}
//					}
//
//					if (isCheck) 
//					{
//						addCount++;
//						addtionalTri.Add (new Vector3 (triangleIndex + 0, triangleIndex + 1, triangleIndex + 2));
//
//						//						addPosVec.Add(vertices[triangles[triangleIndex + 0]]);
//						//						addPosVec.Add(vertices[triangles[triangleIndex + 1]]);
//						//						addPosVec.Add(vertices[triangles[triangleIndex + 2]]);
//						//
//						//						addNorVec.Add(vertices[triangles[triangleIndex + 0]]);
//						//						addNorVec.Add(vertices[triangles[triangleIndex + 1]]);
//						//						addNorVec.Add(vertices[triangles[triangleIndex + 2]]);
//					}
//				}
//			}
//
//			Debug.Log (partCSM.name + " add count = " + addCount + " " + addtionalTri.Count);

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
////			}

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

		csm.name = smr.name;

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
		PartitionInformation pi = new PartitionInformation();

		foreach (SkinnedMeshRenderer smr in sourceObj.GetComponentsInChildren<SkinnedMeshRenderer> ()) 
		{
			CustomSkinnedMesh csm = generateCSM (smr);
			pi.csms.Add (csm);
		}

		Transform[] bones = sourceObj.GetComponentInChildren<SkinnedMeshRenderer> ().bones;
		pi.mat = sourceObj.GetComponentInChildren<SkinnedMeshRenderer> ().material;

		for (int i = 0; i < bones.Length; i++) 
		{
			pi.indexToName.Add (i, bones [i].name);
			pi.nameToIndex.Add (bones [i].name, i);
			pi.jointPosition.Add (bones[i].name, bones[i].position);
		}

//		pi.jointPosition.Remove ("Hips");
//
//		pi.jointPosition.Remove ("Neck");
		pi.jointPosition.Remove ("LeftShoulder");
		pi.jointPosition.Remove ("RightShoulder");

		Vector3 spine2 = pi.jointPosition ["Neck"] + pi.jointPosition ["Spine1"];
		spine2 /= 2.0f;
		spine2 += pi.jointPosition ["Neck"];
		spine2 /= 2.0f;
//		spine2.z += (pi.jointPosition ["Neck"].z - pi.jointPosition ["Head"].z) * 0.75f;
		spine2.z += (pi.jointPosition ["Neck"].z - pi.jointPosition ["Head"].z) * 1.5f;
		Vector3 keepSpine2 = spine2;
		spine2 += pi.jointPosition ["Neck"];
		spine2 /= 2.0f;
		Vector3 keepSpine3 = spine2;
		spine2 += keepSpine2;
		spine2 /= 2.0f;
//		spine2 += keepSpine3;
//		spine2 /= 2.0f;
		pi.jointPosition.Add ("Spine2", spine2);

//		Vector3 spine2 = pi.jointPosition ["Neck"] + pi.jointPosition ["Spine1"];
//		spine2 /= 2.0f;
//		spine2 += pi.jointPosition ["Spine1"];
//		spine2 /= 2.0f;
//		spine2 += pi.jointPosition ["Neck"];
//		spine2 /= 2.0f;
//		pi.jointPosition.Add ("Spine2", spine2);

		Vector3 LeftUpperLeg = pi.jointPosition ["LeftUpLeg"] + pi.jointPosition ["LeftLeg"];
		LeftUpperLeg /= 2.0f;

		Vector3 LeftLowerLeg = pi.jointPosition ["LeftLeg"] + pi.jointPosition ["LeftFoot"];
		LeftLowerLeg /= 2.0f;

		pi.jointPosition.Add ("LeftUpperLeg", LeftUpperLeg);
		pi.jointPosition.Add ("LeftLowerLeg", LeftLowerLeg);

		Vector3 rightUpperLeg = pi.jointPosition ["RightUpLeg"] + pi.jointPosition ["RightLeg"];
		rightUpperLeg /= 2.0f;

		Vector3 rightLowerLeg = pi.jointPosition ["RightLeg"] + pi.jointPosition ["RightFoot"];
		rightLowerLeg /= 2.0f;

		pi.jointPosition.Add ("RightUpperLeg", rightUpperLeg);
		pi.jointPosition.Add ("RightLowerLeg", rightLowerLeg);

		pi.jointPosition.Remove ("RightUpLeg");
		pi.jointPosition.Remove ("LeftUpLeg");

//		Vector3 centerUpperLeg = pi.jointPosition ["LeftUpperLeg"] + pi.jointPosition ["RightUpperLeg"];
//		centerUpperLeg /= 2.0f;
//
//		pi.jointPosition.Add ("CenterUpperLeg", centerUpperLeg);

//		Vector3 neck = pi.jointPosition ["Neck"];
//		Vector3 leftShoulder = pi.jointPosition ["LeftShoulder"];
//		Vector3 rightShoulder = pi.jointPosition ["RightShoulder"];
//
//		leftShoulder.z = rightShoulder.z = neck.z;
//
//		pi.jointPosition ["LeftShoulder"] = leftShoulder;
//		pi.jointPosition ["RightShoulder"] = rightShoulder;

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
		Vector3 leftUpperArm = pi.jointPosition ["LeftArm"] + pi.jointPosition ["LeftForeArm"];
		leftUpperArm /= 2.0f;
		leftUpperArm += pi.jointPosition ["LeftForeArm"];
		leftUpperArm /= 2.0f;
//		leftUpperArm += pi.jointPosition ["LeftForeArm"];
//		leftUpperArm /= 2.0f;
//		Vector3 keepLeftUpperArm = leftUpperArm;
//		leftUpperArm += pi.jointPosition ["LeftForeArm"];
//		leftUpperArm /= 2.0f;
//		leftUpperArm += keepLeftUpperArm;
//		leftUpperArm /= 2.0f;
//		leftUpperArm += keepLeftUpperArm;
//		leftUpperArm /= 2.0f;

		Vector3 leftLowerArm = pi.jointPosition ["LeftForeArm"] + pi.jointPosition ["LeftHand"];
		leftLowerArm /= 2.0f;

		pi.jointPosition.Add ("LeftUpperArm", leftUpperArm);
		pi.jointPosition.Add ("LeftLowerArm", leftLowerArm);

		Vector3 rightUpperArm = pi.jointPosition ["RightArm"] + pi.jointPosition ["RightForeArm"];
		rightUpperArm /= 2.0f;
		rightUpperArm += pi.jointPosition ["RightForeArm"];
		rightUpperArm /= 2.0f;
//		rightUpperArm += pi.jointPosition ["RightForeArm"];
//		rightUpperArm /= 2.0f;
//		Vector3 keepRightUpperArm = rightUpperArm;
//		rightUpperArm += pi.jointPosition ["RightForeArm"];
//		rightUpperArm /= 2.0f;
//		rightUpperArm += keepRightUpperArm;
//		rightUpperArm /= 2.0f;
//		rightUpperArm += keepRightUpperArm;
//		rightUpperArm /= 2.0f;

		Vector3 rightLowerArm = pi.jointPosition ["RightForeArm"] + pi.jointPosition ["RightHand"];
		rightLowerArm /= 2.0f;

		pi.jointPosition.Add ("RightUpperArm", rightUpperArm);
		pi.jointPosition.Add ("RightLowerArm", rightLowerArm);
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

	public Vector3 normalCalcFromJoint(Vector3 point, Vector3 joint1, Vector3 joint2)
	{
		Vector3 vertNor = point - joint1;
		vertNor.Normalize ();

		float aDis = Vector3.Distance (point, joint1);

		Vector3 jointVector = joint1 - joint2;
		jointVector.Normalize ();

		float vectorAngle = 180.0f - Vector3.Angle (-vertNor, -jointVector);

		Vector3 apointVec = Vector3.zero;
		Vector3 newPointVec = Vector3.zero;

		if (vectorAngle == 90.0f) 
		{
			float bDis = aDis / Mathf.Sin (90.0f / 180.0f * Mathf.PI) * Mathf.Sin ((90.0f - vectorAngle) / 180.0f * Mathf.PI);

			apointVec = joint1 - point;
			newPointVec = (joint1 + bDis * -jointVector.normalized) - point;

			newPointVec += apointVec;
		}
		else 
		{
			float bDis = aDis / Mathf.Sin (90.0f / 180.0f * Mathf.PI) * Mathf.Sin ((90.0f - vectorAngle) / 180.0f * Mathf.PI);

			apointVec = joint1 - point;
			newPointVec = (joint1 + bDis * -jointVector) - point;
		}

		newPointVec.Normalize ();

//		newPointVec *= 2.0f;

		return newPointVec;
	}
}
