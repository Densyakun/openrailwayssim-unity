using System;
using System.Runtime.Serialization;
using UnityEngine;

[Serializable]
public class Curve : Track {
	public const string KEY_RADIUS = "RADIUS";
	public const float MIN_RADIUS = 1f;

	private float _radius = MIN_RADIUS;
	public float radius { get { return _radius; } set { _radius = Mathf.Max (MIN_RADIUS, value); } }

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

	public override void reloadEntity () {
		if (entity == null)
			return;

		LineRenderer renderer = entity.GetComponent<LineRenderer> ();
		if (renderer == null)
			renderer = entity.gameObject.AddComponent<LineRenderer> ();
		renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		renderer.receiveShadows = false;
		renderer.endWidth = renderer.startWidth = 1f;
		renderer.endColor = renderer.startColor = Color.white;
		renderer.material = Main.main.line_mat;
		//TODO 細かさを指定
		renderer.SetPositions (new Vector3[]{ pos, pos + (rot * Vector3.right + rot * Vector3.left * Mathf.Cos(_length / _radius) + rot * Vector3.forward * Mathf.Sin(_length / _radius)) * _radius });

		/*BoxCollider collider = entity.GetComponent<BoxCollider> ();
		if (collider == null)
			collider = entity.gameObject.AddComponent<BoxCollider> ();
		collider.isTrigger = false;*/

		base.reloadEntity ();
	}
}
