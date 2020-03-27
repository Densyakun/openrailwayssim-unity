using System;
using System.Runtime.Serialization;
using UnityEngine;

/// <summary>
/// 輪軸
/// </summary>
[Serializable]
public class Axle : MapObject
{

    public const string KEY_ON_TRACK = "ON_TRACK";
    public const string KEY_ON_DIST = "ON_DIST";
    public const string KEY_SPEED = "SPEED";
    public const string KEY_WHEEL_DIA = "WHEEL_DIA";
    public const string KEY_ROT_X = "ROT_X";

    public const string KEY_TM_OUTPUT = "TM_OUTPUT";
    public const string KEY_GEAR_RATIO = "GEAR_RATIO";

    public const string KEY_STARTING_RESISTANCE = "STARTING_RESISTANCE";
    public const string KEY_RUNNING_RESISTANCE_A = "RUNNING_RESISTANCE_A";
    public const string KEY_RUNNING_RESISTANCE_B = "RUNNING_RESISTANCE_B";

    /// <summary>
    /// コライダーの幅
    /// </summary>
    public const float COLLIDER_WIDTH = 2.3f;

    /// <summary>
    /// 走行している軌道
    /// </summary>
    public Track onTrack { get; protected set; }
    protected float _onDist = 0f;
    /// <summary>
    /// 走行位置
    /// </summary>
    public float onDist
    {
        get { return _onDist; }
        set
        {
            if (1f < value)
            {
                if (onTrack.connectingNextTrack == -1)
                {
                    _onDist = 1f;
                    speed = 0f;
                    if (bogieFrame != null)
                    {
                        var a = bogieFrame.body;
                        if (a != null)
                        {
                            foreach (var bf in a.bogieFrames)
                                foreach (var axle in bf.axles)
                                    axle.speed = 0f;
                            var b = bogieFrame.body.permanentCoupler1;
                            while (b != null)
                            {
                                a = b.body1 == a ? b.body2 : b.body1;
                                foreach (var bf in a.bogieFrames)
                                    foreach (var axle in bf.axles)
                                        axle.speed = 0f;
                                b = a.permanentCoupler1 == b ? a.permanentCoupler2 : a.permanentCoupler1;
                            }
                            a = bogieFrame.body;
                            b = bogieFrame.body.permanentCoupler2;
                            while (b != null)
                            {
                                a = b.body1 == a ? b.body2 : b.body1;
                                foreach (var bf in a.bogieFrames)
                                    foreach (var axle in bf.axles)
                                        axle.speed = 0f;
                                b = a.permanentCoupler1 == b ? a.permanentCoupler2 : a.permanentCoupler1;
                            }
                        }
                    }
                }
                else
                {
                    var oldTrack = onTrack;
                    onTrack = oldTrack.nextTracks[oldTrack.connectingNextTrack];
                    if ((oldTrack.getPoint(1f) - onTrack.pos).sqrMagnitude < Track.ALLOWABLE_RANGE && (oldTrack.getRotation(1f).eulerAngles - onTrack.rot.eulerAngles).sqrMagnitude < Track.ALLOWABLE_RANGE)
                        onDist = (value * oldTrack.length - oldTrack.length) / onTrack.length;
                    else
                    {
                        speed = -speed;
                        onDist = (onTrack.length - value * oldTrack.length + oldTrack.length) / onTrack.length;
                    }
                }
            }
            else if (value < 0f)
            {
                if (onTrack.connectingPrevTrack == -1)
                {
                    _onDist = 0f;
                    speed = 0f;
                    if (bogieFrame != null)
                    {
                        var a = bogieFrame.body;
                        if (a != null)
                        {
                            foreach (var bf in a.bogieFrames)
                                foreach (var axle in bf.axles)
                                    axle.speed = 0f;
                            var b = bogieFrame.body.permanentCoupler1;
                            while (b != null)
                            {
                                a = b.body1 == a ? b.body2 : b.body1;
                                foreach (var bf in a.bogieFrames)
                                    foreach (var axle in bf.axles)
                                        axle.speed = 0f;
                                b = a.permanentCoupler1 == b ? a.permanentCoupler2 : a.permanentCoupler1;
                            }
                            a = bogieFrame.body;
                            b = bogieFrame.body.permanentCoupler2;
                            while (b != null)
                            {
                                a = b.body1 == a ? b.body2 : b.body1;
                                foreach (var bf in a.bogieFrames)
                                    foreach (var axle in bf.axles)
                                        axle.speed = 0f;
                                b = a.permanentCoupler1 == b ? a.permanentCoupler2 : a.permanentCoupler1;
                            }
                        }
                    }
                }
                else
                {
                    var oldTrack = onTrack;
                    onTrack = oldTrack.prevTracks[oldTrack.connectingPrevTrack];
                    if ((oldTrack.pos - onTrack.getPoint(1f)).sqrMagnitude < Track.ALLOWABLE_RANGE && (oldTrack.rot.eulerAngles - onTrack.getRotation(1f).eulerAngles).sqrMagnitude < Track.ALLOWABLE_RANGE)
                        onDist = (value * oldTrack.length + onTrack.length) / onTrack.length;
                    else
                    {
                        speed = -speed;
                        onDist = (-value * oldTrack.length) / onTrack.length;
                    }
                }
            }
            else
                _onDist = value;
        }
    }
    /// <summary>
    /// 車軸の速度 m/s
    /// </summary>
    public float speed;
    /// <summary>
    /// 車輪の直径
    /// </summary>
    public float wheelDia;
    /// <summary>
    /// 車軸の向きX
    /// </summary>
    public float rotX;

    // 速度制御
    /// <summary>
    /// 主電動機出力(定格) kW
    /// </summary>
    public float tm_output;
    /// <summary>
    /// 駆動装置の歯車比
    /// </summary>
    public float gearRatio;

    // 列車抵抗
    /// <summary>
    /// 出発抵抗 (N/t)
    /// </summary>
    public float startingResistance;
    /// <summary>
    /// 走行抵抗の定数A。輪軸あたりの車軸と軸受の摩擦に依存する値
    /// </summary>
    public float runningResistanceA;
    /// <summary>
    /// 走行抵抗の定数B。輪軸あたりの車輪とレールの摩擦に依存する値
    /// </summary>
    public float runningResistanceB;

    public GameObject modelObj;
    [NonSerialized]
    public BogieFrame bogieFrame;
    public float lastMoved = -1f;
    //public float lastMoved1 = -1f;

    public Axle(Map map, Track onTrack, float onDist) : base(map)
    {
        this.onTrack = onTrack;
        this.onDist = onDist;
        speed = 0f;
        wheelDia = 0.86f;
        rotX = 0f;
        Vector3 c = onTrack is Shape ?
            ((Shape)onTrack).getRotationCanted(onDist).eulerAngles :
            onTrack is Curve ?
            ((Curve)onTrack).getRotationCanted(onDist).eulerAngles :
            onTrack.rot.eulerAngles;
        pos = (onTrack is Shape ?
            ((Shape)onTrack).getPointCanted(onDist) :
            onTrack is Curve ?
            ((Curve)onTrack).getPointCanted(onDist) :
            onTrack.getPoint(onDist)) + Quaternion.Euler(c) * Vector3.up * wheelDia / 2f;
        rot = Quaternion.Euler(c);

        tm_output = 220f;
        gearRatio = 6.06f;

        startingResistance = 30f;
        runningResistanceA = 0.019890625f;
        runningResistanceB = 0.000015625f;
    }

    protected Axle(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        onTrack = (Track)info.GetValue(KEY_ON_TRACK, typeof(Track));
        _onDist = info.GetSingle(KEY_ON_DIST);
        speed = info.GetSingle(KEY_SPEED);
        wheelDia = info.GetSingle(KEY_WHEEL_DIA);
        rotX = info.GetSingle(KEY_ROT_X);

        tm_output = info.GetSingle(KEY_TM_OUTPUT);
        gearRatio = info.GetSingle(KEY_GEAR_RATIO);

        startingResistance = info.GetSingle(KEY_STARTING_RESISTANCE);
        runningResistanceA = info.GetSingle(KEY_RUNNING_RESISTANCE_A);
        runningResistanceB = info.GetSingle(KEY_RUNNING_RESISTANCE_B);
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(KEY_ON_TRACK, onTrack);
        info.AddValue(KEY_ON_DIST, _onDist);
        info.AddValue(KEY_SPEED, speed);
        info.AddValue(KEY_WHEEL_DIA, wheelDia);
        info.AddValue(KEY_ROT_X, rotX);

        info.AddValue(KEY_TM_OUTPUT, tm_output);
        info.AddValue(KEY_GEAR_RATIO, gearRatio);

        info.AddValue(KEY_STARTING_RESISTANCE, startingResistance);
        info.AddValue(KEY_RUNNING_RESISTANCE_A, runningResistanceA);
        info.AddValue(KEY_RUNNING_RESISTANCE_B, runningResistanceB);
    }

    public override void generate()
    {
        if (entity)
            reloadEntity();
        else
            (entity = new GameObject("Axle").AddComponent<MapEntity>()).init(this);
    }

    public override void update()
    {
        if (bogieFrame == null)
            move();
        reloadEntity();
    }

    /// <summary>
    /// 車軸を移動する
    /// </summary>
    public void move()
    {
        if (lastMoved == map.time)
            return;
        lastMoved = map.time;

        // 走行抵抗
        /*if (bogieFrame != null)
        {
            if (bogieFrame.body != null)
            {
                var e = 0;
                foreach (var bf in bogieFrame.body.bogieFrames)
                    e += bf.axles.Count;
                var w = bogieFrame.body.carWeight / e;
                var a = -Physics.gravity.y * (runningResistanceA + runningResistanceB * speed * 3.6f + bogieFrame.body.runningResistanceC * speed * speed * 3.6f * 3.6f / w / e) * Time.deltaTime * w / 1000f;
                if (speed < 0f)
                    speed = Mathf.Min(speed + a, 0f);
                else
                    speed = Mathf.Max(speed - a, 0f);
            }
        }*/

        var b = speed * Time.deltaTime;
        onDist += b / onTrack.length;
        rotX += b * 360f / Mathf.PI * wheelDia;

        Vector3 d = onTrack is Shape ?
            ((Shape)onTrack).getRotationCanted(onDist).eulerAngles :
            onTrack is Curve ?
            ((Curve)onTrack).getRotationCanted(onDist).eulerAngles :
            onTrack.rot.eulerAngles;
        pos = (onTrack is Shape ?
            ((Shape)onTrack).getPointCanted(onDist) :
            onTrack is Curve ?
            ((Curve)onTrack).getPointCanted(onDist) :
            onTrack.getPoint(onDist)) + Quaternion.Euler(d) * Vector3.up * wheelDia / 2f;
        rot = Quaternion.Euler(d);
    }

    /// <summary>
    /// 車軸を線路に合わせる
    /// </summary>
    public void reloadOnDist()
    {
        // 処理の順番の関係か、速度を実際の速度に設定するとほとんど動かなくなる
        /*if (lastMoved1 == map.time)
            return;
        lastMoved1 = map.time;

        var t = onTrack;
        var d = onDist;*/
        onDist = onTrack.getLength(pos) / onTrack.length;
        //if (onTrack == t)
        //    speed = (onDist - d) / Time.deltaTime;
    }

    public override void reloadEntity()
    {
        if (entity == null)
            return;

        if (modelObj == null)
        {
            (modelObj = GameObject.Instantiate(Main.INSTANCE.axleModel)).transform.parent = entity.transform;
            modelObj.transform.localPosition = Vector3.zero;
            reloadCollider();
        }
        reloadMaterial();
        modelObj.transform.localEulerAngles = new Vector3(rotX, 0f);

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
        collider.size = new Vector3(COLLIDER_WIDTH, wheelDia, wheelDia);
    }

    public float getTrainLoad()
    {
        float w = 0f;
        if (bogieFrame != null)
        {
            var a = bogieFrame.body;
            if (a != null)
            {
                w = bogieFrame.body.carWeight;
                var b = bogieFrame.body.permanentCoupler1;
                while (b != null)
                {
                    a = b.body1 == a ? b.body2 : b.body1;
                    w += a.carWeight;
                    b = a.permanentCoupler1 == b ? a.permanentCoupler2 : a.permanentCoupler1;
                }
                a = bogieFrame.body;
                b = bogieFrame.body.permanentCoupler2;
                while (b != null)
                {
                    a = b.body1 == a ? b.body2 : b.body1;
                    w += a.carWeight;
                    b = a.permanentCoupler1 == b ? a.permanentCoupler2 : a.permanentCoupler1;
                }
            }
        }
        return w;
    }

    public void inputPower(float power, float weight, bool brake = false)
    {
        var a = tm_output * wheelDia * Time.deltaTime * power / 2f / weight / gearRatio;
        if (brake)
        {
            if (speed > 0f)
                speed = Mathf.Max(0f, speed + a);
            else
                speed = Mathf.Min(0f, speed + a);
        }
        else
        {
            if (speed > 0f == a > 0f)
            {
                var b = (1f - Mathf.Clamp(Mathf.Abs(speed * 3.6f), 0f, 3f) / 3f) * startingResistance * Time.deltaTime / weight / 1000f;
                if (a > 0f)
                    speed = Mathf.Max(0f, speed + a - b);
                else
                    speed = Mathf.Min(0f, speed + a + b);
            }
            else
                speed += a;
        }
    }
}
