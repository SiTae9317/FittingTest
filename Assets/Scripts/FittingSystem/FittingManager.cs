using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FittingManager : MonoBehaviour 
{
	public GameObject charObj;
	public GameObject clothObj;
    public GameObject shoesObj;

    public bool isMan = true;

    public GameObject hair;
    public GameObject head;

    private BlendRestore br;
	private PoseMatch pm;
	private MeshCombine mc;
	private MeshSplit ms;

	private Action resultFunc = null;

	// Use this for initialization
	void Start ()
	{
//		fittingStart ();
	}
	
	// Update is called once per frame
	void Update () 
	{
//		if (Input.GetKeyDown (KeyCode.L)) 
//		{
//			fittingStart ();
//		}	
	}

	public void fittingStart(Action endCallback = null)
	{
		resultFunc = endCallback;

        //charObj.SetActive(true);
        //clothObj.SetActive(true);

        HairVolume hv = gameObject.AddComponent<HairVolume>();

        foreach(SkinnedMeshRenderer smr in charObj.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            if(smr.name.Contains("hair"))
            {
                Debug.Log("hair " + smr.name);
                hv.hair = smr;
            }
            else if(smr.name.Contains("mesh"))
            {
                Debug.Log("head " + smr.name);
                hv.head = smr;

                foreach(Transform trs in smr.bones)
                {
                    if(trs.name.Equals("Head"))
                    {
                        hv.headObj = trs.gameObject;
                    }
                    else if(trs.name.Equals("Neck"))
                    {
                        hv.neckObj = trs.gameObject;
                    }
                }
            }
        }

        if(hv.hair != null)
        {
            hv.startHairVol();
        }

        br = gameObject.AddComponent<BlendRestore>();
        br.head = charObj.transform.Find("mesh").gameObject;
        br.isMan = isMan;
        br.initialize();

        pm = gameObject.AddComponent<PoseMatch> ();
		pm.charObj = charObj;
		pm.clothObj = clothObj;

		mc = gameObject.AddComponent<MeshCombine> ();
		mc.charObj = charObj;
		mc.clothObj = clothObj;

		ms = gameObject.AddComponent<MeshSplit> ();
		ms.charObj = charObj;
		ms.clothObj = clothObj;

		mc.construct ();		

		pm.targetCount = -1;
        pm.startPoseMatch(poseMatchEnd);
    }

    public void fittingStart2(Action endCallback = null)
    {
        resultFunc = endCallback;

        charObj.SetActive(true);
        clothObj.SetActive(true);

        pm = gameObject.AddComponent<PoseMatch>();
        pm.charObj = charObj;
        pm.clothObj = clothObj;

        mc = gameObject.AddComponent<MeshCombine>();
        mc.charObj = charObj;
        mc.clothObj = clothObj;

        ms = gameObject.AddComponent<MeshSplit>();
        ms.charObj = charObj;
        ms.clothObj = clothObj;

        mc.construct();

        pm.targetCount = -1;
        pm.startPoseMatch3(poseMatchEnd);
    }

    void poseMatchEnd()
	{
		Destroy (pm);
		mc.startMP (meshCombineEnd);
	}

	void meshCombineEnd()
	{
        //Vector3[] keepEuler = new Vector3[6];

        //foreach (Transform ts in charObj.GetComponentsInChildren<Transform>()) 
        //{
        //	if (ts.name.Contains ("Shoulder")) 
        //	{
        //		int index = 0;

        //		if (ts.name.Contains ("Left")) 
        //		{
        //			index = 0;
        //		}
        //		else if (ts.name.Contains ("Right")) 
        //		{
        //			index = 1;
        //		}

        //		keepEuler [0 + index] = ts.transform.localEulerAngles;

        //		ts.transform.localEulerAngles = Vector3.zero;
        //	}
        //	else if (ts.name.Contains ("tArm")) 
        //	{
        //		int index = 0;

        //		if (ts.name.Contains ("Left")) 
        //		{
        //			index = 0;
        //		}
        //		else if (ts.name.Contains ("Right")) 
        //		{
        //			index = 1;
        //		}

        //		keepEuler [2 + index] = ts.transform.localEulerAngles;

        //		ts.transform.localEulerAngles = Vector3.zero;
        //	}
        //	else if (ts.name.Contains ("ForeArm")) 
        //	{
        //		int index = 0;

        //		if (ts.name.Contains ("Left")) 
        //		{
        //			index = 0;
        //		}
        //		else if (ts.name.Contains ("Right")) 
        //		{
        //			index = 1;
        //		}

        //		keepEuler [4 + index] = ts.transform.localEulerAngles;

        //		ts.transform.localEulerAngles = Vector3.zero;
        //	}
        //}

        //string tPose = System.IO.File.ReadAllText("C:\\RigData\\TPoseData.txt");
        TextAsset ta = (TextAsset)Resources.Load("TPoseData");
        string tPose = ta.text;

        string[] tPoseDatas = tPose.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        Dictionary<string, Vector3> jointToEular = new Dictionary<string, Vector3>();

        for(int i = 0; i < tPoseDatas.Length; i++)
        {
            string[] lineData = tPoseDatas[i].Split(' ');
            jointToEular.Add(lineData[0], new Vector3(float.Parse(lineData[1]), float.Parse(lineData[2]), float.Parse(lineData[3])));
        }


        foreach (Transform ts in charObj.GetComponentsInChildren<Transform>())
        {
            if(jointToEular.ContainsKey(ts.name))
            {
                ts.localEulerAngles = jointToEular[ts.name];
            }
        }

        ms.construct (charObj);

        //foreach (Transform ts in charObj.GetComponentsInChildren<Transform>()) 
        //{
        //	if (ts.name.Contains ("Shoulder")) 
        //	{
        //		if (ts.name.Contains ("Left")) 
        //		{
        //			ts.transform.localEulerAngles = keepEuler [0];
        //		}
        //		else if (ts.name.Contains ("Right")) 
        //		{
        //			ts.transform.localEulerAngles = keepEuler [1];
        //		}
        //	}
        //	else if (ts.name.Contains ("tArm")) 
        //	{
        //		if (ts.name.Contains ("Left")) 
        //		{
        //			ts.transform.localEulerAngles = keepEuler [2];
        //		}
        //		else if (ts.name.Contains ("Right")) 
        //		{
        //			ts.transform.localEulerAngles = keepEuler [3];
        //		}
        //	}
        //	else if (ts.name.Contains ("ForeArm")) 
        //	{
        //		if (ts.name.Contains ("Left")) 
        //		{
        //			ts.transform.localEulerAngles = keepEuler [4];
        //		}
        //		else if (ts.name.Contains ("Right")) 
        //		{
        //			ts.transform.localEulerAngles = keepEuler [5];
        //		}
        //	}
        //}

        foreach (Transform ts in charObj.GetComponentsInChildren<Transform>())
        {
            ts.localEulerAngles = Vector3.zero;
        }

        ms.startMeshSplit (meshSplitEnd);
	}

	void meshSplitEnd()
	{
		Destroy (ms);

        charObj = mc.charObj;
        clothObj = mc.clothObj;
        hair = mc.hair;
        Destroy(mc);

        MeshDataCombine mdc = charObj.GetComponent<MeshDataCombine>();
        mdc.groupCombine();

        head = mdc.head;
        br.head = mdc.head;

        Destroy(mdc);

        br.blendCopy();

        Destroy(br);

        Debug.Log ("end");

        ShoesFit sf = gameObject.AddComponent<ShoesFit>();
        sf.charObj = clothObj.transform.GetChild(0).gameObject;
        sf.shoes = shoesObj.transform.GetChild(0).gameObject;

        sf.setShoes();
        sf.genMesh();

        Destroy(sf);

        if (resultFunc != null)
        {
            resultFunc();
        }
    }
}
