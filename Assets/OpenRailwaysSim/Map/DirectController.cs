using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

[Serializable]
public class DirectController : MapObject
{

    public const string KEY_BODY = "BODY";
    public const string KEY_AXLES = "AXLES";
    public const string KEY_REVERSER = "REVERSER";
    public const string KEY_NOTCH = "NOTCH";
    public const string KEY_POWER_NOTCHS = "POWER_NOTCHS";
    public const string KEY_BRAKE_NOTCHS = "BRAKE_NOTCHS";

    public const float COLLIDER_WIDTH = 1;
    public const float COLLIDER_HEIGHT = 1;
    public const float COLLIDER_DEPTH = 1;

    public Body body;
    public List<Axle> axles;
    public int reverser;
    public int notch;
    public int powerNotchs;
    public int brakeNotchs;

    public GameObject modelObj;

    public DirectController(Map map, Body body, List<Axle> axles) : base(map)
    {
        this.body = body;
        this.axles = axles;
        reverser = 0;
        notch = 0;
        powerNotchs = 5;
        brakeNotchs = 8;
    }

    protected DirectController(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        body = (Body)info.GetValue(KEY_BODY, typeof(Body));
        axles = (List<Axle>)info.GetValue(KEY_AXLES, typeof(List<Axle>));
        reverser = info.GetInt32(KEY_REVERSER);
        notch = info.GetInt32(KEY_NOTCH);
        powerNotchs = info.GetInt32(KEY_POWER_NOTCHS);
        brakeNotchs = info.GetInt32(KEY_BRAKE_NOTCHS);
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(KEY_BODY, body);
        info.AddValue(KEY_AXLES, axles);
        info.AddValue(KEY_REVERSER, reverser);
        info.AddValue(KEY_NOTCH, notch);
        info.AddValue(KEY_POWER_NOTCHS, powerNotchs);
        info.AddValue(KEY_BRAKE_NOTCHS, brakeNotchs);
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
        if (notch < 0 || notch > 0 && reverser != 0)
        {
            float w = 0;
            foreach (var axle in axles)
            {
                if (w == 0)
                    w = axle.getTrainLoad();
                var a = axle.tm_output * axle.wheelDia * axles.Count * Time.deltaTime * notch / 2 / w / axle.gearRatio;
                if (a >= 0)
                {
                    if (reverser == -1)
                        a = -a;
                    axle.speed += a / powerNotchs;
                }
                else
                {
                    if (axle.speed > 0)
                        axle.speed = axle.speed - Mathf.Min(-a / brakeNotchs, axle.speed);
                    else
                        axle.speed = axle.speed + Mathf.Min(-a / brakeNotchs, -axle.speed);
                }
            }
        }
        snapTo();
        reloadEntity();
    }

    public void snapTo()
    {
        body.snapToBogieFrame();
        pos = body.pos + body.rot * Vector3.up * (4f - body.bogieHeight);
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
