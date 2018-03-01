using System;
using System.Collections;
using System.Runtime.Serialization;
using UnityEngine;

[Serializable]
public class Track : MapObject {
	
	public Track (Map map, Vector3 pos, Quaternion rot) : base (map, pos, rot) {
	}

	protected Track (SerializationInfo info, StreamingContext context) : base (info, context) {
		if (info == null)
			throw new ArgumentNullException ("info");
		pos = ((SerializableVector3)info.GetValue (KEY_POS, typeof(SerializableVector3))).toVector3 ();
		rot = ((SerializableQuaternion)info.GetValue (KEY_ROTATION, typeof(SerializableQuaternion))).toQuaternion ();
	}

	public virtual void GetObjectData (SerializationInfo info, StreamingContext context) {
		base.GetObjectData (info, context);
	}

	public virtual void generate () {
		if (entity == null)
			(entity = new GameObject ("track").AddComponent<MapEntity> ()).init (this);
		else
			reloadEntity ();
	}

	public virtual void reloadEntity () {
		if (entity == null)
			return;
		entity.transform.position = pos;
		entity.transform.rotation = rot;
	}
}
