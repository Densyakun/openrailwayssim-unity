using System;
using System.Runtime.Serialization;
using UnityEngine;

/// <summary>
/// 永久連結器
/// </summary>
[Serializable]
public class PermanentCoupler : MapObject
{

    public const string KEY_BODY1 = "BODY1";
    public const string KEY_BODY2 = "BODY2";
    public const string KEY_HEIGHT = "HEIGHT";
    public const string KEY_LENGTH = "LENGTH";

    public const float COLLIDER_WIDTH = 0.28f;
    public const float COLLIDER_HEIGHT = 0.28f;
    public const float COLLIDER_DEPTH = 1.82f;

    public Body body1;
    public Body body2;
    public float height;
    public float length;

    public GameObject modelObj;
    public float lastMoved = -1f;

    public PermanentCoupler(Map map, Body body1, Body body2) : base(map)
    {
        height = 0.845f;
        length = 1.84f;
        this.body1 = body1;
        this.body2 = body2;
        if (body1.permanentCoupler1 == null)
            body1.permanentCoupler1 = this;
        else
            body1.permanentCoupler2 = this;
        if (body2.permanentCoupler1 == null)
            body2.permanentCoupler1 = this;
        else
            body2.permanentCoupler2 = this;
    }

    protected PermanentCoupler(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        body1 = (Body)info.GetValue(KEY_BODY1, typeof(Body));
        body2 = (Body)info.GetValue(KEY_BODY2, typeof(Body));
        height = info.GetSingle(KEY_HEIGHT);
        length = info.GetSingle(KEY_LENGTH);
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(KEY_BODY1, body1);
        info.AddValue(KEY_BODY2, body2);
        info.AddValue(KEY_HEIGHT, height);
        info.AddValue(KEY_LENGTH, length);
    }

    public override void generate()
    {
        if (entity)
            reloadEntity();
        else
            (entity = new GameObject("PermanentCoupler").AddComponent<MapEntity>()).init(this);
    }

    public override void update()
    {
        snapTo();
        snapFrom();
        reloadEntity();
    }

    /// <summary>
    /// 永久連結器を二つの車体に合わせる
    /// </summary>
    public void snapTo()
    {
        if (lastMoved == Time.time)
            return;
        lastMoved = Time.time;

        body1.snapToBogieFrame();
        body2.snapToBogieFrame();
        var pos1 = body1.pos + body1.rot * new Vector3(0, height - body1.bogieHeight, (body1.carLength / 2 - length / 2) * ((Quaternion.Inverse(body1.rot) * (body2.pos - body1.pos)).z > 0 ? 1 : -1));
        var pos2 = body2.pos + body2.rot * new Vector3(0, height - body2.bogieHeight, (body2.carLength / 2 - length / 2) * ((Quaternion.Inverse(body2.rot) * (body1.pos - body2.pos)).z > 0 ? 1 : -1));
        pos = (pos1 + pos2) / 2;

        var f = pos2 - pos1;
        if (f.sqrMagnitude != 0f)
            rot = Quaternion.LookRotation(f, Quaternion.Lerp(body1.rot, body2.rot, 0.5f) * Vector3.up);
    }

    /// <summary>
    /// 二つの車体を永久連結器に合わせる
    /// </summary>
    public void snapFrom()
    {
        body1.pos = pos + rot * Vector3.back * length / 2 * ((Quaternion.Inverse(body1.rot) * (body2.pos - body1.pos)).z > 0 ? 1 : -1) + body1.rot * new Vector3(0, body1.bogieHeight - height, (length / 2 - body1.carLength / 2) * ((Quaternion.Inverse(body1.rot) * (body2.pos - body1.pos)).z > 0 ? 1 : -1));
        body2.pos = pos + rot * Vector3.forward * length / 2 * ((Quaternion.Inverse(body1.rot) * (body2.pos - body1.pos)).z > 0 ? 1 : -1) + body2.rot * new Vector3(0, body2.bogieHeight - height, (length / 2 - body2.carLength / 2) * ((Quaternion.Inverse(body2.rot) * (body1.pos - body2.pos)).z > 0 ? 1 : -1));
        body1.speed = body2.speed = (body1.speed + body2.speed) / 2f;
        body1.snapFromBogieFrame();
        body2.snapFromBogieFrame();
    }

    public override void reloadEntity()
    {
        if (entity == null)
            return;

        if (modelObj == null)
        {
            (modelObj = GameObject.Instantiate(Main.INSTANCE.permanentCouplerModel)).transform.parent = entity.transform;
            modelObj.transform.localPosition = Vector3.zero;
            modelObj.transform.localEulerAngles = Vector3.zero;
            reloadCollider();
        }
        reloadMaterial();

        base.reloadEntity();
    }

    public override void reloadMaterial()
    {
        reloadMaterial(modelObj);
    }

    public void reloadCollider()
    {
        var collider = entity.GetComponent<BoxCollider>();
        if (collider == null)
            collider = entity.gameObject.AddComponent<BoxCollider>();
        collider.isTrigger = true;
        collider.center = Vector3.zero;
        collider.size = new Vector3(COLLIDER_WIDTH, COLLIDER_HEIGHT, COLLIDER_DEPTH);
    }

    public void removeConnects()
    {
        if (this.body1.permanentCoupler1 == this)
            this.body1.permanentCoupler1 = null;
        else if (this.body1.permanentCoupler2 == this)
            this.body1.permanentCoupler2 = null;
        if (this.body2.permanentCoupler1 == this)
            this.body2.permanentCoupler1 = null;
        else if (this.body2.permanentCoupler2 == this)
            this.body2.permanentCoupler2 = null;
    }
}
