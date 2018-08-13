using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

[Serializable]
public class Axle : MapObject
{

    public const string KEY_ON_TRACK = "ON_TRACK";
    public const string KEY_ON_DIST = "ON_DIST";
    public const string KEY_SPEED = "SPEED";
    public const string KEY_WHEEL_DIA = "WHEEL_DIA";
    public const string KEY_TM_OUTPUT = "TM_OUTPUT";
    public const string KEY_GEAR_RATIO = "GEAR_RATIO";
    public const string KEY_ROT_X = "ROT_X";
    public const string KEY_BOGIE_FRAME = "BOGIE_FRAME";

    public const float COLLIDER_WIDTH = 2.3f;

    public Track onTrack { get; protected set; }

    protected float _onDist = 0;

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
                    speed = 0;
                }
                else
                {
                    Track oldTrack = onTrack;
                    onTrack = oldTrack.nextTracks[oldTrack.connectingNextTrack];
                    if ((oldTrack is Curve
                            ? ((Curve)oldTrack).getRotation(1)
                            : oldTrack.rot) == oldTrack.nextTracks[oldTrack.connectingNextTrack].rot)
                    {
                        onDist = value - oldTrack.length;
                        oldTrack = oldTrack.nextTracks[oldTrack.connectingNextTrack];
                    }
                    else
                    {
                        speed = -speed;
                        onDist = oldTrack.nextTracks[oldTrack.connectingNextTrack].length - value + oldTrack.length;
                    }
                }
            }
            else if (value < 0)
            {
                if (onTrack.connectingPrevTrack == -1)
                {
                    _onDist = 0;
                    speed = 0;
                }
                else
                {
                    Track oldTrack = onTrack;
                    onTrack = oldTrack.prevTracks[oldTrack.connectingPrevTrack];
                    if (oldTrack.rot == (oldTrack.prevTracks[oldTrack.connectingPrevTrack] is Curve
                            ? ((Curve)oldTrack.prevTracks[oldTrack.connectingPrevTrack]).getRotation(1)
                            : oldTrack.prevTracks[oldTrack.connectingPrevTrack].rot))
                    {
                        onDist = oldTrack.prevTracks[oldTrack.connectingPrevTrack].length + value;
                    }
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

    public float rotX;
    public BogieFrame bogieFrame;

    public GameObject modelObj;

    public float lastFixed = -1;

    public Axle(Map map, Track onTrack, float onDist) : base(map)
    {
        this.onTrack = onTrack;
        this.onDist = onDist;
        speed = 0;
        wheelDia = 0.86f;
        tm_output = 220;
        gearRatio = 6.06f;
        rotX = 0;
        Vector3 a = onTrack is Curve
            ? ((Curve)onTrack).getRotation(onDist / onTrack.length).eulerAngles
            : onTrack.rot.eulerAngles;
        pos = onTrack.getPoint(onDist / onTrack.length) + Quaternion.Euler(a) * Vector3.up * wheelDia / 2;
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
        info.AddValue(KEY_ROT_X, rotX);
        info.AddValue(KEY_BOGIE_FRAME, bogieFrame);
    }

    public override void generate()
    {
        if (entity == null)
            (entity = new GameObject("axle").AddComponent<MapEntity>()).init(this);
        else
            reloadEntity();
    }

    public override void update()
    {
        //Editor上で移動するテスト
        //SyncFromEntity();
        //reloadOnDist();
        //fixedMove();

        fixedMove();
        reloadEntity();
    }

    public void fixedMove()
    {
        if (lastFixed == Time.fixedTime)
            return;
        float a = speed * 10 * Time.deltaTime / 36;
        onDist += a;
        rotX += a * 360 / Mathf.PI * wheelDia;
        lastFixed = Time.fixedTime;

        Vector3 b = onTrack is Curve
            ? ((Curve)onTrack).getRotation(onDist / onTrack.length).eulerAngles
            : onTrack.rot.eulerAngles;
        pos = onTrack.getPoint(onDist / onTrack.length) + Quaternion.Euler(b) * Vector3.up * wheelDia / 2;
        rot = Quaternion.Euler(b);
    }

    public void reloadOnDist()
    {
        if (onTrack is Curve)
        {
            Vector3 a = Quaternion.Inverse(onTrack.rot) * (pos - onTrack.pos);
            float r1 = ((Curve)onTrack).radius;
            if (r1 < 0)
            {
                r1 = -r1;
                a.x = -a.x;
            }

            float r2 = Vector3.Distance(a, Vector3.right * r1);
            float A = Mathf.Atan(a.z / (r2 - a.x));
            if (A < 0)
            {
                if (a.z >= 0)
                    A += Mathf.PI;
            }
            else if (a.z < 0)
                A += Mathf.PI;
            onDist = A * r1;
            //float b = onDist - speed * 10 * Time.deltaTime / 36;
            //speed = ((onDist = A * r1) - b) * 36 / 10 / Time.deltaTime;
        }
        else
        {
            onDist = (Quaternion.Inverse(onTrack.rot) * (pos - onTrack.pos)).z;
            //float b = onDist - speed * 10 * Time.deltaTime / 36;
            //speed = ((onDist = (Quaternion.Inverse(onTrack.rot) * (pos - onTrack.pos)).z) - b) * 36 / 10 / Time.deltaTime;
        }
    }

    public override void reloadEntity()
    {
        if (entity == null)
            return;

        if (modelObj == null)
        {
            (modelObj = GameObject.Instantiate(Main.main.axleModel)).transform.parent = entity.transform;
            modelObj.transform.localPosition = Vector3.zero;
            reloadCollider();
        }

        modelObj.transform.localEulerAngles = new Vector3(rotX, 0);

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
            w = bogieFrame.body.weight;
            var a = bogieFrame.body;
            var b = bogieFrame.body.permanentCoupler1;
            while (b != null)
            {
                a = b.body1;
                w += a.weight;
                b = a.permanentCoupler1 == b ? a.permanentCoupler2 : a.permanentCoupler1;
            }
            a = bogieFrame.body;
            b = bogieFrame.body.permanentCoupler2;
            while (b != null)
            {
                a = b.body1;
                w += a.weight;
                b = a.permanentCoupler1 == b ? a.permanentCoupler2 : a.permanentCoupler1;
            }
        }
        return w;
    }
}
