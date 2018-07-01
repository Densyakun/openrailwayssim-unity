using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

[Serializable]
public class BogieFrame : MapObject
{

    public const string KEY_AXLES = "AXLES";

    public List<Axle> axles { get; private set; }

    public GameObject modelObj;

    public BogieFrame(Map map) : base(map)
    {
        axles = new List<Axle>();
    }

    protected BogieFrame(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        axles = (List<Axle>) info.GetValue(KEY_AXLES, typeof(List<Axle>));
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(KEY_AXLES, axles);
    }

    public void addAxle(Axle axle)
    {
        axles.Add(axle);
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
        foreach (var a in axles)
        {
            a.fixedMove();
        }
        /*float a = speed * 10 * Time.deltaTime / 36;
        onDist += a;
        rotX += a * 360 / Mathf.PI * wheelDia;*/
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

        reloadCollider();

        base.reloadEntity();
    }

    public void reloadCollider()
    {
        /*BoxCollider collider = entity.GetComponent<BoxCollider>();
        if (collider == null)
            collider = entity.gameObject.AddComponent<BoxCollider>();
        collider.isTrigger = true;
        collider.center = Vector3.forward * length / 2;
        collider.size = new Vector3(COLLIDER_WIDTH, COLLIDER_HEIGHT, length);
        collider.enabled = enableCollider;*/
    }
}
