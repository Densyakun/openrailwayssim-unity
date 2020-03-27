using System;
using System.Runtime.Serialization;
using UnityEngine;

/// <summary>
/// 運転台
/// </summary>
[Serializable]
public class Cab : MapObject
{

    public const string KEY_IS_FRONT = "IS_FRONT";

    public bool isFront;

    public Cab(Map map, bool isFront) : base(map)
    {
        this.isFront = isFront;
    }

    protected Cab(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        isFront = info.GetBoolean(KEY_IS_FRONT);
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(KEY_IS_FRONT, isFront);
    }

    public override void generate()
    {
        if (entity)
            reloadEntity();
        else
            (entity = new GameObject("Cab").AddComponent<MapEntity>()).init(this);
    }
}
