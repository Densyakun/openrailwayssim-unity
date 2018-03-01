using System;
using UnityEngine;

[Serializable]
public class SerializableVector4 {
	public float x;
	public float y;
	public float z;
	public float w;

	public SerializableVector4 (Vector4 v) {
		x = v.x;
		y = v.y;
		z = v.z;
		w = v.w;
	}

	public Vector4 toVector4 () {
		return new Vector4 (x, y, z, w);
	}
}
