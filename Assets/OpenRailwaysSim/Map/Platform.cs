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

    /*public override void generate()
    {
        if (entity)
            reloadEntity();
        else
            (entity = new GameObject("Platform").AddComponent<MapEntity>()).init(this);
    }

    public override void reloadEntity()
    {
        if (entity == null)
            return;

        if (modelObj == null)
        {
            (modelObj = GameObject.Instantiate(Main.INSTANCE.axleModel)).transform.parent = entity.transform;
            modelObj.transform.localPosition = Vector3.zero;
            reloadCollider();
        }

        modelObj.transform.localEulerAngles = new Vector3(rotX, 0f);

        reloadMaterial(modelObj);

        base.reloadEntity();
    }

    public void reloadCollider()
    {
        BoxCollider collider = entity.GetComponent<BoxCollider>();
        if (collider == null)
            collider = entity.gameObject.AddComponent<BoxCollider>();
        collider.isTrigger = true;
        collider.size = new Vector3(COLLIDER_WIDTH, wheelDia, wheelDia);
    }*/
}
