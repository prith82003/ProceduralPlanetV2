using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;

[RequireComponent(typeof(MeshFilter))]
public class IcosphereGenerator : MonoBehaviour
{
	Mesh sphereMesh;
	MeshFilter meshFilter;
	public List<Vector3> vertices;
	public List<int> triangles;
	[Range(0.0001f, float.MaxValue)]
	public float scale = 1;
	public int subdivisionLimit;
	public float testValue;
	public float offsetTest;
	static IcosphereGenerator instance;
	Dictionary<long, int> middlePointIndexCache;

	void OnValidate()
	{
		instance = this;
	}

	void Awake()
	{
		instance = this;
	}

	private void Start()
	{
		GenerateMesh();
	}

	public void GenerateMesh()
	{
		meshFilter = GetComponent<MeshFilter>();
		sphereMesh = new Mesh();
		middlePointIndexCache = new Dictionary<long, int>();
		meshFilter.sharedMesh = sphereMesh;

		CreateShape();
		Subdivide();
		UpdateMesh();
	}

	void CreateShape()
	{
		float phi = (1.0f + Mathf.Sqrt(5.0f)) * 0.5f; // Golden Ratio

		float a = 1.0f * scale;
		float b = 1.0f / phi * scale;

		vertices = new List<Vector3>();

		vertices.Add(new Vector3(0, b, -a));
		vertices.Add(new Vector3(b, a, 0));
		vertices.Add(new Vector3(-b, a, 0));
		vertices.Add(new Vector3(0, b, a));
		vertices.Add(new Vector3(0, -b, a));
		vertices.Add(new Vector3(-a, 0, b));
		vertices.Add(new Vector3(0, -b, -a));
		vertices.Add(new Vector3(a, 0, -b));
		vertices.Add(new Vector3(a, 0, b));
		vertices.Add(new Vector3(-a, 0, -b));
		vertices.Add(new Vector3(b, -a, 0));
		vertices.Add(new Vector3(-b, -a, 0));

		triangles = new List<int>()
		{
			2, 1, 0,
			1, 2, 3,
			5, 4, 3,
			4, 8, 3,
			7, 6, 0,
			6, 9, 0,
			11, 10, 4,
			10, 11, 6,
			9, 5, 2,
			5, 9, 11,
			8, 7, 1,
			7, 8, 10,
			2, 5, 3,
			8, 1, 3,
			9, 2, 0,
			1, 7, 0,
			11, 9, 6,
			7, 10, 6,
			5, 11, 4,
			10, 8, 4
		};
	}

	void UpdateMesh()
	{
		sphereMesh.Clear();

		sphereMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

		sphereMesh.SetVertices(vertices);
		sphereMesh.SetTriangles(triangles, 0);

		sphereMesh.RecalculateNormals();
	}

	void Subdivide()
	{
		for (int i = 0; i < subdivisionLimit; i++)
		{
			var newTriangles = new List<int>();

			for (int j = 0; j < triangles.Count; j += 3)
			{
				int v1 = triangles[j];
				int v2 = triangles[j + 1];
				int v3 = triangles[j + 2];

				int a = MiddlePoint(v1, v2);
				int b = MiddlePoint(v2, v3);
				int c = MiddlePoint(v3, v1);

				List<int> newTris = new List<int>()
				{
					v1, a, c,
					v2, b, a,
					v3, c, b,
					a, b, c
				};

				newTriangles.AddRange(newTris);
			}

			triangles = newTriangles;
		}
	}

	void AddVertex(Vector3 point)
	{
		vertices.Add(ProjectToUnitSphere(point));
	}

	int MiddlePoint(int p1, int p2)
	{
		bool firstIsSmaller = p1 < p2;
		long smallerIndex = firstIsSmaller ? p1 : p2;
		long greaterIndex = firstIsSmaller ? p2 : p1;

		long key = (smallerIndex << 32) + greaterIndex;

		int ret;

		if (middlePointIndexCache.TryGetValue(key, out ret))
			return ret;

		Vector3 point1 = vertices[p1];
		Vector3 point2 = vertices[p2];
		Vector3 middle = new Vector3(
			(point1.x + point2.x) / 2,
			(point1.y + point2.y) / 2,
			(point1.z + point2.z) / 2
		);

		vertices[p1] = ProjectToUnitSphere(point1);
		vertices[p2] = ProjectToUnitSphere(point2);
		AddVertex(middle);

		middlePointIndexCache.Add(key, vertices.Count - 1);

		return vertices.Count - 1;
	}

	Vector3 ProjectToUnitSphere(Vector3 point)
	{
		float length = point.magnitude;
		return new Vector3(point.x / length, point.y / length, point.z / length) * scale;
	}
}

[CustomEditor(typeof(IcosphereGenerator))]
public class SphereGeneratorInspector : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		IcosphereGenerator script = (IcosphereGenerator)target;

		if (GUILayout.Button("Generate Mesh"))
		{
			script.GenerateMesh();
		}
	}
}