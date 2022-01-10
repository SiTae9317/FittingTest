using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

//[RequireComponent(typeof(MeshPartition2))]
public class PoseMatch : MonoBehaviour 
{
	public bool isDebug = true;
    public bool isCheckMode = false;

	public GameObject charObj;
	public GameObject clothObj;

	public bool position = false;

	private List<Transform> tfL = new List<Transform>();

	public int count = -1;
	public int targetCount = 0;
	private int totalCount = -1;

	private bool auto = false;

	public string nowNode;
	public string childNode;
	public string isRot;
	public string isMove;
	public Vector3 xzVec;
	public Vector3 xyVec;
	public int pivot0;
	public int pivot1;
	public int pivot2;
	public int pivot3;

//	private int stopIndex = 38;

	private List<string> nodeNames = new List<string>();

	private Dictionary<string, JointInformation> jointPivot;

	public Vector3 normal0;
	public Vector3 normal1;
	public Vector3 normal2;
	public Vector3 normal3;

//	private MeshPartition2 nmp;

	// Use this for initialization
	void Start () 
	{
        //		charObj.SetActive (true);
        //		clothObj.SetActive (true);
        //
        //		nmp = GetComponent<MeshPartition2> ();
        //		nmp.charObj = charObj;
        //		nmp.clothObj = clothObj;
        //
        //		nmp.construct ();
        //
        //		if (targetCount < 0) 
        //		{
        //			startPoseMatch();
        //		}
        //		else 
        //		{
        //			startPoseMatch2();
        //		}

        /////////////////////////////////
        /// test
        if (isCheckMode)
        {
            importData();

            //importBinaryData ();

            charObj.transform.position = Vector3.zero;
            clothObj.transform.position = Vector3.zero;

            setRigNode(findNodeToName(charObj, "Pelvis"));

            foreach (Transform oriTf in charObj.GetComponentsInChildren<Transform>())
            {
                tfL.Add(oriTf);
            }

            for (int i = 0; i < tfL.Count; i++)
            {
                setAllRotation(i, position);
            }

            count = 0;

            for (; count < targetCount; count += 2)
            {
                excuteMatchRig();
            }
        }
    }

	public void startPoseMatch(System.Action endCallback)
	{
        //importData();
        //exportBinaryData();

        importBinaryData ();

        charObj.transform.position = Vector3.zero;
		clothObj.transform.position = Vector3.zero;

		setRigNode (findNodeToName(charObj, "Pelvis"));

		foreach (Transform oriTf in charObj.GetComponentsInChildren<Transform>()) 
		{
			tfL.Add (oriTf);
		}

		for (int i = 0; i < tfL.Count; i++) 
		{
			setAllRotation (i, position);
		}

		count = 0;

        for (; count < nodeNames.Count; count += 2)
        {
            excuteMatchRig();
        }

        endCallback ();
    }

    public void startPoseMatch3(System.Action endCallback)
    {
        charObj.transform.position = Vector3.zero;
        clothObj.transform.position = Vector3.zero;

        setRigNode2(findNodeToName(charObj, "skeleton"), findNodeToName(clothObj, "skeleton"));

        endCallback();
    }

    void setRigNode2(GameObject charParentObj, GameObject clothParentObj)
    {
        int childCount = charParentObj.transform.childCount;

        for (int i = 0; i < childCount; i++)
        {
            Transform clothPt = clothParentObj.transform.GetChild(i);
            Transform charPt = charParentObj.transform.GetChild(i);

            if(clothPt.name == charPt.name)
            {
                clothPt.localScale = charPt.localScale;
                clothPt.localEulerAngles = charPt.localEulerAngles;
                clothPt.localPosition = charPt.localPosition;

                setRigNode2(charPt.gameObject, clothPt.gameObject);
            }
        }
    }

    public void startPoseMatch2()
	{
		importData ();

		charObj.transform.position = Vector3.zero;
		clothObj.transform.position = Vector3.zero;

		setRigNode (findNodeToName(charObj, "Pelvis"));

		foreach (Transform oriTf in charObj.GetComponentsInChildren<Transform>()) 
		{
			tfL.Add (oriTf);
		}

		for (int i = 0; i < tfL.Count; i++) 
		{
			setAllRotation (i, position);
		}

		count = 0;
		totalCount = tfL.Count;

		for (; count < targetCount; count += 2) 
		{
			excuteMatchRig ();
		}
	}

	void importData()
	{
		jointPivot = new Dictionary<string, JointInformation> ();

		string rigDatas = File.ReadAllText ("C://RigData//newRigData.txt");

		string[] rigInfos = rigDatas.Split (new char[]{'\r', '\n'},System.StringSplitOptions.RemoveEmptyEntries);

		for (int i = 0; i < rigInfos.Length; i++) 
		{
			string[] rigInfo = rigInfos [i].Split (' ');

			JointInformation ji = new JointInformation ();

			ji.yzVector = new Vector3 (float.Parse (rigInfo [1]), float.Parse (rigInfo [2]), float.Parse (rigInfo [3]));
			ji.yxVector = new Vector3 (float.Parse (rigInfo [4]), float.Parse (rigInfo [5]), float.Parse (rigInfo [6]));

			ji.isRotationJoint = int.Parse(rigInfo [7]) == 0 ? false : true;

			ji.isPositionJoint = int.Parse(rigInfo [8]) == 0 ? false : true;

			ji.childCount = int.Parse (rigInfo [9]);

			ji.index = int.Parse (rigInfo [10]);

			ji.pivot0 = int.Parse (rigInfo [11]);

			ji.pivot1 = int.Parse (rigInfo [12]);

			ji.pivot2 = int.Parse (rigInfo [13]);

			ji.pivot3 = int.Parse (rigInfo [14]);

			jointPivot.Add (rigInfo [0], ji);
		}
	}	

	void importBinaryData()
	{
		jointPivot = new Dictionary<string, JointInformation> ();

		TextAsset ta = Resources.Load ("newRigData") as TextAsset;

		using (MemoryStream ms = new MemoryStream (ta.bytes)) 
		{
			using (BinaryReader br = new BinaryReader (ms)) 
			{
				while (br.BaseStream.Position < br.BaseStream.Length) 
				{
					JointInformation ji = new JointInformation ();

					string name = br.ReadString ();

					ji.yzVector = new Vector3 (br.ReadSingle (), br.ReadSingle (), br.ReadSingle ());
					ji.yxVector = new Vector3 (br.ReadSingle (), br.ReadSingle (), br.ReadSingle ());

					ji.isRotationJoint = br.ReadInt32 () == 0 ? false : true;

					ji.isPositionJoint = br.ReadInt32 () == 0 ? false : true;

					ji.childCount = br.ReadInt32 ();

					ji.index = br.ReadInt32 ();

					ji.pivot0 = br.ReadInt32 ();

					ji.pivot1 = br.ReadInt32 ();

					ji.pivot2 = br.ReadInt32 ();

					ji.pivot3 = br.ReadInt32 ();

					jointPivot.Add (name, ji);
				}
				br.Close ();
			}
			ms.Close ();
		}
	}	

	void exportData()
	{
		List<string> rigNames = new List<string> ();

		GameObject go = findNodeToName (charObj, "Pelvis");

		rigNames.Add (go.name);

		addNameData (go, rigNames);

		string outputData = "";

		for (int i = 0; i < rigNames.Count; i++) 
		{
			outputData += rigNames [i];
			outputData += " ";
			outputData += (int)(jointPivot [rigNames [i]].yzVector.x);
			outputData += " ";
			outputData += (int)(jointPivot [rigNames [i]].yzVector.y);
			outputData += " ";
			outputData += (int)(jointPivot [rigNames [i]].yzVector.z);
			outputData += " ";
			outputData += (int)(jointPivot [rigNames [i]].yxVector.x);
			outputData += " ";
			outputData += (int)(jointPivot [rigNames [i]].yxVector.y);
			outputData += " ";
			outputData += (int)(jointPivot [rigNames [i]].yxVector.z);
			outputData += " ";
			outputData += (int)(jointPivot [rigNames [i]].isRotationJoint ? 1 : 0);
			outputData += " ";
			outputData += (int)(jointPivot [rigNames [i]].isPositionJoint ? 1 : 0);
			outputData += " ";
			outputData += (int)(jointPivot [rigNames [i]].childCount);
			outputData += " ";
			outputData += (int)(jointPivot [rigNames [i]].index);
			outputData += " ";
			outputData += (int)(jointPivot [rigNames [i]].pivot0);
			outputData += " ";
			outputData += (int)(jointPivot [rigNames [i]].pivot1);
			outputData += " ";
			outputData += (int)(jointPivot [rigNames [i]].pivot2);
			outputData += " ";
			outputData += (int)(jointPivot [rigNames [i]].pivot3);
			outputData += "\r\n";
		}

		File.WriteAllText ("C://RigData//newRigData2.txt", outputData);
	}

	void exportBinaryData()
	{
		List<string> rigNames = new List<string> ();

		GameObject go = findNodeToName (charObj, "Pelvis");

		rigNames.Add (go.name);

		addNameData (go, rigNames);

		using (FileStream fs = new FileStream ("C://RigData//newRigData.bytes", FileMode.OpenOrCreate, FileAccess.Write)) 
		{
			using(BinaryWriter bw = new BinaryWriter(fs))
			{
				for (int i = 0; i < rigNames.Count; i++) 
				{
					bw.Write (rigNames [i]);

					bw.Write (jointPivot [rigNames [i]].yzVector.x);
					bw.Write (jointPivot [rigNames [i]].yzVector.y);
					bw.Write (jointPivot [rigNames [i]].yzVector.z);

					bw.Write (jointPivot [rigNames [i]].yxVector.x);
					bw.Write (jointPivot [rigNames [i]].yxVector.y);
					bw.Write (jointPivot [rigNames [i]].yxVector.z);

					bw.Write (jointPivot [rigNames [i]].isRotationJoint ? 1 : 0);
					bw.Write (jointPivot [rigNames [i]].isPositionJoint ? 1 : 0);
					bw.Write (jointPivot [rigNames [i]].childCount);
					bw.Write (jointPivot [rigNames [i]].index);
					bw.Write (jointPivot [rigNames [i]].pivot0);
					bw.Write (jointPivot [rigNames [i]].pivot1);
					bw.Write (jointPivot [rigNames [i]].pivot2);
					bw.Write (jointPivot [rigNames [i]].pivot3);
				}
				bw.Close();
			}
			fs.Close();
		}
	}

	void addNameData(GameObject go, List<string> rigNames)
	{
		int childCount = go.transform.childCount;

		if (childCount == 0) 
		{
			return ;
		}

		for (int i = 0; i < childCount; i++) 
		{
			rigNames.Add(go.transform.GetChild (i).name);
		}

		for (int i = 0; i < childCount; i++) 
		{
			addNameData(go.transform.GetChild (i).gameObject, rigNames);
		}
	}

	// Update is called once per frame
	void Update () 
	{
//		excuteMatchRig ();

		if (isDebug && count < nodeNames.Count) 
		{
			nowNode = nodeNames [count];
			childNode = nodeNames [count + 1];
			isRot = jointPivot [childNode].isRotationJoint.ToString();
			isMove = jointPivot [nowNode].isPositionJoint.ToString();
			xzVec = jointPivot [nowNode].yzVector;
			xyVec = jointPivot [nowNode].yxVector;
			pivot0 = jointPivot [nowNode].pivot0;
			pivot1 = jointPivot [nowNode].pivot1;
			pivot2 = jointPivot [nowNode].pivot2;
			pivot3 = jointPivot [nowNode].pivot3;

			GameObject charRoot = findNodeToName (charObj, nodeNames[count]);
			GameObject clothRoot = findNodeToName (clothObj, nodeNames[count]);
			GameObject charChildRoot = findNodeToName (charObj, nodeNames[count + 1]);
			GameObject clothChildRoot = findNodeToName (clothObj, nodeNames[count + 1]);

			normal0 = charRoot.transform.position;
			normal1 = charChildRoot.transform.position;
			normal2 = clothRoot.transform.position;
			normal3 = clothChildRoot.transform.position;

			if (Input.GetKeyDown (KeyCode.UpArrow)) 
			{
				if (isDebug) 
				{
					Debug.Log (nodeNames [count] + " -> " + nodeNames [count + 1]);
				}

				charRoot = findNodeToName (charObj, nodeNames[count]);
				clothRoot = findNodeToName (clothObj, nodeNames[count]);
				charChildRoot = findNodeToName (charObj, nodeNames[count + 1]);
				clothChildRoot = findNodeToName (clothObj, nodeNames[count + 1]);

				matchingRig (charRoot, charChildRoot, clothRoot, clothChildRoot);

				matchingRig (charRoot, charChildRoot, clothRoot, clothChildRoot);
			}

			if (Input.GetKeyDown (KeyCode.DownArrow)) 
			{
				if (isDebug) 
				{
					Debug.Log (nodeNames [count] + " -> " + nodeNames [count + 1]);
				}

				charRoot = findNodeToName (charObj, nodeNames[count]);
				clothRoot = findNodeToName (clothObj, nodeNames[count]);
				charChildRoot = findNodeToName (charObj, nodeNames[count + 1]);
				clothChildRoot = findNodeToName (clothObj, nodeNames[count + 1]);

				mathcingPosition (charRoot, charChildRoot, clothRoot, clothChildRoot);
			}
		}

//		normal0 = charRoot.transform.position;
//		normal1 = getNormalVector (charRoot, charChildRoot);
//		normal2 = clothRoot.transform.position;
//		normal3 = getNormalVector (clothRoot, clothChildRoot);
//
//		Debug.DrawRay (normal0, normal1, Color.green);
//		Debug.DrawRay (normal2, normal3, Color.yellow);

		Debug.DrawLine (normal0, normal1, Color.green);
		Debug.DrawLine (normal2, normal3, Color.yellow);

		if (Input.GetKeyDown (KeyCode.W)) 
		{
			excuteMatchRig ();


			count += 2;
		}
		if (Input.GetKeyDown (KeyCode.E)) 
		{
			Debug.Log ("auto start");
			auto = true;
		}

		if (auto) 
		{
			if (count >= totalCount) 
			{
				count = 0;
				auto = false;
				Debug.Log ("auto end");
			}

			excuteMatchRig ();
			count += 2;
		}

		if (Input.GetKeyDown (KeyCode.RightArrow)) 
		{
			count += 2;
		}
	}

	GameObject findNodeToName(GameObject obj, string name)
	{
		foreach (Transform ts in obj.GetComponentsInChildren<Transform>()) 
		{
			if (ts.name.Equals (name)) 
			{
				return ts.gameObject;
			}
		}

		return null;
	}

	GameObject findMeshNodeToName(GameObject obj, string name)
	{
		foreach (MeshFilter ts in obj.GetComponentsInChildren<MeshFilter>()) 
		{
			if (ts.name.Equals (name)) 
			{
				return ts.gameObject;
			}
		}

		return null;
	}

//	void excuteMatchRig ()
//	{
//		if (isDebug) 
//		{
//			Debug.Log (nodeNames [count] + " -> " + nodeNames [count + 1]);
//		}
//
//		GameObject charRoot = findNodeToName (charObj, nodeNames[count]);
//		GameObject clothRoot = findNodeToName (clothObj, nodeNames[count]);
//		GameObject charChildRoot = findNodeToName (charObj, nodeNames[count + 1]);
//		GameObject clothChildRoot = findNodeToName (clothObj, nodeNames[count + 1]);
//
//		if (charRoot.transform.childCount <= 1) 
//		{
//			matchingRig (charRoot, charChildRoot, clothRoot, clothChildRoot);
//		}
//		mathcingPosition (charRoot, charChildRoot, clothRoot, clothChildRoot);
//	}
//
//	void mathcingPosition(GameObject charRoot, GameObject charChildRoot, GameObject clothRoot, GameObject clothChildRoot)
//	{
//		Vector3 charNormalVec = Vector3.zero;
//		Vector3 clothNormalVec = Vector3.zero;
//
//		Vector3 afterCharNormalVec = Vector3.zero;
//		Vector3 afterClothNormalVec = Vector3.zero;
//
//		charNormalVec = getNormalVector (charRoot, charChildRoot);
//		clothNormalVec = getNormalVector (clothRoot, clothChildRoot);
//
//		float dis = Vector3.Distance (charChildRoot.transform.position, charRoot.transform.position);
//		clothChildRoot.transform.position = charRoot.transform.position + charNormalVec.normalized * dis;
//	}
//
//	void matchingRig(GameObject charRoot, GameObject charChildRoot, GameObject clothRoot, GameObject clothChildRoot)
//	{
//		Vector3 charNormalVec = Vector3.zero;
//		Vector3 clothNormalVec = Vector3.zero;
//
//		Vector3 afterCharNormalVec = Vector3.zero;
//		Vector3 afterClothNormalVec = Vector3.zero;
//
//		charNormalVec = getNormalVector (charRoot, charChildRoot);
//		clothNormalVec = getNormalVector (clothRoot, clothChildRoot);
//
//		normal0 = charRoot.transform.position;
//		normal1 = charNormalVec;
//		normal2 = clothRoot.transform.position;
//		normal3 = clothNormalVec;
//
//		string selfName = charRoot.name;
//		string childName = charChildRoot.name;
//
//		float cos = Vector3.Dot (charNormalVec.normalized, clothNormalVec.normalized);
//		//				Vector3 normalVec = Vector3.Cross (charNormalVec.normalized, clothNormalVec.normalized);
//		Vector3 normalVec = Vector3.Cross (clothNormalVec.normalized, charNormalVec.normalized);
//
//		float radian = Mathf.Acos(cos);
//
//		float angle = ((radian) * (180.0f / Mathf.PI));
//
//		Debug.Log (angle + " " + normalVec);
//
//		if (angle > Mathf.Epsilon) 
//		{
//			clothRoot.transform.Rotate (normalVec.normalized, angle, Space.World);
//		}
//	}

	void excuteMatchRig ()
	{
		if (isDebug) 
		{
			Debug.Log (nodeNames [count] + " -> " + nodeNames [count + 1]);
		}

		GameObject charRoot = findNodeToName (charObj, nodeNames[count]);
		GameObject clothRoot = findNodeToName (clothObj, nodeNames[count]);
		GameObject charChildRoot = findNodeToName (charObj, nodeNames[count + 1]);
		GameObject clothChildRoot = findNodeToName (clothObj, nodeNames[count + 1]);

		matchingRig (charRoot, charChildRoot, clothRoot, clothChildRoot);
		matchingRig (charRoot, charChildRoot, clothRoot, clothChildRoot);
		mathcingPosition (charRoot, charChildRoot, clothRoot, clothChildRoot);
	}

	void mathcingPosition(GameObject charRoot, GameObject charChildRoot, GameObject clothRoot, GameObject clothChildRoot)
	{
		Vector3 charNormalVec = Vector3.zero;
		Vector3 clothNormalVec = Vector3.zero;

		Vector3 afterCharNormalVec = Vector3.zero;
		Vector3 afterClothNormalVec = Vector3.zero;

		charNormalVec = getNormalVector (charRoot, charChildRoot);
		clothNormalVec = getNormalVector (clothRoot, clothChildRoot);

		if (jointPivot [charRoot.name].isPositionJoint) 
		{
			if (charNormalVec - clothNormalVec != Vector3.zero && jointPivot [charRoot.name].isRotationJoint) 
			{
				afterCharNormalVec = getNormalVector (charRoot, charChildRoot);
				afterClothNormalVec = getNormalVector (clothRoot, clothChildRoot);

				float charDis = Vector3.Distance (charRoot.transform.position, charChildRoot.transform.position);
//				float clothDis = Vector3.Distance (clothRoot.transform.position, clothChildRoot.transform.position);

				afterClothNormalVec = getNormalVector (clothRoot, charChildRoot);
				clothChildRoot.transform.position = clothRoot.transform.position + afterClothNormalVec * charDis;
			}
			else 
			{		
				clothChildRoot.transform.position = charChildRoot.transform.position;

				afterCharNormalVec = getNormalVector (charRoot, charChildRoot);
				afterClothNormalVec = getNormalVector (clothRoot, clothChildRoot);
			}
		}
	}

    float calcDiffAngleVal(float left, float right)
    {
        float returnVal = 0.0f;

        if (left < 0.0f && right > 0.0f)
        {
            Debug.Log("neg cloth : " + left);
            left = (180.0f + left);
            Debug.Log("neg cloth : " + left);
        }
        else if(left >0.0f && right < 0.0f)
        {
            Debug.Log("neg char : " + right);
            right = (180.0f + right);
            Debug.Log("neg char : " + right);
        }

        returnVal = left - right;

        return returnVal;
    }

	void matchingRig(GameObject charRoot, GameObject charChildRoot, GameObject clothRoot, GameObject clothChildRoot)
	{
		Vector3 charNormalVec = Vector3.zero;
		Vector3 clothNormalVec = Vector3.zero;

		Vector3 afterCharNormalVec = Vector3.zero;
		Vector3 afterClothNormalVec = Vector3.zero;

		charNormalVec = getNormalVector (charRoot, charChildRoot);
		clothNormalVec = getNormalVector (clothRoot, clothChildRoot);

		string selfName = charRoot.name;
		string childName = charChildRoot.name;

		if (isDebug) 
		{
			Debug.Log ("rotation = " + jointPivot [childName].isRotationJoint);
		}

		if (charNormalVec - clothNormalVec != Vector3.zero && jointPivot [childName].isRotationJoint) 
		{
			Vector3 charZyNormalVec = getNormalVector (charRoot, charChildRoot);
			Vector3 clothZyNormalVec = getNormalVector (clothRoot, clothChildRoot);

			Vector3 eurZyVal = clothRoot.transform.eulerAngles;

			List<float> charZyNormalVecList = new List<float> ();
			charZyNormalVecList.Add (charZyNormalVec.x);
			charZyNormalVecList.Add (charZyNormalVec.y);
			charZyNormalVecList.Add (charZyNormalVec.z);

			List<float> clothZyNormalVecList = new List<float> ();
			clothZyNormalVecList.Add (clothZyNormalVec.x);
			clothZyNormalVecList.Add (clothZyNormalVec.y);
			clothZyNormalVecList.Add (clothZyNormalVec.z);

			//jointPivot[selfName].pivot0
			float charYZAngle = gradientToAngle (charZyNormalVecList[jointPivot[selfName].pivot0], charZyNormalVecList[jointPivot[selfName].pivot1]);
			float clothYZAngle = gradientToAngle (clothZyNormalVecList[jointPivot[selfName].pivot0], clothZyNormalVecList[jointPivot[selfName].pivot1]);

            //float diffYZAngle = clothYZAngle - charYZAngle;
            float diffYZAngle = calcDiffAngleVal(clothYZAngle, charYZAngle);

            if (isDebug) 
			{
				Debug.Log ("value = " + calcAngleToEuler (diffYZAngle, jointPivot [selfName].yzVector));
			}
			eurZyVal += calcAngleToEuler (diffYZAngle, jointPivot [selfName].yzVector);

			clothRoot.transform.eulerAngles = eurZyVal;

			charZyNormalVecList.Clear ();
			clothZyNormalVecList.Clear ();

			Vector3 charXyNormalVec = getNormalVector (charRoot, charChildRoot);
			Vector3 clothXyNormalVec = getNormalVector (clothRoot, clothChildRoot);

			Vector3 eurXyVal = clothRoot.transform.eulerAngles;

			List<float> charXyNormalVecList = new List<float> ();
			charXyNormalVecList.Add (charXyNormalVec.x);
			charXyNormalVecList.Add (charXyNormalVec.y);
			charXyNormalVecList.Add (charXyNormalVec.z);

			List<float> clothXyNormalVecList = new List<float> ();
			clothXyNormalVecList.Add (clothXyNormalVec.x);
			clothXyNormalVecList.Add (clothXyNormalVec.y);
			clothXyNormalVecList.Add (clothXyNormalVec.z);

			float charXYAngle = gradientToAngle (charXyNormalVecList[jointPivot[selfName].pivot2], charXyNormalVecList[jointPivot[selfName].pivot3]);
			float clothXYAngle = gradientToAngle (clothXyNormalVecList[jointPivot[selfName].pivot2], clothXyNormalVecList[jointPivot[selfName].pivot3]);

            //float diffXYAngle = clothXYAngle - charXYAngle;
            float diffXYAngle = calcDiffAngleVal(clothXYAngle, charXYAngle);

            if (isDebug) 
			{
				Debug.Log ("value = " + calcAngleToEuler (diffXYAngle, jointPivot [selfName].yxVector));
			}
			eurXyVal += calcAngleToEuler (diffXYAngle, jointPivot [selfName].yxVector);

			clothRoot.transform.eulerAngles = eurXyVal;
		}

		afterCharNormalVec = getNormalVector (charRoot, charChildRoot);
		afterClothNormalVec = getNormalVector (clothRoot, clothChildRoot);
	}

	Vector3 calcAngleToEuler(float angle, Vector3 standardVec)
	{
		if (isDebug) 
		{
			Debug.Log (standardVec * angle);
		}
		return standardVec * angle;
	}

    float gradientToAngle(float increaseX, float increaseY)
    {
        float angle = Mathf.Atan(increaseY / increaseX) / Mathf.PI * 180.0f;

        float pnValue = 0.0f;

        if (angle < 0.0f)
        {
            pnValue = -1.0f;
        }
        else
        {
            pnValue = 1.0f;
        }

        angle = 90.0f - (Mathf.Abs(angle));

        angle *= pnValue;

        if (isDebug)
        {
            Debug.Log(angle);
        }

        return angle;
    }

    Vector3 getNormalVector(GameObject startObj, GameObject endObj)
	{
		Vector3 returnValue = new Vector3 ();

		returnValue = endObj.transform.position - startObj.transform.position;

		returnValue.Normalize ();

		return returnValue;
	}

	void setAllRotation(int index, bool setPosition = false)
	{
		foreach (Transform tarTf in clothObj.GetComponentsInChildren<Transform>()) 
		{
			if (tfL [index].name.Equals (tarTf.name)) 
			{
				tarTf.localEulerAngles = tfL[index].localEulerAngles;

				if (setPosition) 
				{
					tarTf.localPosition = tfL[index].localPosition;
				}

				if (tfL [index].name.Equals ("Pelvis")) 
				{
					tarTf.position = tfL [index].position;
				}

				tarTf.localScale = tfL[index].localScale;
				break;
			}
		}
	}

	void setRigNode(GameObject parentObj)
	{
		int childCount = parentObj.transform.childCount;

		if (childCount == 0) 
		{
			return ;
		}

		for (int i = 0; i < childCount; i++) 
		{
			for (int j = 0; j < childCount; j++) 
			{
				if (jointPivot [parentObj.transform.GetChild (j).name].index == i) 
				{
//					Debug.Log (parentObj.transform.name);
//					Debug.Log (parentObj.transform.GetChild(j).name);
//					Debug.Log ("============================");
					nodeNames.Add (parentObj.transform.name);
					nodeNames.Add (parentObj.transform.GetChild (j).name);

					break;
				}
			}
		}

		for (int i = 0; i < childCount; i++) 
		{
			setRigNode (parentObj.transform.GetChild (i).gameObject);
		}
	}
}
