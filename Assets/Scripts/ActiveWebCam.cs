using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ScreenSizeStatus
{
	FullScreenSize = 0,
	ThreeByFour
}

//[RequireComponent(typeof(GUITexture))]
public class ActiveWebCam : MonoBehaviour 
{
    //public GameObject avatarObj;

    public RectTransform rectTrs;
    public Text uiText = null;
	public Image displayUITexture = null;
    public Material webcamMat = null;

	private WebCamTexture wct = null;
	private Texture2D camTexture = null;
	private Texture2D newCamTexture = null;

	private int deviceMaxNum = 0;
	private int deviceNum = 1;
	private int camWidth;
	private int camHeight;

	private List<Color32> oriTex;
	private List<Color32> rotTex;

	private bool initSuccess = false;
	private bool isRunning = false;
	private bool captureStart = false;
	private bool captureStop = false;

	private bool withMeMode = false;

	private ScreenSizeStatus screenSizeState = ScreenSizeStatus.FullScreenSize;

    //private ScreenOrientation so = ScreenOrientation.Unknown;
    private DeviceOrientation deviceOri = DeviceOrientation.Unknown;
    //private DeviceOrientation nowDeviceRotation = DeviceOrientation.Portrait;

    /// <summary>
    /// local type
    /// </summary>

    private Vector3 screenSize;
	private Vector3 localEuler;

	// Use this for initialization
	void Start () 
	{
		///
		/// local init
		///

		screenSize = new Vector3();
		localEuler = new Vector3 ();
        //gameObject.transform.localPosition = new Vector3 (0.5f, 0.5f, 0.0f);

        //gameObject.transform.localScale = new Vector3 (1.0f, 1.0f, 1.0f);

        //deviceOri = DeviceRotate.ori;
        //so = Screen.orientation;

        deviceOri = DeviceOrientation.Portrait;

        startCapture ();
	}
	
	// Update is called once per frame
	void Update () 
	{
		/*
		if (wct != null && !wct.isPlaying)// && captureStart) 
		{
			wct.Play ();
		}
		*/

		//if (deviceOri != DeviceRotate.ori) 
		//{
		//	deviceOri = DeviceRotate.ori;

		//	StartCoroutine (rotateCapture(deviceOri));
		//}

		/*
		#if UNITY_EDITOR
		if (Input.GetKeyDown (KeyCode.Alpha1)) 
		{
			so = ScreenOrientation.Portrait;
			StartCoroutine (rotateCapture(so));
		}
		if (Input.GetKeyDown (KeyCode.Alpha2)) 
		{
			so = ScreenOrientation.LandscapeLeft;
			StartCoroutine (rotateCapture(so));
		}
		if (Input.GetKeyDown (KeyCode.Alpha3)) 
		{
			so = ScreenOrientation.LandscapeRight;
			StartCoroutine (rotateCapture(so));
		}
		#elif UNITY_ANDROID
		if (so != Screen.orientation) 
		{
		so = Screen.orientation;

		StartCoroutine (rotateCapture(so));
		}
		#endif
		*/

		/*
		if (nowDeviceRotation != DeviceRotate.ori) 
		{
			nowDeviceRotation = DeviceRotate.ori;
			StartCoroutine (rotateCapture(nowDeviceRotation));
		}
		*/

		//screenResizing ();
	}

	//public void setTargetDisplay(UITexture targetUITexture)
	//{
	//	displayUITexture = targetUITexture;
	//}

	public void setWithMeMode(bool isMode)
	{
		withMeMode = isMode;
	}

	public void screenResizing()
	{
		if (deviceOri == DeviceOrientation.Portrait) 
		{
			screenSize.x = camHeight / (float)Screen.width;
			screenSize.y = camWidth / (float)Screen.height;
			screenSize.z = 1.0f;

			if (screenSizeState == ScreenSizeStatus.FullScreenSize) 
			{
				Vector3 keepVector = gameObject.transform.localScale;
				keepVector.x = 1.0f;
				keepVector.y = 1.0f / (screenSize.y / screenSize.x);
				keepVector.z = 1.0f;

				gameObject.transform.localScale = keepVector;
			}
			else if (screenSizeState == ScreenSizeStatus.ThreeByFour) 
			{
				Vector3 keepVector = gameObject.transform.localScale;
				keepVector.x = screenSize.y / screenSize.x;
				keepVector.y = 1.0f;
				keepVector.z = 1.0f;

				gameObject.transform.localScale = keepVector;
			}

			localEuler.x = 0.0f;
			localEuler.y = 0.0f;
			localEuler.z = 0.0f;

			gameObject.transform.localEulerAngles = localEuler;
		}
		else if (deviceOri == DeviceOrientation.LandscapeLeft)
		{
			screenSize.x = camWidth / (float)Screen.height;
			screenSize.y = camHeight / (float)Screen.width;
			screenSize.z = 1.0f;

			if (screenSizeState == ScreenSizeStatus.FullScreenSize) 
			{
				Vector3 keepVector = gameObject.transform.localScale;
				keepVector.x = 1.0f / (screenSize.x / screenSize.y);
				keepVector.y = 1.0f;
				keepVector.z = 1.0f;

				gameObject.transform.localScale = keepVector;
			}
			else if (screenSizeState == ScreenSizeStatus.ThreeByFour) 
			{
				Vector3 keepVector = gameObject.transform.localScale;
				keepVector.x = 1.0f;
				keepVector.y = screenSize.x / screenSize.y;
				keepVector.z = 1.0f;

				gameObject.transform.localScale = keepVector;
			}

			localEuler.x = 0.0f;
			localEuler.y = 0.0f;
			localEuler.z = -90.0f;

			gameObject.transform.localEulerAngles = localEuler;
		}
		else if (deviceOri == DeviceOrientation.LandscapeRight)
		{
			screenSize.x = camWidth / (float)Screen.height;
			screenSize.y = camHeight / (float)Screen.width;
			screenSize.z = 1.0f;

			if (screenSizeState == ScreenSizeStatus.FullScreenSize) 
			{
				Vector3 keepVector = gameObject.transform.localScale;
				keepVector.x = 1.0f / (screenSize.x / screenSize.y);
				keepVector.y = 1.0f;
				keepVector.z = 1.0f;

				gameObject.transform.localScale = keepVector;
			}
			else if (screenSizeState == ScreenSizeStatus.ThreeByFour) 
			{
				Vector3 keepVector = gameObject.transform.localScale;
				keepVector.x = 1.0f;
				keepVector.y = screenSize.x / screenSize.y;
				keepVector.z = 1.0f;

				gameObject.transform.localScale = keepVector;
			}

			localEuler.x = 0.0f;
			localEuler.y = 0.0f;
			localEuler.z = 90.0f;

			gameObject.transform.localEulerAngles = localEuler;
		}
		//if (withMeMode && displayUITexture != null) 
		//{
		//	displayUITexture.gameObject.transform.localScale = gameObject.transform.localScale;
		//	displayUITexture.gameObject.transform.localEulerAngles = gameObject.transform.localEulerAngles;
		//}

		/*
		Vector3 screenSize = new Vector3 ((float)camTexture.width / (float)Screen.width, (float)camTexture.height / (float)Screen.height, 1.0f);
		if (so == ScreenOrientation.Portrait) 
		{
			gameObject.transform.localScale = new Vector3 (1.0f, screenSize.y / screenSize.x, 1.0f);
		}
		else 
		{
			gameObject.transform.localScale = new Vector3 (screenSize.x / screenSize.y, 1.0f, 1.0f);
		}
		*/
	}

	private void startCapture()
	{
		if ((deviceMaxNum = WebCamTexture.devices.Length) == 0) 
		{
			initSuccess = false;
		}
		else 
		{
			if (wct == null)
            {
                wct = new WebCamTexture(WebCamTexture.devices[WebCamTexture.devices.Length - 1].name, 640, 480 , 20);
                //wct = new WebCamTexture(WebCamTexture.devices[WebCamTexture.devices.Length - 1].name, 640, 480 , 20);

                wct.Play ();

				captureStart = true;

				camWidth = wct.width;
				camHeight = wct.height;

                uiText.text = camWidth + " " + camHeight;
                rectTrs.localScale = new Vector3(camHeight * 0.005f, camWidth * 0.005f, 1.0f);

                Debug.Log ("wct == null");
			}

			if (oriTex == null) 
			{
				oriTex = new List<Color32> ();
			}

			if (rotTex == null) 
			{
				rotTex = new List<Color32> ();
			}

			//StartCoroutine (rotateCapture(so));
			StartCoroutine (rotateCapture(deviceOri));
		}
	}

	/*
	void startCapture()
	{
		if ((deviceMaxNum = WebCamTexture.devices.Length) == 0) 
		{
			initSuccess = false;
			return ;
		}
		initSuccess = true;

		//#if UNITY_EDITOR
		//wct = new WebCamTexture(WebCamTexture.devices[0].name);

		//#else 
		//wct = new WebCamTexture(WebCamTexture.devices[1].name, 640, 480);
		//#endif

		wct = new WebCamTexture(WebCamTexture.devices[WebCamTexture.devices.Length - 1].name, 640, 480);

		wct.Play ();

		if (oriTex == null) 
		{
			oriTex = new List<Color32> ();
		}

		if (rotTex == null) 
		{
			rotTex = new List<Color32> ();
		}

		camWidth = wct.width;
		camHeight = wct.height;

		camTexture = new Texture2D (camHeight, camWidth);

		gameObject.GetComponent<GUITexture>().texture = camTexture;

		foreach (SkinnedMeshRenderer smr in avatarObj.GetComponentsInChildren<SkinnedMeshRenderer>()) 
		{
			smr.material.mainTexture = camTexture;
		}

		StartCoroutine ("rotateTexture");
	}

	void stopCapture()
	{
		initSuccess = false;

		wct.Stop ();
		wct = null;

		oriTex.Clear ();
		rotTex.Clear ();

		DestroyObject (camTexture);
		camTexture = null;
	}

	IEnumerator rotateTexture()
	{
		while (initSuccess) 
		{
			yield return null;

			oriTex.Clear ();
			rotTex.Clear ();

			oriTex.AddRange (wct.GetPixels32 ());

			if (DeviceRotate.ori == DeviceOrientation.Portrait) 
			{
				for (int x = 0; x < camWidth; x++) 
				{
					for (int y = 0; y < camHeight; y++) 
					{
						rotTex.Add (oriTex [y * camWidth + x]);
					}
				}
			}
			else if (DeviceRotate.ori == DeviceOrientation.LandscapeLeft) 
			{
				for (int x = camWidth - 1; x >= 0; x--) 
				{
					for (int y = 0; y < camHeight; y++) 
					{
						rotTex.Add (oriTex [y * camWidth + x]);
					}
				}
			}

			else if (DeviceRotate.ori == DeviceOrientation.LandscapeRight) 
			{
				for (int x = camWidth - 1; x >= 0; x--) 
				{
					for (int y = 0; y < camHeight; y++) 
					{
						rotTex.Add (oriTex [y * camWidth + x]);
					}
				}
			}

			camTexture.SetPixels32 (rotTex.ToArray ());
			camTexture.Apply ();
		}
	}
	*/

	/*
	public void webcamPlay ()
	{
		Debug.Log ("play");
		StartCoroutine(startCapture ());
	}

	public void webcamStop ()
	{
		captureStop = true;
		Debug.Log ("stop");
		StartCoroutine(stopCapture ());
	}

	public bool isCaptureStop()
	{
		return captureStop;
	}
	*/

	public Texture2D getWebcamTexture()
	{
		if (initSuccess) 
		{
			return camTexture;
		} 
		else 
		{
			return null;
		}
	}

	public GameObject getAvatarObj()
	{
		return null;//avatarObj;
	}

	/*
	IEnumerator rotateCapture(ScreenOrientation screenOri)
	{
		gameObject.GetComponent<GUITexture> ().texture = null;

		initSuccess = false;

		while(isRunning)
		{
			yield return null;
		}

		if (camTexture != null) 
		{
			DestroyObject (camTexture);
			camTexture = null;
		}

		if (screenOri == ScreenOrientation.Portrait) 
		{
			Debug.Log ("1");
			//camTexture = new Texture2D (camWidth, camHeight);
			camTexture = new Texture2D (camHeight, camWidth);

			initSuccess = true;

			StartCoroutine (portraitCamTexture());
		}
		else if (screenOri == ScreenOrientation.Landscape)
		{
			Debug.Log ("2");
			//camTexture = new Texture2D (camHeight, camWidth);
			camTexture = new Texture2D (camWidth, camHeight);

			initSuccess = true;

			StartCoroutine (lendscapeLeftCamTexture());
		}
		else if (screenOri == ScreenOrientation.LandscapeRight)
		{
			Debug.Log ("3");
			//camTexture = new Texture2D (camHeight, camWidth);
			camTexture = new Texture2D (camWidth, camHeight);

			initSuccess = true;

			StartCoroutine (lendscapeRightCamTexture());
		}

		gameObject.GetComponent<GUITexture>().texture = camTexture;
	}
	*/

	/*
	IEnumerator stopCapture()
	{
		initSuccess = false;

		while(isRunning)
		{
			yield return null;
		}

		if (newCamTexture != null) 
		{
			DestroyObject (newCamTexture);
			newCamTexture = null;
		}

		oriTex.Clear ();
		rotTex.Clear ();

		//wct.Stop ();
		//wct = null;

		//DestroyObject (camTexture);
		//camTexture = null;

		captureStop = false;
	}
	*/

	public void camPause()
	{
		initSuccess = false;
	}

	public void camStart()
	{
		StartCoroutine (rotateCapture(DeviceOrientation.Portrait));
	}

	IEnumerator rotateCapture(DeviceOrientation screenOri)
	{
        Debug.Log(screenOri);
		//gameObject.GetComponent<GUITexture> ().texture = null;

		//gameObject.GetComponent<UITexture> ().mainTexture = null;

		initSuccess = false;

		while(isRunning)
		{
			yield return null;
        }
        Debug.Log("0");

        if (newCamTexture != null) 
		{
			DestroyObject (newCamTexture);
			newCamTexture = null;
        }
        Debug.Log("0.0");

        if (screenOri == DeviceOrientation.Portrait) 
		{
			Debug.Log ("1");
			//camTexture = new Texture2D (camWidth, camHeight);
			newCamTexture = new Texture2D (camHeight, camWidth);

			initSuccess = true;

			StartCoroutine (portraitCamTexture());
		}
		else if (screenOri == DeviceOrientation.LandscapeLeft)
		{
			Debug.Log ("2");
			//camTexture = new Texture2D (camHeight, camWidth);
			newCamTexture = new Texture2D (camWidth, camHeight);

			initSuccess = true;

			StartCoroutine (lendscapeLeftCamTexture());
		}
		else if (screenOri == DeviceOrientation.LandscapeRight)
		{
			Debug.Log ("3");
			//camTexture = new Texture2D (camHeight, camWidth);
			newCamTexture = new Texture2D (camWidth, camHeight);

			initSuccess = true;

			StartCoroutine (lendscapeRightCamTexture());
		}
        else
        {
            Debug.Log("4");
        }

		//gameObject.GetComponent<GUITexture>().texture = camTexture;
	}

	IEnumerator portraitCamTexture()
	{
		bool isFirst = false;

		isRunning = true;

		while (initSuccess) 
		{
			yield return null;

			oriTex.Clear ();
			rotTex.Clear ();

			if (wct == null) 
			{
				break;
			}

            if (!wct.didUpdateThisFrame) 
			{
				continue;
            }

            oriTex.AddRange (wct.GetPixels32 ());

			for (int x = 0; x < camWidth; x++) 
			{
				for (int y = 0; y < camHeight; y++) 
				{
					rotTex.Add (oriTex [y * camWidth + x]);
				}
			}

			if (!isFirst)
            {
                isFirst = true;

				newCamTexture.SetPixels32 (rotTex.ToArray ());
				newCamTexture.Apply ();

				screenResizing ();

				if (camTexture != null) 
				{
					DestroyObject (camTexture);
					camTexture = null;
				}

				camTexture = new Texture2D (newCamTexture.width, newCamTexture.height);
				camTexture.SetPixels32 (newCamTexture.GetPixels32 ());
				camTexture.Apply ();

                webcamMat.mainTexture = camTexture;

                displayUITexture.material = webcamMat;

                displayUITexture.enabled = true;

                //if (withMeMode) 
                //{
                //	displayUITexture.material.mainTexture = camTexture;
                //}
                //gameObject.GetComponent<UITexture> ().mainTexture = camTexture;
                //gameObject.GetComponent<GUITexture> ().texture = camTexture;
            }
			else
            {
                camTexture.SetPixels32 (rotTex.ToArray ());
				camTexture.Apply ();
                //displayUITexture.material.mainTexture = camTexture;
            }
        }

		isRunning = false;
	}

	IEnumerator lendscapeLeftCamTexture()
	{
		bool isFirst = false;

		isRunning = true;

		while (initSuccess) 
		{
			yield return null;

			oriTex.Clear ();
			rotTex.Clear ();

			if (!wct.didUpdateThisFrame) 
			{
				continue;
			}

			oriTex.AddRange (wct.GetPixels32 ());

			for (int y = 0; y < camHeight; y++) 
			{
				for (int x = camWidth - 1; x >= 0; x--) 
				{
					rotTex.Add (oriTex [y * camWidth + x]);
				}
			}

			if (!isFirst) 
			{
				isFirst = true;

				newCamTexture.SetPixels32 (rotTex.ToArray ());
				newCamTexture.Apply ();

				screenResizing ();

				if (camTexture != null) 
				{
					DestroyObject (camTexture);
					camTexture = null;
				}

				camTexture = new Texture2D (newCamTexture.width, newCamTexture.height);
				camTexture.SetPixels32 (newCamTexture.GetPixels32 ());
				camTexture.Apply ();

				if (withMeMode) 
				{
					displayUITexture.material.mainTexture = camTexture;
				}
				//gameObject.GetComponent<UITexture> ().mainTexture = camTexture;
				//gameObject.GetComponent<GUITexture> ().texture = camTexture;
			}
			else 
			{
				camTexture.SetPixels32 (rotTex.ToArray ());
				camTexture.Apply ();
			}
		}

		isRunning = false;
	}

	IEnumerator lendscapeRightCamTexture()
	{
		bool isFirst = false;

		isRunning = true;

		while (initSuccess) 
		{
			yield return null;

			oriTex.Clear ();
			rotTex.Clear ();

			if (!wct.didUpdateThisFrame) 
			{
				continue;
			}

			oriTex = new List<Color32> ();
			rotTex = new List<Color32> ();

			oriTex.AddRange (wct.GetPixels32 ());

			for (int y = camHeight - 1; y >= 0; y--) 
			{
				for (int x = 0; x < camWidth; x++) 
				{
					rotTex.Add (oriTex [y * camWidth + x]);
				}
			}

			if (!isFirst) 
			{
				isFirst = true;

				newCamTexture.SetPixels32 (rotTex.ToArray ());
				newCamTexture.Apply ();

				screenResizing ();

				if (camTexture != null) 
				{
					DestroyObject (camTexture);
					camTexture = null;
				}

				camTexture = new Texture2D (newCamTexture.width, newCamTexture.height);
				camTexture.SetPixels32 (newCamTexture.GetPixels32 ());
				camTexture.Apply ();

				if (withMeMode) 
				{
					displayUITexture.material.mainTexture = camTexture;
				}
				//gameObject.GetComponent<UITexture> ().mainTexture = camTexture;
				//gameObject.GetComponent<GUITexture> ().texture = camTexture;
			}
			else 
			{
				camTexture.SetPixels32 (rotTex.ToArray ());
				camTexture.Apply ();
			}
		}

		isRunning = false;
	}

	public ScreenSizeStatus getScreenStatus ()
	{
		return screenSizeState;
	}

	public void changeScreenSizeStatus(int sizeState = -1)
	{
		int keepInt = -1;

		if (sizeState < 0) 
		{
			screenSizeState++;
			keepInt = (int)screenSizeState;
		}
		else 
		{
			keepInt = sizeState;
		}

		keepInt %= 2;
		screenSizeState = (ScreenSizeStatus)keepInt;

		screenResizing ();
	}
}
