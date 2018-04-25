using System;
using System.Runtime.Serialization;
using UnityEngine;

[Serializable]
public class Curve : Track {
	public const string KEY_RADIUS = "RADIUS";
	public const float MIN_RADIUS = 1f;
	public const float FINENESS_DISTANCE = 5f;

	private float _radius = MIN_RADIUS;
	public float radius {
		get {
			return _radius;
		}
		set {
			if (value > 0)
				_radius = Mathf.Max (MIN_RADIUS, value);
			else
				_radius = Mathf.Min (-MIN_RADIUS, value);
		}
	}

	public Curve (Map map, Vector3 pos) : base (map, pos) {
	}

	public Curve (Map map, Vector3 pos, Quaternion rot) : base (map, pos, rot) {
	}

	protected Curve (SerializationInfo info, StreamingContext context) : base (info, context) {
		_radius = info.GetSingle (KEY_RADIUS);
	}

	public override void GetObjectData (SerializationInfo info, StreamingContext context) {
		base.GetObjectData (info, context);
		info.AddValue (KEY_RADIUS, _radius);
	}

	public override void generate () {
		if (entity == null)
			(entity = new GameObject ("curve").AddComponent<MapEntity> ()).init (this);
		else
			reloadEntity ();
	}

	public override void reloadLineRendererPositions (LineRenderer renderer) {
		int l = Mathf.CeilToInt (_length / FINENESS_DISTANCE);
		Vector3[] p = new Vector3[l + 1];
		p[0] = pos;
		for (int a = 1; a <= l; a++) {
			p[a] = getPoint ((float)a / (float)l);
		}
		renderer.positionCount = p.Length;
		renderer.SetPositions (p);
	}

	public override Vector3 getPoint (float a) {
		return pos + rot * (Vector3.right + Vector3.left * Mathf.Cos (_length * a / Mathf.Abs (_radius))) * _radius + 
			rot * Vector3.forward * Mathf.Sin (_length * a / Mathf.Abs (_radius)) * Mathf.Abs (_radius);
	}

	public virtual Quaternion getRotation (float a) {
		return rot * Quaternion.Euler (new Vector3 (0, _length * a * Mathf.Rad2Deg / _radius));
	}
}
