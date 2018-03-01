using System;
using System.Collections;
using System.Runtime.Serialization;
using UnityEngine;

[Serializable]
public class MapObject : ISerializable {
	public const string KEY_POS = "POS";
	public const string KEY_ROTATION = "ROT";

	public virtual MapEntity entity { get; protected set; }

	[NonSerialized]
	private Map _map;
	public Map map {
		get { return _map; }
		set {
			if (_map == null)
				_map = value;
		}
	}
	public Vector3 pos { get; protected set; }
	public Quaternion rot { get; protected set; }

	public MapObject (Map map) : this (map, new Vector3 (), new Quaternion ()) {
	}

	public MapObject (Map map, Vector3 pos) : this (map, pos, new Quaternion ()) {
	}
	
	public MapObject (Map map, Vector3 pos, Quaternion rot) {
		this.map = map;
		this.pos = pos;
		this.rot = rot;
	}

	protected MapObject (SerializationInfo info, StreamingContext context) {
		if (info == null)
			throw new ArgumentNullException ("info");
		pos = ((SerializableVector3)info.GetValue (KEY_POS, typeof(SerializableVector3))).toVector3 ();
		rot = ((SerializableQuaternion)info.GetValue (KEY_ROTATION, typeof(SerializableQuaternion))).toQuaternion ();
	}

	public virtual void GetObjectData (SerializationInfo info, StreamingContext context) {
		if (info == null)
			throw new ArgumentNullException ("info");
		SyncFromEntity ();
		info.AddValue (KEY_POS, new SerializableVector3 (pos));
		info.AddValue (KEY_ROTATION, new SerializableQuaternion (rot));
	}

	public virtual void generate () {
		if (entity == null)
			(entity = new GameObject ("mapobj").AddComponent<MapEntity> ()).init (this);
		else
			reloadEntity ();
	}

	public virtual void teleport (Vector3 pos) {
		this.pos = pos;
		if (entity != null) {
			entity.transform.position = pos;
		}
	}

	public virtual void teleport (Vector3 pos, Quaternion rot) {
		teleport (pos);
		this.rot = rot;
		if (entity != null) {
			entity.transform.rotation = rot;
		}
	}

	public virtual void reloadEntity () {
		if (entity == null)
			return;
		entity.transform.position = pos;
		entity.transform.rotation = rot;
	}

	//時間が経過するメソッド。ticksには経過時間を指定。
	public virtual void TimePasses (long ticks) {
	}

	public virtual void SyncFromEntity () {
		if (entity != null) {
			pos = entity.transform.position;
			rot = entity.transform.rotation;
		}
	}
}
