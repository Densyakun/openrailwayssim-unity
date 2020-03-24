using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

/// <summary>
/// マップ
/// </summary>
[Serializable]
public class Map : ISerializable
{

    /// <summary>
    /// 一日の時間 (秒)
    /// </summary>
    public const float TIME_OF_DAY = 86400f;

    public const string KEY_OBJECTS = "OBJS";
    public const string KEY_TIME = "TIME";
    public const string KEY_CAMERA_POS = "CAMERA_POS";
    public const string KEY_CAMERA_ROT = "CAMERA_ROT";

    public static Vector3 DEFAULT_CAMERA_POS = new Vector3(0f, 10f, -20f);
    public static Vector3 DEFAULT_CAMERA_ROT = new Vector3(30f, 0f, 0f);

    public string mapname;

    [Serializable]
    public class MapInfo
    {
        public DateTime created;
        public DateTime updated;

        public void update()
        {
            updated = DateTime.Now;
        }
    }
    public MapInfo info; // マップの情報
    public List<MapObject> objs { get; private set; } // オブジェクト
    public float time; // マップの時間。0時からの秒数
    public Vector3 cameraPos; // カメラの位置（マップ読み込み時用）
    public Vector3 cameraRot; // カメラの角度（マップ読み込み時用）

    public Map(string mapname)
    {
        this.mapname = mapname;

        info = new MapInfo();
        info.created = info.updated = DateTime.Now;

        objs = new List<MapObject>();
        time = 9f * 60f * 60f; // 朝9時からスタート
        cameraPos = DEFAULT_CAMERA_POS;
        cameraRot = DEFAULT_CAMERA_ROT;
    }

    protected Map(SerializationInfo info, StreamingContext context)
    {
        if (info == null)
            throw new ArgumentNullException("info");
        objs = (List<MapObject>)info.GetValue(KEY_OBJECTS, typeof(List<MapObject>));
        time = info.GetSingle(KEY_TIME);
        cameraPos = ((SerializableVector3)info.GetValue(KEY_CAMERA_POS, typeof(SerializableVector3))).toVector3();
        cameraRot = ((SerializableVector3)info.GetValue(KEY_CAMERA_ROT, typeof(SerializableVector3))).toVector3();
    }

    public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        if (info == null)
            throw new ArgumentNullException("info");
        info.AddValue(KEY_OBJECTS, objs);
        info.AddValue(KEY_TIME, time);
        info.AddValue(KEY_CAMERA_POS, new SerializableVector3(cameraPos));
        info.AddValue(KEY_CAMERA_ROT, new SerializableVector3(cameraRot));
    }

    public void init()
    {
        foreach (var obj in objs)
            if (obj != null)
                obj.map = this;
    }

    public void addObject(MapObject obj)
    {
        objs.Add(obj);
    }

    public bool removeObject(MapObject obj)
    {
        if (obj is Track)
            ((Track)obj).removeConnects();
        if (obj is BogieFrame)
            foreach (var axle in ((BogieFrame)obj).axles)
                axle.bogieFrame = null;
        if (obj is Body)
            foreach (var bogieFrame in ((Body)obj).bogieFrames)
                bogieFrame.body = null;
        if (obj is PermanentCoupler)
            ((PermanentCoupler)obj).removeConnects();
        return objs.Remove(obj);
    }

    public virtual void generate()
    {
        foreach (MapObject obj in objs)
            obj.generate();
    }

    public void DestroyAll()
    {
        foreach (MapEntity e in GameObject.FindObjectsOfType<MapEntity>())
            e.Destroy();
    }

    public void Update()
    {
        time = Mathf.Repeat(time + Time.deltaTime, TIME_OF_DAY);
    }

    public int getRawMinutes()
    {
        return Mathf.FloorToInt(getRawSeconds() / 60f);
    }

    public int getRawSeconds()
    {
        return Mathf.FloorToInt(time);
    }

    public int getHours()
    {
        return Mathf.FloorToInt(getRawMinutes() / 60f);
    }

    public int getMinutes()
    {
        return getRawMinutes() - getHours() * 60;
    }

    public int getSeconds()
    {
        return getRawSeconds() - getRawMinutes() * 60;
    }
}
