using System;
using System.Runtime.Serialization;
using UnityEngine;

/// <summary>
/// 停止位置
/// </summary>
[Serializable]
public class StopTarget : MapObject
{

    public const string KEY_ON_TRACK = "ON_TRACK";
    public const string KEY_ON_DIST = "ON_DIST";
    /*public const string KEY_STATION = "STA";
    public const string KEY_PASSENGER_TO = "P_TO";
    public const string KEY_PASSENGER_AMOUNT = "P_AMOUNT";
    public const string KEY_ALLOWABLE_DIST = "A_DIST";*/

    public static Vector3 COLLIDER_SIZE = new Vector3(1f, 1f, 1f);

    public Track onTrack;
    public float onDist;
    //public float allowable_dist;

    public TextEntity textEntity;

    public StopTarget(Map map, Track onTrack, float onDist) : base(map, onTrack.getPoint(onDist))
    {
        this.onTrack = onTrack;
        this.onDist = onDist;
        //allowable_dist = 0.5f;
    }

    protected StopTarget(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        onTrack = (Track)info.GetValue(KEY_ON_TRACK, typeof(Track));
        onDist = info.GetSingle(KEY_ON_DIST);
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(KEY_ON_TRACK, onTrack);
        info.AddValue(KEY_ON_DIST, onDist);
    }

    public override void generate()
    {
        if (entity)
            reloadEntity();
        else
            (entity = new GameObject("StopTarget").AddComponent<MapEntity>()).init(this);
    }

    public override void reloadEntity()
    {
        if (entity == null)
            return;

        if (textEntity == null)
            (textEntity = new GameObject("StopTargetText").AddComponent<TextEntity>()).obj = this;
        textEntity.str = "・";
        textEntity.normalColor = new Color(0f, 0f, 1f, 0.75f);

        base.reloadEntity();
    }

    public override void destroy()
    {
        GameObject.Destroy(textEntity.gameObject);

        base.destroy();
    }
}
