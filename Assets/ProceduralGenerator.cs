using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralGenerator : MonoBehaviour
{
	public List<ShapeGenerator> generators;
	MeshFilter mf;
	IcosphereGenerator sphereGenerator;
	Mesh mesh;

	private void Awake()
	{
		mf = GetComponent<MeshFilter>();
		mesh = mf.sharedMesh;
		sphereGenerator = GetComponent<IcosphereGenerator>();
	}

	private void Generate()
	{
		if (!gameObject.activeInHierarchy) return;

		Vector3[] vertices = sphereGenerator.vertices.ToArray();
		Vector3[] heights = vertices.Clone() as Vector3[];

		foreach (var generator in generators)
		{
			Vector3[] current = vertices.Clone() as Vector3[];
			generator.Generate(ref current);

			for (int i = 0; i < vertices.Length; i++)
			{
				heights[i] += current[i];
			}
		}

		int[] tris = mf.sharedMesh.triangles;

		mf.sharedMesh.Clear();

		mf.sharedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
		mf.sharedMesh.SetVertices(heights);
		mf.sharedMesh.SetTriangles(tris, 0, true);

		mf.sharedMesh.RecalculateNormals();
	}
}
