using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

[Serializable]
public class PermanentCoupler : MapObject
{

    public const string KEY_BODY1 = "BODY1";
    public const string KEY_BODY2 = "BODY2";
    public const string KEY_HEIGHT = "HEIGHT";
    public const string KEY_COUPLING_FACE = "COUPLING_FACE";
    public const string KEY_LENGTH = "LENGTH";

    public const float COLLIDER_WIDTH = 0.28f;
    public const float COLLIDER_HEIGHT = 0.28f;
    public const float COLLIDER_DEPTH = 1.82f;

    public Body body1;
    public Body body2;
    public float height;
    public float couplingFace;
    public float length;

    public GameObject modelObj;

    public PermanentCoupler(Map map) : base(map)
    {
        height = 0.845f;
        couplingFace = 10f;
        length = 1.84f;
    }

    protected PermanentCoupler(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        body1 = (Body)info.GetValue(KEY_BODY1, typeof(Body));
        body2 = (Body)info.GetValue(KEY_BODY2, typeof(Body));
        height = info.GetSingle(KEY_HEIGHT);
        couplingFace = info.GetSingle(KEY_COUPLING_FACE);
        length = info.GetSingle(KEY_LENGTH);
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(KEY_BODY1, body1);
        info.AddValue(KEY_BODY2, body2);
        info.AddValue(KEY_HEIGHT, height);
        info.AddValue(KEY_COUPLING_FACE, couplingFace);
        info.AddValue(KEY_LENGTH, length);
    }

    public override void generate()
    {
        if (entity == null)
            (entity = new GameObject("permanentCoupler").AddComponent<MapEntity>()).init(this);
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
        body1.snapToBogieFrame();
        body2.snapToBogieFrame();
        var pos1 = body1.pos + body1.rot * new Vector3(0, height - body1.height, (couplingFace - length / 2) * ((Quaternion.Inverse(body1.rot) * (body2.pos - body1.pos)).z > 0 ? 1 : -1));
        var pos2 = body2.pos + body2.rot * new Vector3(0, height - body2.height, (couplingFace - length / 2) * ((Quaternion.Inverse(body2.rot) * (body1.pos - body2.pos)).z > 0 ? 1 : -1));
        pos = (pos1 + pos2) / 2;

        rot = Quaternion.LookRotation(pos2 - pos1);
    }

    public void snapFrom()
    {
        body1.pos = pos + rot * Vector3.back * length / 2 * ((Quaternion.Inverse(body1.rot) * (body2.pos - body1.pos)).z > 0 ? 1 : -1) + body1.rot * new Vector3(0, body1.height - height, (length / 2 - couplingFace) * ((Quaternion.Inverse(body1.rot) * (body2.pos - body1.pos)).z > 0 ? 1 : -1));
        body2.pos = pos + rot * Vector3.forward * length / 2 * ((Quaternion.Inverse(body1.rot) * (body2.pos - body1.pos)).z > 0 ? 1 : -1) + body2.rot * new Vector3(0, body2.height - height, (length / 2 - couplingFace) * ((Quaternion.Inverse(body2.rot) * (body1.pos - body2.pos)).z > 0 ? 1 : -1));
        body1.snapFromBogieFrame();
        body2.snapFromBogieFrame();
    }

    public override void reloadEntity()
    {
        if (entity == null)
            return;

        if (modelObj == null)
        {
            (modelObj = GameObject.Instantiate(Main.main.permanentCouplerModel)).transform.parent = entity.transform;
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
