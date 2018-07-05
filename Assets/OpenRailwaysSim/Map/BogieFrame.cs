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

    public const float COLLIDER_WIDTH = 2.3f;
    public const float COLLIDER_HEIGHT = 0.4f;
    public const float COLLIDER_DEPTH = 2.6f;

    public float height;
    public float wheelbase;
    public List<Axle> axles { get; private set; }

    public GameObject modelObj;

    public float lastFixed = -1;

    public BogieFrame(Map map) : base(map)
    {
        height = 0.8f;
        wheelbase = 2.1f;
        axles = new List<Axle>();
    }

    protected BogieFrame(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        height = info.GetSingle(KEY_HEIGHT);
        wheelbase = info.GetSingle(KEY_WHEELBASE);
        axles = (List<Axle>) info.GetValue(KEY_AXLES, typeof(List<Axle>));
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(KEY_HEIGHT, height);
        info.AddValue(KEY_WHEELBASE, wheelbase);
        info.AddValue(KEY_AXLES, axles);
    }

    public void addAxle(Axle axle)
    {
        axles.Add(axle);
    }

    public void setAxles(List<Axle> axles)
    {
        this.axles = axles;
    }

    public bool removeAxle(Axle axle)
    {
        return axles.Remove(axle);
    }

    public override void generate()
    {
        if (entity == null)
            (entity = new GameObject("bogieFrame").AddComponent<MapEntity>()).init(this);
        else
            reloadEntity();
    }

    public override void update()
    {
        reloadEntity();
    }

    public override void fixedUpdate()
    {
        fixedMove();
    }

    public void fixedMove()
    {
        if (lastFixed == Time.fixedTime)
            return;
        var p = Vector3.zero;
        var p_ = Vector3.zero;
        var q = Vector3.zero;
        foreach (var d in axles)
        {
            d.fixedMove();
            p += d.pos + d.rot * Vector3.down * d.wheelDia / 2;
        }

        p = p / axles.Count;

        foreach (var d in axles)
            q += d.rot.eulerAngles;
        rot = Quaternion.Euler(q / axles.Count);

        for (var d = 0; d < axles.Count; d++)
            p_ += (axles[d].pos = p + axles[d].rot * Vector3.up * axles[d].wheelDia / 2 +
                                  (axles.Count == 1 || d * 2 - (axles.Count - 1) == 0
                                      ? Vector3.zero
                                      : rot * Vector3.forward * wheelbase * ((float) -(axles.Count - 1) / 2 + d))) -
                  axles[d].rot * Vector3.down * axles[d].wheelDia / 2;

        pos = (p_ / axles.Count) + rot * Vector3.up * height;

        foreach (var axle in axles)
        {
            axle.reloadOnDist();
            axle.rot = rot;
        }
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
        }

        reloadMaterial(modelObj);

        reloadCollider();

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
