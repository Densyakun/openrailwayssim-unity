using System;
using UnityEngine;

[Serializable]
public class SerializableQuaternion {
	public float x;
	public float y;
	public float z;
	public float w;

	public SerializableQuaternion (Quaternion r) {
		x = r.x;
		y = r.y;
		z = r.z;
		w = r.w;
	}

	public Quaternion toQuaternion () {
		return new Quaternion (x, y, z, w);
	}
}
