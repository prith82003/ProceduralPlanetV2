using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ShapeSettings))]
public abstract class ShapeGenerator : ScriptableObject
{
	public ShapeSettings settings;

	public abstract void Generate(ref Vector3[] vertices);
	public virtual void GenerateSecondary(Vector3 position, float scale) { }
}
