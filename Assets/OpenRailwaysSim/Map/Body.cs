using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

/// <summary>
/// 車体
/// </summary>
[Serializable]
public class Body : MapObject
{

    public const string KEY_CAR_WEIGHT = "CAR_WEIGHT";
    public const string KEY_BOGIE_HEIGHT = "BOGIE_HEIGHT";
    public const string KEY_BOGIE_CENTER_DIST = "BCD";
    public const string KEY_CAR_LENGTH = "CAR_LENGTH";
    public const string KEY_BOGIEFRAMES = "BOGIEFRAMES";

    public const float COLLIDER_WIDTH = 2.95f;
    public const float COLLIDER_HEIGHT = 0.16f;
    public const float COLLIDER_DEPTH = 19f;

    public float carWeight;
    public float bogieHeight;
    public float bogieCenterDist;
    public float carLength;
    public List<BogieFrame> bogieFrames { get; private set; }


    // Direct Controller
    public const string KEY_MOTORS = "MOTORS";
    public const string KEY_REVERSER = "REVERSER";
    public const string KEY_NOTCH = "NOTCH";
    public const string KEY_POWER_NOTCHS = "POWER_NOTCHS";
    public const string KEY_BRAKE_NOTCHS = "BRAKE_NOTCHS";

    public List<Axle> motors;
    public int reverser;
    public int notch;
    public int powerNotchs;
    public int brakeNotchs;


    public GameObject modelObj;
    [NonSerialized]
    public PermanentCoupler permanentCoupler1;
    [NonSerialized]
    public PermanentCoupler permanentCoupler2;
    public float speed = 0f;
    public float lastMoved = -1f;

    public Body(Map map, List<BogieFrame> bogieFrames) : base(map)
    {
        carWeight = 20.95f;
        bogieHeight = 0.97f;
        bogieCenterDist = 13.8f;
        carLength = 19.5f;
        foreach (var bf in this.bogieFrames = bogieFrames)
            bf.body = this;

        this.motors = new List<Axle>();
        reverser = 0;
        notch = 0;
        powerNotchs = 5;
        brakeNotchs = 8;
    }

    protected Body(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        carWeight = info.GetSingle(KEY_CAR_WEIGHT);
        bogieHeight = info.GetSingle(KEY_BOGIE_HEIGHT);
        bogieCenterDist = info.GetSingle(KEY_BOGIE_CENTER_DIST);
        carLength = info.GetSingle(KEY_CAR_LENGTH);
        bogieFrames = (List<BogieFrame>)info.GetValue(KEY_BOGIEFRAMES, typeof(List<BogieFrame>));
        foreach (var bf in bogieFrames)
            bf.body = this;

        motors = (List<Axle>)info.GetValue(KEY_MOTORS, typeof(List<Axle>));
        reverser = info.GetInt32(KEY_REVERSER);
        notch = info.GetInt32(KEY_NOTCH);
        powerNotchs = info.GetInt32(KEY_POWER_NOTCHS);
        brakeNotchs = info.GetInt32(KEY_BRAKE_NOTCHS);
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(KEY_CAR_WEIGHT, carWeight);
        info.AddValue(KEY_BOGIE_HEIGHT, bogieHeight);
        info.AddValue(KEY_BOGIE_CENTER_DIST, bogieCenterDist);
        info.AddValue(KEY_CAR_LENGTH, carLength);
        info.AddValue(KEY_BOGIEFRAMES, bogieFrames);
        foreach (var bf in bogieFrames)
            bf.body = this;

        info.AddValue(KEY_MOTORS, motors);
        info.AddValue(KEY_REVERSER, reverser);
        info.AddValue(KEY_NOTCH, notch);
        info.AddValue(KEY_POWER_NOTCHS, powerNotchs);
        info.AddValue(KEY_BRAKE_NOTCHS, brakeNotchs);
    }

    public override void generate()
    {
        if (entity)
            reloadEntity();
        else
            (entity = new GameObject("Body").AddComponent<MapEntity>()).init(this);
    }

    public override void update()
    {
        snapToBogieFrame();
        snapFromBogieFrame();

        reloadEntity();
    }

    /// <summary>
    /// 車体を台車枠に合わせる
    /// </summary>
    public void snapToBogieFrame()
    {
        if (lastMoved == Time.time)
            return;
        lastMoved = Time.time;

        // 運転台の操作を反映
        if (Main.INSTANCE.runPanel.body == this)
            Main.INSTANCE.runPanel.controlOnUpdate();
        if (motors.Count != 0 && notch != 0)
        {
            float w = 0f;
            foreach (var axle in motors)
            {
                if (w == 0)
                    w = axle.getTrainLoad() / motors.Count;
                var a = (float)notch;
                if (a < 0)
                {
                    if (axle.speed < 0)
                        axle.inputPower(-a / brakeNotchs, w, true);
                    else
                        axle.inputPower(a / brakeNotchs, w, true);
                }
                else if (a > 0 && reverser != 0)
                    axle.inputPower((reverser == 1 ? a : -a) / powerNotchs, w);
            }
        }

        if (bogieFrames.Count > 0)
        {
            var p = Vector3.zero;
            var p_ = Vector3.zero;
            speed = 0f;
            foreach (var d in bogieFrames)
            {
                d.snapToAxle();
                p += d.pos + d.rot * Vector3.down * d.height;
                speed += d.speed;
            }
            p /= bogieFrames.Count;
            speed /= bogieFrames.Count;

            if (bogieFrames.Count == 1)
                rot = bogieFrames[0].rot;
            else
            {
                var f = bogieFrames[bogieFrames.Count - 1].pos - bogieFrames[0].pos;
                if (f.sqrMagnitude != 0f)
                    rot = Quaternion.LookRotation(f, Quaternion.Lerp(bogieFrames[0].rot, bogieFrames[bogieFrames.Count - 1].rot, 0.5f) * Vector3.up);
            }

            for (var d = 0; d < bogieFrames.Count; d++)
                p_ += (p + (bogieFrames.Count == 1 || d * 2 - (bogieFrames.Count - 1) == 0
                           ? Vector3.zero
                           : rot * Vector3.forward * bogieCenterDist * ((float)-(bogieFrames.Count - 1) / 2 + d)));

            pos = (p_ / bogieFrames.Count) + rot * Vector3.up * bogieHeight;
        }
    }

    /// <summary>
    /// 台車枠を車体に合わせる
    /// </summary>
    public void snapFromBogieFrame()
    {
        for (var d = 0; d < bogieFrames.Count; d++)
        {
            bogieFrames[d].pos = pos + bogieFrames[d].rot * (Vector3.up * (bogieFrames[d].height - bogieHeight)) +
                                 (bogieFrames.Count == 1 || d * 2 - (bogieFrames.Count - 1) == 0
                                     ? Vector3.zero
                                     : rot * Vector3.forward * bogieCenterDist *
                                       ((float)-(bogieFrames.Count - 1) / 2 + d));
            bogieFrames[d].speed = speed;
            bogieFrames[d].snapFromAxle(); // 台車枠を車軸に合わせる。車体とずれ台車中心間距離を失うが、次のフレームで合わせるので省略している。
        }
    }

    public override void reloadEntity()
    {
        if (entity == null)
            return;

        if (modelObj == null)
        {
            (modelObj = GameObject.Instantiate(Main.INSTANCE.bodyModel)).transform.parent = entity.transform;
            modelObj.transform.localPosition = Vector3.zero;
            modelObj.transform.localEulerAngles = Vector3.zero;
            reloadCollider();
        }
        reloadMaterial();

        base.reloadEntity();
    }

    public override void reloadMaterial()
    {
        reloadMaterial(modelObj);
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
