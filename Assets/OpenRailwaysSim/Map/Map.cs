using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

[Serializable]
public class Map : ISerializable {
	public const string KEY_MAPNAME = "MAPNAME";
	public const string KEY_CREATED = "CREATED";
	public const string KEY_TRACKS = "TRACKS";
	public const string KEY_OBJECTS = "OBJS";
	public const string KEY_TIME = "TIME";
	public const string KEY_FAST_FORWARDING = "FASTFORWARDING";
	public const float ABYSS_HEIGHT = -100f;
	public const float FAST_FORWARDING_SPEED = 72f; //早送り中の速度。実時間20分でゲームが1日進む

	//・複数のマップを同時に読み込んではいけない。

	public string mapname { get; }

	//TODO マップ全体を読み込まなくてもファイルヘッダにマップの基本情報を書き込む。
	//マップの基本情報にはマップのチャンクやプレイヤーデータを除いたマップの作成日時などがある。
	public DateTime created { get; }
	public List<Track> tracks { get; private set; }
	public List<MapObject> objs { get; private set; }
	public long time { get; private set; } //マップの時間。0時から始まり1tickが1msである。
	public bool fastForwarding { get; private set; } //早送り

	//TODO マップに変更があるかどうかの判定（自動セーブ用）

	public Map (string mapname) {
		this.mapname = mapname;
		created = DateTime.Now;
		tracks = new List<Track> ();
		objs = new List<MapObject> ();
		time = 6 * 60 * 60000; //朝6時からスタート
		fastForwarding = false;
	}

	protected Map (SerializationInfo info, StreamingContext context) {
		if (info == null)
			throw new ArgumentNullException ("info");
		mapname = info.GetString (KEY_MAPNAME);
		created = new DateTime (info.GetInt64 (KEY_CREATED));
		tracks = (List<Track>)info.GetValue (KEY_TRACKS, typeof(List<Track>));
		foreach (Track track in tracks)
			track.map = this;
		objs = (List<MapObject>)info.GetValue (KEY_OBJECTS, typeof(List<MapObject>));
		foreach (MapObject obj in objs)
			obj.map = this;
		time = info.GetInt64 (KEY_TIME);
		fastForwarding = info.GetBoolean (KEY_FAST_FORWARDING);
	}

	public virtual void GetObjectData (SerializationInfo info, StreamingContext context) {
		if (info == null)
			throw new ArgumentNullException ("info");
		info.AddValue (KEY_MAPNAME, mapname);
		info.AddValue (KEY_CREATED, created.Ticks);
		info.AddValue (KEY_TRACKS, tracks);
		info.AddValue (KEY_OBJECTS, objs);
		info.AddValue (KEY_TIME, time);
		info.AddValue (KEY_FAST_FORWARDING, fastForwarding);
	}

	public void addObject (MapObject obj) {
		objs.Add (obj);
	}

	public bool removeObject (MapObject obj) {
		return objs.Remove (obj);
	}

	public void addTrack (Track track) {
		tracks.Add (track);
	}

	public bool removeTrack (Track track) {
		return tracks.Remove (track);
	}

	public void DestroyAll () {
		foreach (MapEntity e in GameObject.FindObjectsOfType<MapEntity> ())
			e.Destroy ();
		foreach (TrackEntity e in GameObject.FindObjectsOfType<TrackEntity> ())
			GameObject.Destroy (e.gameObject);
	}

	//時間が経過するメソッド。ticksには経過時間を指定。
	public void TimePasses (long ticks) {
		time += ticks;
	}

	public void setFastForwarding (bool fastForwarding) {
		this.fastForwarding = fastForwarding;
	}

	public long getRawHours () {
		return Mathf.FloorToInt (getRawMinutes () / 60);
	}

	public long getRawMinutes () {
		return Mathf.FloorToInt (getRawSeconds () / 60);
	}

	public long getRawSeconds () {
		return Mathf.FloorToInt (time / 1000);
	}

	public long getDays () {
		return Mathf.FloorToInt (getRawHours () / 24);
	}

	public long getHours () {
		return getRawHours () - getDays () * 24;
	}

	public long getMinutes () {
		return getRawMinutes () - getRawHours () * 60;
	}

	public long getSeconds () {
		return getRawSeconds () - getRawMinutes () * 60;
	}

	public long getMilliSeconds () {
		return time - getRawSeconds () * 1000;
	}
}
