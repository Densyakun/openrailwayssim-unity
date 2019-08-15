using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

[Serializable]
public class BogieFrame : MapObject
{

    public const string KEY_HEIGHT = "HEIGHT";
    public const string KEY_WHEELBASE = "WB";
    public const string KEY_AXLES = "AXLES";
    public const string KEY_BODY = "BODY";

    public const float COLLIDER_WIDTH = 2.3f;
    public const float COLLIDER_HEIGHT = 0.4f;
    public const float COLLIDER_DEPTH = 2.6f;

    public float height;
    public float wheelbase;
    public List<Axle> axles { get; private set; }
    public Body body;

    public GameObject modelObj;

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
        body = (Body)info.GetValue(KEY_BODY, typeof(Body));
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(KEY_HEIGHT, height);
        info.AddValue(KEY_WHEELBASE, wheelbase);
        info.AddValue(KEY_AXLES, axles);
        info.AddValue(KEY_BODY, body);
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
        snapToAxle();
        snapFromAxle();
        reloadEntity();
    }

    public void snapToAxle()
    {
        var p = Vector3.zero;
        var p_ = Vector3.zero;
        var q = Vector3.zero;
        foreach (var d in axles)
        {
            d.fixedMove();
            p += d.pos + d.rot * Vector3.down * d.wheelDia / 2;
        }

        p /= axles.Count;

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

    public void snapFromAxle()
    {
        float s = 0;
        for (var d = 0; d < axles.Count; d++)
        {
            axles[d].pos = pos + axles[d].rot * (Vector3.up * (axles[d].wheelDia / 2 - height)) +
                           (axles.Count == 1 || d * 2 - (axles.Count - 1) == 0
                               ? Vector3.zero
                               : rot * Vector3.forward * wheelbase * ((float)-(axles.Count - 1) / 2 + d));
            s += axles[d].speed;
        }

        s /= axles.Count;

        //車軸をレールに合わせると、ホイールベースを失う
        foreach (var axle in axles)
        {
            axle.reloadOnDist();
            var r = rot * Quaternion.AngleAxis(-180f, Vector3.up);
            axle.rot = Mathf.Abs(Quaternion.Dot(axle.rot, rot)) < Mathf.Abs(Quaternion.Dot(axle.rot, r)) ? rot : r;
            axle.speed = s;
        }
        //車軸をレールに合わせると、台車枠がずれる。次のフレームで合わせるので省略している。
    }

    public override void reloadEntity()
    {
        if (entity == null)
            return;

        if (modelObj == null)
        {
            (modelObj = GameObject.Instantiate(Main.main.bogieFrameModel)).transform.parent = entity.transform;
            modelObj.transform.localPosition = Vector3.zero;
            modelObj.transform.localEulerAngles = Vector3.zero;
            reloadCollider();
        }

        reloadMaterial(modelObj);

        base.reloadEntity();
    }

    public void reloadCollider()
    {
        BoxCollider collider = entity.GetComponent<BoxCollider>();
        if (collider == null)
            collider = entity.gameObject.AddComponent<BoxCollider>();
        collider.isTrigger = true;
        collider.center = Vector3.zero;
        collider.size = new Vector3(COLLIDER_WIDTH, COLLIDER_HEIGHT, COLLIDER_DEPTH);
    }
}
