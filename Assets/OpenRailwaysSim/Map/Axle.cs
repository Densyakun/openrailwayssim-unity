using System;
using System.Runtime.Serialization;
using UnityEngine;

/// <summary>
/// 車軸
/// </summary>
[Serializable]
public class Axle : MapObject
{

    public const string KEY_ON_TRACK = "ON_TRACK";
    public const string KEY_ON_DIST = "ON_DIST";
    public const string KEY_SPEED = "SPEED";
    public const string KEY_WHEEL_DIA = "WHEEL_DIA";
    public const string KEY_TM_OUTPUT = "TM_OUTPUT";
    public const string KEY_GEAR_RATIO = "GEAR_RATIO";
    public const string KEY_STARTING_RESISTANCE = "STARTING_RESISTANCE";
    public const string KEY_RUNNING_RESISTANCE_A = "RUNNING_RESISTANCE_A";
    public const string KEY_RUNNING_RESISTANCE_B = "RUNNING_RESISTANCE_B";
    public const string KEY_ROT_X = "ROT_X";
    public const string KEY_BOGIE_FRAME = "BOGIE_FRAME";

    public const float COLLIDER_WIDTH = 2.3f;

    public Track onTrack { get; protected set; }

    protected float _onDist = 0f;

    public float onDist
    {
        get { return _onDist; }
        set
        {
            if (onTrack.length < value)
            {
                if (onTrack.connectingNextTrack == -1)
                {
                    _onDist = onTrack.length;
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
                    Track oldTrack = onTrack;
                    onTrack = oldTrack.nextTracks[oldTrack.connectingNextTrack];
                    if ((oldTrack.getPoint(1f) - onTrack.pos).sqrMagnitude < Main.ALLOWABLE_RANGE && ((oldTrack is Curve ? ((Curve)oldTrack).getRotation(1f).eulerAngles : oldTrack.rot.eulerAngles) - onTrack.rot.eulerAngles).sqrMagnitude < Main.ALLOWABLE_RANGE)
                        onDist = value - oldTrack.length;
                    else
                    {
                        speed = -speed;
                        onDist = onTrack.length - value + oldTrack.length;
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
                    Track oldTrack = onTrack;
                    onTrack = oldTrack.prevTracks[oldTrack.connectingPrevTrack];
                    if ((oldTrack.pos - onTrack.getPoint(1f)).sqrMagnitude < Main.ALLOWABLE_RANGE && (oldTrack.rot.eulerAngles - (onTrack is Curve ? ((Curve)onTrack).getRotation(1f).eulerAngles : onTrack.rot.eulerAngles)).sqrMagnitude < Main.ALLOWABLE_RANGE)
                        onDist = onTrack.length + value;
                    else
                    {
                        speed = -speed;
                        onDist = -value;
                    }
                }
            }
            else
                _onDist = value;
        }
    }

    public float speed;

    public float wheelDia;
    public float tm_output;
    public float gearRatio;
    public float startingResistance;
    public float runningResistanceA;
    public float runningResistanceB;

    public float rotX;
    public BogieFrame bogieFrame;

    public GameObject modelObj;

    public float lastFixed = -1f;

    public Axle(Map map, Track onTrack, float onDist) : base(map)
    {
        this.onTrack = onTrack;
        this.onDist = onDist;
        speed = 0f;
        wheelDia = 0.86f;
        tm_output = 220f;
        gearRatio = 6.06f;
        startingResistance = 30f;
        runningResistanceA = 0.019890625f;
        runningResistanceB = 0.000015625f;
        rotX = 0f;
        Vector3 a = onTrack is Curve
            ? ((Curve)onTrack).getRotationCanted(onDist / onTrack.length).eulerAngles
            : onTrack.rot.eulerAngles;
        pos = (onTrack is Curve ? ((Curve)onTrack).getPointCanted(onDist / onTrack.length) : onTrack.getPoint(onDist / onTrack.length)) + Quaternion.Euler(a) * Vector3.up * wheelDia / 2f;
        a.x = rotX;
        rot = Quaternion.Euler(a);
    }

    protected Axle(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        onTrack = (Track)info.GetValue(KEY_ON_TRACK, typeof(Track));
        _onDist = info.GetSingle(KEY_ON_DIST);
        speed = info.GetSingle(KEY_SPEED);
        wheelDia = info.GetSingle(KEY_WHEEL_DIA);
        tm_output = info.GetSingle(KEY_TM_OUTPUT);
        gearRatio = info.GetSingle(KEY_GEAR_RATIO);
        startingResistance = info.GetSingle(KEY_STARTING_RESISTANCE);
        runningResistanceA = info.GetSingle(KEY_RUNNING_RESISTANCE_A);
        runningResistanceB = info.GetSingle(KEY_RUNNING_RESISTANCE_B);
        rotX = info.GetSingle(KEY_ROT_X);
        bogieFrame = (BogieFrame)info.GetValue(KEY_BOGIE_FRAME, typeof(BogieFrame));
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(KEY_ON_TRACK, onTrack);
        info.AddValue(KEY_ON_DIST, _onDist);
        info.AddValue(KEY_SPEED, speed);
        info.AddValue(KEY_WHEEL_DIA, wheelDia);
        info.AddValue(KEY_TM_OUTPUT, tm_output);
        info.AddValue(KEY_GEAR_RATIO, gearRatio);
        info.AddValue(KEY_STARTING_RESISTANCE, startingResistance);
        info.AddValue(KEY_RUNNING_RESISTANCE_A, runningResistanceA);
        info.AddValue(KEY_RUNNING_RESISTANCE_B, runningResistanceB);
        info.AddValue(KEY_ROT_X, rotX);
        info.AddValue(KEY_BOGIE_FRAME, bogieFrame);
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
        // Editor上で移動するテスト
        //SyncFromEntity();
        //reloadOnDist();
        //fixedMove();

        fixedMove();
        reloadEntity();
    }

    /// <summary>
    /// 車軸を移動する
    /// </summary>
    public void fixedMove()
    {
        if (lastFixed == Time.fixedTime)
            return;

        float w = 0f;
        if (bogieFrame != null)
        {
            var body = bogieFrame.body;
            if (body != null)
            {
                w = bogieFrame.body.carWeight;
                int d = 0;
                foreach (var bf in bogieFrame.body.bogieFrames)
                    d += bf.axles.Count;
                w /= d;
                float a = -Physics.gravity.y * runningResistanceA;
                a += runningResistanceB * speed;
                a *= Time.deltaTime * 36f / 100f;
                if (speed < 0f)
                    speed = Mathf.Min(speed + a, 0f);
                else
                    speed = Mathf.Max(speed - a, 0f);
            }
        }
        float b = speed * 10f * Time.deltaTime / 36f;
        onDist += b;
        rotX += b * 360f / Mathf.PI * wheelDia;
        lastFixed = Time.fixedTime;

        Vector3 c = onTrack is Shape ?
            ((Shape)onTrack).getRotationCanted(onDist / onTrack.length).eulerAngles :
            onTrack is Curve ?
            ((Curve)onTrack).getRotationCanted(onDist / onTrack.length).eulerAngles :
            onTrack.rot.eulerAngles;
        pos = (onTrack is Shape ?
            ((Shape)onTrack).getPointCanted(onDist / onTrack.length) :
            onTrack is Curve ?
            ((Curve)onTrack).getPointCanted(onDist / onTrack.length) :
            onTrack.getPoint(onDist / onTrack.length)) + Quaternion.Euler(c) * Vector3.up * wheelDia / 2f;
        rot = Quaternion.Euler(c);
    }

    /// <summary>
    /// 車軸を線路に合わせる
    /// </summary>
    public void reloadOnDist()
    {
        onDist = onTrack.getLength(pos);
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

        modelObj.transform.localEulerAngles = new Vector3(rotX, 0f);

        reloadMaterial(modelObj);

        base.reloadEntity();
    }

    public void reloadCollider()
    {
        BoxCollider collider = entity.GetComponent<BoxCollider>();
        if (collider == null)
            collider = entity.gameObject.AddComponent<BoxCollider>();
        collider.isTrigger = true;
        collider.size = new Vector3(COLLIDER_WIDTH, wheelDia, wheelDia);
    }

    public float getTrainLoad()
    {
        float w = 0;
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
        if (speed > 0f == a > 0f)
        {
            var b = a > 0f ? a : -a;
            b -= (1f - Mathf.Clamp(Mathf.Abs(speed), 0f, 3f) / 3f) * startingResistance * Time.deltaTime * 36f / 100000f / weight;
            if (a > 0f)
                speed += b;
            else
                speed -= b;
        }
        else
        {
            if (brake)
            {
                if (speed > 0f)
                    speed = Mathf.Max(speed + a, 0f);
                else
                    speed = Mathf.Min(speed + a, 0f);
            }
            else
                speed += a;
        }
    }
}
