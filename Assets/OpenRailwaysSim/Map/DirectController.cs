using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

[Serializable]
public class DirectController : MapObject
{

    public const string KEY_BODY = "BODY";
    public const string KEY_NOTCH = "NOTCH";
    public const string KEY_AXLES = "AXLES";

    public const float COLLIDER_WIDTH = 1;
    public const float COLLIDER_HEIGHT = 1;
    public const float COLLIDER_DEPTH = 1;

    public Body body;
    public int notch;
    public List<Axle> axles;

    public GameObject modelObj;

    public DirectController(Map map) : base(map)
    {
        notch = 0;
        axles = new List<Axle>();
    }

    protected DirectController(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        body = (Body)info.GetValue(KEY_BODY, typeof(Body));
        notch = info.GetInt32(KEY_NOTCH);
        axles = (List<Axle>)info.GetValue(KEY_AXLES, typeof(List<Axle>));
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(KEY_BODY, body);
        info.AddValue(KEY_NOTCH, notch);
        info.AddValue(KEY_AXLES, axles);
    }

    public override void generate()
    {
        if (entity == null)
            (entity = new GameObject("directController").AddComponent<MapEntity>()).init(this);
        else
            reloadEntity();
    }

    public override void update()
    {
        if (notch > 0)
        {
            float w = 0;
            foreach (var axle in axles)
            {
                if (w == 0)
                    w = axle.getTrainLoad();
                axle.speed += axle.tm_output * axle.wheelDia * axles.Count * Time.deltaTime / 2 / w / axle.gearRatio;
            }
        }
        snapTo();
        reloadEntity();
    }

    public void snapTo()
    {
        body.snapToBogieFrame();
        pos = body.pos + body.rot * Vector3.up * (4f - body.height);
        rot = body.rot;
    }

    public override void reloadEntity()
    {
        if (entity == null)
            return;

        if (modelObj == null)
        {
            (modelObj = GameObject.Instantiate(Main.main.directControllerModel)).transform.parent = entity.transform;
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
