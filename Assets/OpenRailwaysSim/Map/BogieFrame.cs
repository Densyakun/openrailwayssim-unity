using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

/// <summary>
/// 台車枠
/// </summary>
[Serializable]
public class BogieFrame : MapObject
{

    public const string KEY_HEIGHT = "HEIGHT";
    public const string KEY_WHEELBASE = "WB";
    public const string KEY_AXLES = "AXLES";

    public const float COLLIDER_WIDTH = 2.3f;
    public const float COLLIDER_HEIGHT = 0.4f;
    public const float COLLIDER_DEPTH = 2.6f;

    public float height;
    public float wheelbase;
    public List<Axle> axles { get; private set; }

    public GameObject modelObj;
    [NonSerialized]
    public Body body;
    public float speed = 0f;

    public BogieFrame(Map map, List<Axle> axles) : base(map)
    {
        height = 0.8f;
        wheelbase = 2.1f;
        foreach (var axle in this.axles = axles)
            axle.bogieFrame = this;
    }

    protected BogieFrame(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        height = info.GetSingle(KEY_HEIGHT);
        wheelbase = info.GetSingle(KEY_WHEELBASE);
        axles = (List<Axle>)info.GetValue(KEY_AXLES, typeof(List<Axle>));
        foreach (var a in axles)
            a.bogieFrame = this;
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(KEY_HEIGHT, height);
        info.AddValue(KEY_WHEELBASE, wheelbase);
        info.AddValue(KEY_AXLES, axles);
    }

    public override void generate()
    {
        if (entity)
            reloadEntity();
        else
            (entity = new GameObject("BogieFrame").AddComponent<MapEntity>()).init(this);
    }

    public override void update()
    {
        if (body == null)
        {
            snapToAxle();
            snapFromAxle();
        }
        reloadEntity();
    }

    /// <summary>
    /// 台車枠を車軸に合わせる
    /// </summary>
    public void snapToAxle()
    {
        var p = Vector3.zero;
        var p_ = Vector3.zero;
        var q = Vector3.zero;
        speed = 0f;
        foreach (var d in axles)
        {
            d.move();
            p += d.pos + d.rot * Vector3.down * d.wheelDia / 2;
            speed += d.speed;
        }
        p /= axles.Count;
        speed /= axles.Count;

        if (axles.Count == 1)
            rot = axles[0].rot;
        else
        {
            var f = axles[axles.Count - 1].pos - axles[0].pos;
            if (f.sqrMagnitude != 0f)
                rot = Quaternion.LookRotation(f, Quaternion.Lerp(axles[0].rot, axles[axles.Count - 1].rot, 0.5f) * Vector3.up);
        }

        for (var d = 0; d < axles.Count; d++)
            p_ += (p + (axles.Count == 1 || d * 2 - (axles.Count - 1) == 0
                       ? Vector3.zero
                       : rot * Vector3.forward * wheelbase * ((float)-(axles.Count - 1) / 2 + d)));

        pos = (p_ / axles.Count) + rot * Vector3.up * height;
    }

    /// <summary>
    /// 車軸を台車枠に合わせる
    /// </summary>
    public void snapFromAxle()
    {
        for (var d = 0; d < axles.Count; d++)
        {
            axles[d].pos = pos + axles[d].rot * (Vector3.up * (axles[d].wheelDia / 2 - height)) +
                           (axles.Count == 1 || d * 2 - (axles.Count - 1) == 0
                               ? Vector3.zero
                               : rot * Vector3.forward * wheelbase * ((float)-(axles.Count - 1) / 2 + d));
        }
        foreach (var axle in axles)
        {
            axle.speed = speed;
            axle.reloadOnDist(); // 車軸をレールに合わせる。台車枠とずれホイールベースを失うが、次のフレームで合わせるので省略している。
            var r = rot * Quaternion.AngleAxis(-180f, Vector3.up);
            axle.rot = Mathf.Abs(Quaternion.Dot(axle.rot, rot)) < Mathf.Abs(Quaternion.Dot(axle.rot, r)) ? rot : r;
        }
    }

    public override void reloadEntity()
    {
        if (entity == null)
            return;

        if (modelObj == null)
        {
            (modelObj = GameObject.Instantiate(Main.INSTANCE.bogieFrameModel)).transform.parent = entity.transform;
            modelObj.transform.localPosition = Vector3.zero;
            modelObj.transform.localEulerAngles = Vector3.zero;
            reloadCollider();
        }

        reloadMaterial(modelObj);

        base.reloadEntity();
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
}
