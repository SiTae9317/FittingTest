using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomSkinnedMesh
{
	public string name = "";
	public List<Vector3> vertices = new List<Vector3> ();
	public List<Vector3> normals = new List<Vector3> ();
	public List<Vector3> calcNormals = new List<Vector3> ();
	public List<Vector2> uv = new List<Vector2> ();
	public List<int> triangles = new List<int> ();
	public List<BoneWeight> boneWeights = new List<BoneWeight> ();
	public List<Matrix4x4> boneMatrices = new List<Matrix4x4>();
}