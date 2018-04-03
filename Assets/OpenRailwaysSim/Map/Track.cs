using System;
using System.Collections;
using System.Runtime.Serialization;
using UnityEngine;

[Serializable]
public class Track : ISerializable {
	public const string KEY_POS = "POS";
	public const string KEY_ROTATION = "ROT";

	public TrackEntity entity { get; protected set; }

	[NonSerialized]
	private Map _map;
	public Map map {
		get {
			return _map;
		}
		set {
			if (_map == null)
				_map = value;
		}
	}

	[NonSerialized]
	private Vector3 _pos;
	public Vector3 pos {
		get {
			return _pos;
		}
		set {
			_pos = value;
			if (entity != null)
				entity.transform.position = pos;
		}
	}

	[NonSerialized]
	private Quaternion _rot;
	public Quaternion rot {
		get {
			return _rot;
		}
		set {
			_rot = value;
			if (entity != null)
				entity.transform.rotation = rot;
		}
	}

	[NonSerialized]
	private float _length = 0;
	public float length {
		get {
			return _length;
		}
		set {
			this._length = value;
			reloadEntity ();
		}
	}

	public Track (Map map) : this (map, new Vector3 (), new Quaternion ()) {
	}

	public Track (Map map, Vector3 pos) : this (map, pos, new Quaternion ()) {
	}

	public Track (Map map, Vector3 pos, Quaternion rot) {
		_map = map;
		_pos = pos;
		_rot = rot;
		(entity = new GameObject ("track").AddComponent<TrackEntity> ()).init (this);
	}

	protected Track (SerializationInfo info, StreamingContext context) {
		if (info == null)
			throw new ArgumentNullException ("info");
		_pos = ((SerializableVector3)info.GetValue (KEY_POS, typeof(SerializableVector3))).toVector3 ();
		_rot = ((SerializableQuaternion)info.GetValue (KEY_ROTATION, typeof(SerializableQuaternion))).toQuaternion ();
	}

	public void GetObjectData (SerializationInfo info, StreamingContext context) {
		if (info == null)
			throw new ArgumentNullException ("info");
		info.AddValue (KEY_POS, new SerializableVector3 (_pos));
		info.AddValue (KEY_ROTATION, new SerializableQuaternion (_rot));
	}

	public void reloadEntity () {
		if (entity == null)
			return;
		entity.transform.position = pos;
		entity.transform.rotation = rot;

		LineRenderer renderer = entity.GetComponent<LineRenderer> ();
		BoxCollider collider = entity.GetComponent<BoxCollider> ();
		if (renderer == null)
			renderer = entity.gameObject.AddComponent<LineRenderer> ();
		if (collider == null)
			collider = entity.gameObject.AddComponent<BoxCollider> ();

		collider.isTrigger = false;

		renderer.SetPositions (new Vector3[]{ pos, pos + rot * Vector3.forward * length });
	}
}
