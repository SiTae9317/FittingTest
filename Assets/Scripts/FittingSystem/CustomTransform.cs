using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomTransform
{
	public CustomTransform (Transform trs)
	{
		name = trs.name;

		localPosition = trs.localPosition;
		position = trs.position;

		localEulerAngles = trs.localEulerAngles;
		eulerAngles = trs.eulerAngles;

		localScale = trs.localScale;
		lossyScale = trs.lossyScale;

		localToWorldMatrix = trs.localToWorldMatrix;
		worldToLocalMatrix = trs.worldToLocalMatrix;
	}

	public Vector3 localPosition 
	{
		get 
		{
//			CustomTransform ct = this;
//
//			Matrix4x4 m = Matrix4x4.identity;
//
//			if (ct.parent != null) 
//			{
//				m = ct.parent.worldToLocalMatrix * ct.localToWorldMatrix;
//			}
//			else 
//			{
//				m = ct.localToWorldMatrix;
//			}
//
//			lp = m.GetColumn (3);

			return lp;
		}
		set 
		{
			lp = value;
//			localToWorldMatrix = localToWorldMatrix;
//			worldToLocalMatrix = worldToLocalMatrix;
		}
	}
	public Vector3 position 
	{
		get 
		{
//			CustomTransform ct = this;
//
//			Matrix4x4 m = Matrix4x4.identity;
//
//			if (ct.parent != null) 
//			{
//				m = ct.wtl * ct.parent.ltw;
//			}
//			else 
//			{
//				m = ct.wtl;
//			}
//
//			p = m.MultiplyPoint3x4(localPosition);

			return p;
		}
		set 
		{
			p = value;
//			localToWorldMatrix = localToWorldMatrix;
//			worldToLocalMatrix = worldToLocalMatrix;
		}
	}

	public Vector3 localEulerAngles
	{
		get 
		{
//			CustomTransform ct = this;
//
//			Matrix4x4 m = Matrix4x4.identity;
//
//			float x, y, z, w, scale; 
//
//			if (ct.parent != null) 
//			{
//				m = ct.parent.worldToLocalMatrix * ct.localToWorldMatrix;
//				scale = Mathf.Pow (m.determinant, 1.0f / 3.0f);
//			}
//			else 
//			{
//				m = ct.localToWorldMatrix;
//				scale = 1.0f;
//			}
//
//			w = (float) (Mathf.Sqrt(Mathf.Max(0, scale + m[0, 0] + m[1, 1] + m[2, 2])) / 2); 
//			x = (float) (Mathf.Sqrt(Mathf.Max(0, scale + m[0, 0] - m[1, 1] - m[2, 2])) / 2); 
//			y = (float) (Mathf.Sqrt(Mathf.Max(0, scale - m[0, 0] + m[1, 1] - m[2, 2])) / 2); 
//			z = (float) (Mathf.Sqrt(Mathf.Max(0, scale - m[0, 0] - m[1, 1] + m[2, 2])) / 2); 
//
//			if (m [2, 1] - m [1, 2] < 0) 
//			{
//				x = -x; 
//			}
//
//			if (m [0, 2] - m [2, 0] < 0) 
//			{
//				y = -y; 
//			}
//
//			if (m [1, 0] - m [0, 1] < 0) 
//			{
//				z = -z; 
//			}
//
//			Quaternion rotation = new Quaternion (x, y, z, w);
//
//			lea = rotation.eulerAngles;

			return lea;
		}
		set 
		{
			lea = value;
//			localToWorldMatrix = localToWorldMatrix;
//			worldToLocalMatrix = worldToLocalMatrix;
		}
	}
	public Vector3 eulerAngles
	{
		get 
		{
			return ea;
		}
		set 
		{
			ea = value;
//			localToWorldMatrix = localToWorldMatrix;
//			worldToLocalMatrix = worldToLocalMatrix;
		}
	}

	public Vector3 localScale
	{
		get 
		{
//			CustomTransform ct = this;
//
//			Matrix4x4 m = Matrix4x4.identity;
//
//			if (ct.parent != null) 
//			{
//				m = ct.parent.wtl * ct.ltw;
//			}
//			else 
//			{
//				m = ct.ltw;
//			}
//
//			ls.x = m.GetColumn (0).magnitude;
//			ls.y = m.GetColumn (1).magnitude;
//			ls.z = m.GetColumn (2).magnitude;

			return ls;
		}
		set 
		{
			ls = value;
//			localToWorldMatrix = localToWorldMatrix;
//			worldToLocalMatrix = worldToLocalMatrix;
		}
	}
	public Vector3 lossyScale
	{
		get 
		{
			return s;
		}
		set 
		{
			s = value;
//			localToWorldMatrix = localToWorldMatrix;
//			worldToLocalMatrix = worldToLocalMatrix;
		}
	}

	public Matrix4x4 localToWorldMatrix
	{
		get
		{
			CustomTransform ct = this;
			ltw = Matrix4x4.identity;

			List<Matrix4x4> keepMat = new List<Matrix4x4> ();

			while (ct != null) 
			{
				keepMat.Add (Matrix4x4.TRS (ct.lp, Quaternion.Euler (ct.lea), ct.ls));
				ct = ct.parent;
			}

			for (int i = keepMat.Count - 1; i >= 0; i--) 
			{
				ltw *= keepMat [i];
			}

			keepMat.Clear ();
			keepMat = null;

			return ltw;
		}
		set 
		{
			ltw = value;
		}
	}

	public Matrix4x4 worldToLocalMatrix
	{
		get
		{
			CustomTransform ct = this;
			wtl = Matrix4x4.identity;

			while (ct != null) 
			{
				wtl *= Matrix4x4.TRS (ct.lp, Quaternion.Euler (ct.lea), ct.ls).inverse;
				ct = ct.parent;
			}

			return wtl;
		}
		set 
		{
			wtl = value;
		}
	}

	public string name;

	public CustomTransform parent = null;

	private Vector3 lp = Vector3.zero;
	private Vector3 p = Vector3.zero;

	private Vector3 lea = Vector3.zero;
	private Vector3 ea = Vector3.zero;

	private Vector3 ls = Vector3.zero;
	private Vector3 s = Vector3.zero;

	private Matrix4x4 ltw = Matrix4x4.identity;
	private Matrix4x4 wtl = Matrix4x4.identity;
}
