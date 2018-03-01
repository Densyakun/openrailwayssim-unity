using System;
using UnityEngine;

[Serializable]
public class SerializableVector2 {
	public float x;
	public float y;

	public SerializableVector2 (Vector2 v) {
		x = v.x;
		y = v.y;
	}

	public Vector2 toVector2 () {
		return new Vector2 (x, y);
	}
}
