using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

[Serializable]
public class Body : MapObject
{

    public const string KEY_HEIGHT = "HEIGHT";
    public const string KEY_BOGIE_CENTER_DIST = "BCD";
    public const string KEY_BOGIEFRAMES = "BOGIEFRAMES";

    public const float COLLIDER_WIDTH = 2.95f;
    public const float COLLIDER_HEIGHT = 2.65f;
    public const float COLLIDER_DEPTH = 19.5f;

    public float height;
    public float bogieCenterDist;
    public List<BogieFrame> bogieFrames { get; private set; }

    public GameObject modelObj;

    public Body(Map map) : base(map)
    {
        height = 2.295f;
        bogieCenterDist = 13.8f;
        bogieFrames = new List<BogieFrame>();
    }

    protected Body(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        height = info.GetSingle(KEY_HEIGHT);
        bogieCenterDist = info.GetSingle(KEY_BOGIE_CENTER_DIST);
        bogieFrames = (List<BogieFrame>) info.GetValue(KEY_BOGIEFRAMES, typeof(List<BogieFrame>));
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(KEY_HEIGHT, height);
        info.AddValue(KEY_BOGIE_CENTER_DIST, bogieCenterDist);
        info.AddValue(KEY_BOGIEFRAMES, bogieFrames);
    }

    public void addBogieFrame(BogieFrame bogieFrame)
    {
        bogieFrames.Add(bogieFrame);
    }

    public void setBogieFrames(List<BogieFrame> bogieFrames)
    {
        this.bogieFrames = bogieFrames;
    }

    public bool removeBogieFrame(BogieFrame bogieFrame)
    {
        return bogieFrames.Remove(bogieFrame);
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
                           : rot * Vector3.forward * bogieCenterDist * ((float) -(bogieFrames.Count - 1) / 2 + d)));

            pos = (p_ / bogieFrames.Count) + rot * Vector3.up * height;
        }
    }

    public void snapFromBogieFrame()
    {
        //台車枠を車軸に合わせると、台車中心間距離を失う
        for (var d = 0; d < bogieFrames.Count; d++)
        {
            bogieFrames[d].pos = pos + bogieFrames[d].rot * (Vector3.up * (bogieFrames[d].height - height)) +
                                 (bogieFrames.Count == 1 || d * 2 - (bogieFrames.Count - 1) == 0
                                     ? Vector3.zero
                                     : rot * Vector3.forward * bogieCenterDist *
                                       ((float) -(bogieFrames.Count - 1) / 2 + d));
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
