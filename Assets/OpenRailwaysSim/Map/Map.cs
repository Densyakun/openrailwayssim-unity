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

    public const string KEY_MAPNAME = "MAPNAME";
    public const string KEY_CREATED = "CREATED";
    public const string KEY_OBJECTS = "OBJS";
    public const string KEY_TIME = "TIME";
    public const string KEY_FAST_FORWARDING = "FASTFORWARDING";
    public const string KEY_CAMERA_POS = "CAMERA_POS";

    public const string KEY_CAMERA_ROT = "CAMERA_ROT";

    //public const float ABYSS_HEIGHT = -100f;
    public const float FAST_FORWARDING_SPEED = 72f; // 早送り中の速度。実時間20分でゲームが1日進む

    public static Vector3 DEFAULT_CAMERA_POS = new Vector3(0f, 10f, -20f);
    public static Vector3 DEFAULT_CAMERA_ROT = new Vector3(30f, 0f, 0f);

    public string mapname { get; private set; }
    public DateTime created { get; private set; }
    public List<MapObject> objs { get; private set; } // オブジェクト
    public long time { get; private set; } // マップの時間。0時から始まり1tickが1msである。
    public bool fastForwarding { get; private set; } // 早送り
    public Vector3 cameraPos; // カメラの位置（マップ読み込み時用）
    public Vector3 cameraRot; // カメラの角度（マップ読み込み時用）

    public Map(string mapname)
    {
        this.mapname = mapname;
        created = DateTime.Now;
        objs = new List<MapObject>();
        time = 6 * 60 * 60000; // 朝6時からスタート
        fastForwarding = false;
        cameraPos = DEFAULT_CAMERA_POS;
        cameraRot = DEFAULT_CAMERA_ROT;
    }

    protected Map(SerializationInfo info, StreamingContext context)
    {
        if (info == null)
            throw new ArgumentNullException("info");
        mapname = info.GetString(KEY_MAPNAME);
        created = new DateTime(info.GetInt64(KEY_CREATED));
        objs = (List<MapObject>)info.GetValue(KEY_OBJECTS, typeof(List<MapObject>));
        foreach (var obj in objs)
            if (obj != null)
                obj.map = this;
        time = info.GetInt64(KEY_TIME);
        fastForwarding = info.GetBoolean(KEY_FAST_FORWARDING);
        cameraPos = ((SerializableVector3)info.GetValue(KEY_CAMERA_POS, typeof(SerializableVector3))).toVector3();
        cameraRot = ((SerializableVector3)info.GetValue(KEY_CAMERA_ROT, typeof(SerializableVector3))).toVector3();
    }

    public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        if (info == null)
            throw new ArgumentNullException("info");
        info.AddValue(KEY_MAPNAME, mapname);
        info.AddValue(KEY_CREATED, created.Ticks);
        info.AddValue(KEY_OBJECTS, objs);
        info.AddValue(KEY_TIME, time);
        info.AddValue(KEY_FAST_FORWARDING, fastForwarding);
        info.AddValue(KEY_CAMERA_POS, new SerializableVector3(cameraPos));
        info.AddValue(KEY_CAMERA_ROT, new SerializableVector3(cameraRot));
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

    // 時間が経過するメソッド。ticksには経過時間を指定。
    public void TimePasses(long ticks)
    {
        time += ticks;
    }

    public void setFastForwarding(bool fastForwarding)
    {
        this.fastForwarding = fastForwarding;
    }

    public long getRawHours()
    {
        return Mathf.FloorToInt(getRawMinutes() / 60);
    }

    public long getRawMinutes()
    {
        return Mathf.FloorToInt(getRawSeconds() / 60);
    }

    public long getRawSeconds()
    {
        return Mathf.FloorToInt(time / 1000);
    }

    public long getDays()
    {
        return Mathf.FloorToInt(getRawHours() / 24);
    }

    public long getHours()
    {
        return getRawHours() - getDays() * 24;
    }

    public long getMinutes()
    {
        return getRawMinutes() - getRawHours() * 60;
    }

    public long getSeconds()
    {
        return getRawSeconds() - getRawMinutes() * 60;
    }

    public long getMilliSeconds()
    {
        return time - getRawSeconds() * 1000;
    }
}
