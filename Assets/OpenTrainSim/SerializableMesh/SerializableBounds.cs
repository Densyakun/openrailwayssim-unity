using System;
using UnityEngine;

[Serializable]
public class SerializableBounds {
	public SerializableVector3 center;
	public SerializableVector3 size;

	public SerializableBounds (Bounds bounds) {
		center = new SerializableVector3 (bounds.center);
		size = new SerializableVector3 (bounds.size);
	}

	public Bounds toBounds () {
		return new Bounds (center.toVector3 (), size.toVector3 ());
	}
}
