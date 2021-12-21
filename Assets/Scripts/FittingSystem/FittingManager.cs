using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FittingManager : MonoBehaviour 
{
	public GameObject charObj;
	public GameObject clothObj;

	private PoseMatch pm;
	private MeshCombine mc;
	private MeshSplit ms;

	private Action resultFunc = null;

	// Use this for initialization
	void Start ()
	{
		fittingStart ();
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

		charObj.SetActive (true);
		clothObj.SetActive (true);

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
		pm.startPoseMatch (poseMatchEnd);
	}

	void poseMatchEnd()
	{
		Destroy (pm);
		mc.startMP (meshCombineEnd);
	}

	void meshCombineEnd()
	{
		Destroy (mc);

		Vector3[] keepEuler = new Vector3[6];

		foreach (Transform ts in charObj.GetComponentsInChildren<Transform>()) 
		{
			if (ts.name.Contains ("Shoulder")) 
			{
				int index = 0;

				if (ts.name.Contains ("Left")) 
				{
					index = 0;
				}
				else if (ts.name.Contains ("Right")) 
				{
					index = 1;
				}

				keepEuler [0 + index] = ts.transform.localEulerAngles;

				ts.transform.localEulerAngles = Vector3.zero;
			}
			else if (ts.name.Contains ("tArm")) 
			{
				int index = 0;

				if (ts.name.Contains ("Left")) 
				{
					index = 0;
				}
				else if (ts.name.Contains ("Right")) 
				{
					index = 1;
				}

				keepEuler [2 + index] = ts.transform.localEulerAngles;

				ts.transform.localEulerAngles = Vector3.zero;
			}
			else if (ts.name.Contains ("ForeArm")) 
			{
				int index = 0;

				if (ts.name.Contains ("Left")) 
				{
					index = 0;
				}
				else if (ts.name.Contains ("Right")) 
				{
					index = 1;
				}

				keepEuler [4 + index] = ts.transform.localEulerAngles;

				ts.transform.localEulerAngles = Vector3.zero;
			}
		}

		ms.construct (charObj);

		foreach (Transform ts in charObj.GetComponentsInChildren<Transform>()) 
		{
			if (ts.name.Contains ("Shoulder")) 
			{
				if (ts.name.Contains ("Left")) 
				{
					ts.transform.localEulerAngles = keepEuler [0];
				}
				else if (ts.name.Contains ("Right")) 
				{
					ts.transform.localEulerAngles = keepEuler [1];
				}
			}
			else if (ts.name.Contains ("tArm")) 
			{
				if (ts.name.Contains ("Left")) 
				{
					ts.transform.localEulerAngles = keepEuler [2];
				}
				else if (ts.name.Contains ("Right")) 
				{
					ts.transform.localEulerAngles = keepEuler [3];
				}
			}
			else if (ts.name.Contains ("ForeArm")) 
			{
				if (ts.name.Contains ("Left")) 
				{
					ts.transform.localEulerAngles = keepEuler [4];
				}
				else if (ts.name.Contains ("Right")) 
				{
					ts.transform.localEulerAngles = keepEuler [5];
				}
			}
		}

		ms.startMeshSplit (meshSplitEnd);
	}

	void meshSplitEnd()
	{
		Destroy (ms);

		if (resultFunc != null) 
		{
			resultFunc ();
		}

		Debug.Log ("end");
	}
}
