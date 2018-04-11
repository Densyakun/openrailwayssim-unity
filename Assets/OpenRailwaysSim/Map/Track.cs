using System;
using System.Collections;
using System.Runtime.Serialization;
using UnityEngine;

[Serializable]
public class Track : MapObject {
	public const string KEY_LENGTH = "LENGTH";
	public const float MIN_TRACK_LENGTH = 0.01f;

	private float _length = MIN_TRACK_LENGTH;
	public float length { get { return _length; } set { _length = Mathf.Max (MIN_TRACK_LENGTH, value); } }

	public Track (Map map, Vector3 pos) : base (map, pos) {
	}

	public Track (Map map, Vector3 pos, Quaternion rot) : base (map, pos, rot) {
	}

	protected Track (SerializationInfo info, StreamingContext context) : base (info, context) {
		_length = info.GetSingle (KEY_LENGTH);
	}

	public override void GetObjectData (SerializationInfo info, StreamingContext context) {
		base.GetObjectData (info, context);
		info.AddValue (KEY_LENGTH, _length);
	}

	public override void generate () {
		if (entity == null)
			(entity = new GameObject ("track").AddComponent<MapEntity> ()).init (this);
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
		renderer.endWidth = renderer.startWidth = 0.1f;
		renderer.endColor = renderer.startColor = Color.red;
		renderer.material = Main.main.line_mat;
		renderer.SetPositions (new Vector3[]{ pos, pos + rot * Vector3.forward * _length });

		/*BoxCollider collider = entity.GetComponent<BoxCollider> ();
		if (collider == null)
			collider = entity.gameObject.AddComponent<BoxCollider> ();
		collider.isTrigger = false;*/

		base.reloadEntity ();
	}
}
