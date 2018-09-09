using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

[Serializable]
public class Body : MapObject
{

    public const string KEY_CAR_WEIGHT = "CAR_WEIGHT";
    public const string KEY_BOGIE_HEIGHT = "BOGIE_HEIGHT";
    public const string KEY_BOGIE_CENTER_DIST = "BCD";
    public const string KEY_CAR_LENGTH = "CAR_LENGTH";
    public const string KEY_BOGIEFRAMES = "BOGIEFRAMES";
    public const string KEY_PERMANENT_COUPLER_1 = "PC1";
    public const string KEY_PERMANENT_COUPLER_2 = "PC2";

    public const float COLLIDER_WIDTH = 2.95f;
    public const float COLLIDER_HEIGHT = 0.16f;
    public const float COLLIDER_DEPTH = 19f;

    public float carWeight;
    public float bogieHeight;
    public float bogieCenterDist;
    public float carLength;
    public List<BogieFrame> bogieFrames { get; private set; }
    public PermanentCoupler permanentCoupler1;
    public PermanentCoupler permanentCoupler2;

    public GameObject modelObj;

    public Body(Map map, List<BogieFrame> bogieFrames) : base(map)
    {
        carWeight = 20.95f;
        bogieHeight = 0.97f;
        bogieCenterDist = 13.8f;
        carLength = 19.5f;
        foreach (var bogieFrame in this.bogieFrames = bogieFrames)
            bogieFrame.body = this;
    }

    protected Body(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        carWeight = info.GetSingle(KEY_CAR_WEIGHT);
        bogieHeight = info.GetSingle(KEY_BOGIE_HEIGHT);
        bogieCenterDist = info.GetSingle(KEY_BOGIE_CENTER_DIST);
        carLength = info.GetSingle(KEY_CAR_LENGTH);
        bogieFrames = (List<BogieFrame>)info.GetValue(KEY_BOGIEFRAMES, typeof(List<BogieFrame>));
        permanentCoupler1 = (PermanentCoupler)info.GetValue(KEY_PERMANENT_COUPLER_1, typeof(PermanentCoupler));
        permanentCoupler2 = (PermanentCoupler)info.GetValue(KEY_PERMANENT_COUPLER_2, typeof(PermanentCoupler));
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(KEY_CAR_WEIGHT, carWeight);
        info.AddValue(KEY_BOGIE_HEIGHT, bogieHeight);
        info.AddValue(KEY_BOGIE_CENTER_DIST, bogieCenterDist);
        info.AddValue(KEY_CAR_LENGTH, carLength);
        info.AddValue(KEY_BOGIEFRAMES, bogieFrames);
        info.AddValue(KEY_PERMANENT_COUPLER_1, permanentCoupler1);
        info.AddValue(KEY_PERMANENT_COUPLER_2, permanentCoupler2);
    }

    public override void generate()
    {
        if (entity == null)
            (entity = new GameObject("body").AddComponent<MapEntity>()).init(this);
        else
            reloadEntity();
    }

    public override void update()
    {
        snapToBogieFrame();
        snapFromBogieFrame();
        reloadEntity();
    }

    public void snapToBogieFrame()
    {
        if (bogieFrames.Count > 0)
        {
            var p = Vector3.zero;
            var p_ = Vector3.zero;
            foreach (var d in bogieFrames)
            {
                d.snapToAxle();
                p += d.pos + d.rot * Vector3.down * d.height;
            }

            p /= bogieFrames.Count;

            if (bogieFrames.Count == 1)
                rot = bogieFrames[0].rot;
            else
                rot = Quaternion.LookRotation(bogieFrames[bogieFrames.Count - 1].pos - bogieFrames[0].pos);

            for (var d = 0; d < bogieFrames.Count; d++)
                p_ += (p + (bogieFrames.Count == 1 || d * 2 - (bogieFrames.Count - 1) == 0
                           ? Vector3.zero
                           : rot * Vector3.forward * bogieCenterDist * ((float)-(bogieFrames.Count - 1) / 2 + d)));

            pos = (p_ / bogieFrames.Count) + rot * Vector3.up * bogieHeight;
        }
    }

    public void snapFromBogieFrame()
    {
        //台車枠を車軸に合わせると、台車中心間距離を失う
        for (var d = 0; d < bogieFrames.Count; d++)
        {
            bogieFrames[d].pos = pos + bogieFrames[d].rot * (Vector3.up * (bogieFrames[d].height - bogieHeight)) +
                                 (bogieFrames.Count == 1 || d * 2 - (bogieFrames.Count - 1) == 0
                                     ? Vector3.zero
                                     : rot * Vector3.forward * bogieCenterDist *
                                       ((float)-(bogieFrames.Count - 1) / 2 + d));
            bogieFrames[d].snapFromAxle();
        }
        //台車枠を車軸に合わせると、車体がずれる。次のフレームで合わせるので省略している。
    }

    public override void reloadEntity()
    {
        if (entity == null)
            return;

        if (modelObj == null)
        {
            (modelObj = GameObject.Instantiate(Main.main.bodyModel)).transform.parent = entity.transform;
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
