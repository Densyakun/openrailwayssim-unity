using System;
using System.Runtime.Serialization;
using UnityEngine;

//マップ上に存在するオブジェクトを管理するクラス
[Serializable]
public class MapObject : ISerializable
{
	public const string KEY_POS = "POS";
	public const string KEY_ROTATION = "ROT";

	public virtual MapEntity entity { get; protected set; }

	[NonSerialized] private Map _map;

	public Map map
	{
		get { return _map; }
		set
		{
			if (_map == null)
				_map = value;
		}
	}

	private Vector3 _pos;

	public Vector3 pos
	{
		get { return _pos; }
		set
		{
			_pos = value;
			if (entity != null)
				entity.transform.position = pos;
		}
	}

	private Quaternion _rot;

	public Quaternion rot
	{
		get { return _rot; }
		set
		{
			_rot = value;
			if (entity != null)
				entity.transform.rotation = rot;
		}
	}

	public bool useSelectingMat = false;

	public MapObject(Map map) : this(map, new Vector3(), new Quaternion())
	{
	}

	public MapObject(Map map, Vector3 pos) : this(map, pos, new Quaternion())
	{
	}

	public MapObject(Map map, Vector3 pos, Quaternion rot)
	{
		this.map = map;
		this._pos = pos;
		this._rot = rot;
	}

	protected MapObject(SerializationInfo info, StreamingContext context)
	{
		if (info == null)
			throw new ArgumentNullException("info");
		_pos = ((SerializableVector3) info.GetValue(KEY_POS, typeof(SerializableVector3))).toVector3();
		_rot = ((SerializableQuaternion) info.GetValue(KEY_ROTATION, typeof(SerializableQuaternion))).toQuaternion();
	}

	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		if (info == null)
			throw new ArgumentNullException("info");
		SyncFromEntity();
		info.AddValue(KEY_POS, new SerializableVector3(_pos));
		info.AddValue(KEY_ROTATION, new SerializableQuaternion(_rot));
	}

	public virtual void generate()
	{
		if (entity)
			reloadEntity();
		else
			(entity = new GameObject("mapobj").AddComponent<MapEntity>()).init(this);
	}

	public virtual void update()
	{
	}

	public virtual void fixedUpdate()
	{
	}

	public virtual void reloadEntity()
	{
		if (!entity)
			return;
		entity.transform.position = _pos;
		entity.transform.rotation = _rot;
	}

	//時間が経過するメソッド。ticksには経過時間を指定。
	public virtual void TimePasses(long ticks)
	{
	}

	public virtual void SyncFromEntity()
	{
		if (entity)
		{
			_pos = entity.transform.position;
			_rot = entity.transform.rotation;
		}
	}
}
