using System;
using System.Runtime.Serialization;
using UnityEngine;

/// <summary>
/// プラットホーム
/// </summary>
[Serializable]
public class Platform : MapObject
{

    public const string KEY_ON_TRACK = "ON_TRACK";
    public const string KEY_ON_DIST = "ON_DIST";
    public const string KEY_LENGTH = "LEN";
    public const string KEY_PASSENGER_TO = "P_TO";
    public const string KEY_PASSENGER_ = "P_AMOUNT";

    public static Vector3 COLLIDER_SIZE = new Vector3(1f, 1f, 1f);

    public Track onTrack { get; protected set; }

    public GameObject modelObj;

    public Platform(Map map, Track onTrack, float onDist) : base(map)
    {
        this.onTrack = onTrack;
    }

    protected Platform(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        onTrack = (Track)info.GetValue(KEY_ON_TRACK, typeof(Track));
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(KEY_ON_TRACK, onTrack);
    }
}
