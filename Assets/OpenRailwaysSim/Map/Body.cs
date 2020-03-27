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
    public const string KEY_RUNNING_RESISTANCE_C = "RUNNING_RESISTANCE_C";
    public const string KEY_FRONT_CAB = "FRONT_CAB";
    public const string KEY_BACK_CAB = "BACK_CAB";

    public const float COLLIDER_WIDTH = 2.95f;
    public const float COLLIDER_HEIGHT = 0.16f;
    public const float COLLIDER_DEPTH = 19f;

    /// <summary>
    /// 車両重量
    /// </summary>
    public float carWeight;
    /// <summary>
    /// 台車の高さ
    /// </summary>
    public float bogieHeight;
    /// <summary>
    /// 台車間距離
    /// </summary>
    public float bogieCenterDist;
    /// <summary>
    /// 連結面間距離
    /// </summary>
    public float carLength;
    /// <summary>
    /// 台車
    /// </summary>
    public List<BogieFrame> bogieFrames { get; private set; }
    /// <summary>
    /// 走行抵抗の定数C。車両あたりの空気抵抗に依存する値
    /// </summary>
    public float runningResistanceC;
    /// <summary>
    /// 前方の運転台
    /// </summary>
    public Cab frontCab;
    /// <summary>
    /// 後方の運転台
    /// </summary>
    public Cab backCab;


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
        runningResistanceC = 0.00000863125f;

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
        try { runningResistanceC = info.GetSingle(KEY_RUNNING_RESISTANCE_C); } catch { runningResistanceC = 0.0001381f; }

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
        info.AddValue(KEY_RUNNING_RESISTANCE_C, runningResistanceC);

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
        if (lastMoved == map.time)
            return;
        lastMoved = map.time;

        // 運転台の操作を反映
        if (Main.INSTANCE.runPanel.body == this)
            Main.INSTANCE.runPanel.controlOnUpdate();
        if (motors.Count != 0 && notch != 0)
        {
            float w = 0f;
            foreach (var axle in motors)
            {
                if (w == 0f)
                    w = axle.getTrainLoad() / motors.Count;
                var a = (float)notch;
                if (a < 0f)
                {
                    if (axle.speed < 0f)
                        axle.inputPower(-a / brakeNotchs, w, true);
                    else if (0f < axle.speed)
                        axle.inputPower(a / brakeNotchs, w, true);
                }
                else if (a > 0f && reverser != 0)
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
