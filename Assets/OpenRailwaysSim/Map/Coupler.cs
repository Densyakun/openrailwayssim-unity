using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

[Serializable]
public class Coupler : MapObject
{

    public const string KEY_BODY = "BODY";
    public const string KEY_IS_FRONT = "IS_FRONT";
    public const string KEY_HEIGHT = "HEIGHT";
    public const string KEY_COUPLING_FACE = "COUPLING_FACE";
    public const string KEY_LENGTH = "LENGTH";
    public const string KEY_CONNECTING_COUPLER = "CONNECTING_COUPLER";
    public const string KEY_LOCAL_ROT = "LOCAL_ROT";

    public const float COLLIDER_WIDTH = 0.28f;
    public const float COLLIDER_HEIGHT = 0.28f;
    public const float COLLIDER_DEPTH = 0.92f;

    public Body body;
    public bool isFront;
    public float height;
    public float couplingFace;
    public float length;
    public Coupler connectingCoupler;
    public Quaternion localRot;

    public GameObject modelObj;

    public Coupler(Map map) : base(map)
    {
        isFront = true;
        height = 0.845f;
        couplingFace = 10f;
        length = 0.92f;
        localRot = new Quaternion();
    }

    protected Coupler(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        body = (Body)info.GetValue(KEY_BODY, typeof(Body));
        isFront = info.GetBoolean(KEY_IS_FRONT);
        height = info.GetSingle(KEY_HEIGHT);
        couplingFace = info.GetSingle(KEY_COUPLING_FACE);
        length = info.GetSingle(KEY_LENGTH);
        connectingCoupler = (Coupler)info.GetValue(KEY_CONNECTING_COUPLER, typeof(Coupler));
        localRot = ((SerializableQuaternion)info.GetValue(KEY_LOCAL_ROT, typeof(SerializableQuaternion))).toQuaternion();
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(KEY_BODY, body);
        info.AddValue(KEY_IS_FRONT, isFront);
        info.AddValue(KEY_HEIGHT, height);
        info.AddValue(KEY_COUPLING_FACE, couplingFace);
        info.AddValue(KEY_LENGTH, length);
        info.AddValue(KEY_CONNECTING_COUPLER, connectingCoupler);
        info.AddValue(KEY_LOCAL_ROT, new SerializableQuaternion(localRot));
    }

    public override void generate()
    {
        if (entity == null)
            (entity = new GameObject("coupler").AddComponent<MapEntity>()).init(this);
        else
            reloadEntity();
    }

    public override void update()
    {
        snapTo();
        snapFrom();
        reloadEntity();
    }

    public void snapTo()
    {
        body.snapToBogieFrame();
        pos = body.pos + body.rot * new Vector3(0, height - body.height, (couplingFace - length) * (isFront ? 1 : -1));

        if (connectingCoupler == null)
        {
            var e = Quaternion.Euler((isFront ? body.rot.eulerAngles : body.rot.eulerAngles + Vector3.up * -180) + localRot.eulerAngles);
            rot = e;
        }
        else
            rot = Quaternion.LookRotation(connectingCoupler.pos - pos);
    }

    public void snapFrom()
    {
        body.pos = pos + body.rot * new Vector3(0, body.height - height, (length - couplingFace) * (isFront ? 1 : -1));
        body.snapFromBogieFrame();
    }

    public override void reloadEntity()
    {
        if (entity == null)
            return;

        if (modelObj == null)
        {
            (modelObj = GameObject.Instantiate(Main.main.couplerModel)).transform.parent = entity.transform;
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
