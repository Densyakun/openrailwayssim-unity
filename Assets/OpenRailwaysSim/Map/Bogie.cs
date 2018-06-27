using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

[Serializable]
public class Bogie : MapObject
{

    public const string KEY_ON_TRACK = "ON_TRACK";
    public const string KEY_ON_DIST = "ON_DIST";
    public const string KEY_SPEED = "SPEED";

    public const float RENDER_WIDTH = 2.5f;

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
                            ? ((Curve) oldTrack).getRotation(1)
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
                            ? ((Curve) oldTrack.prevTracks[oldTrack.connectingPrevTrack]).getRotation(1)
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

    public Bogie(Map map, Track onTrack, float onDist) : base(map, onTrack.getPoint(onDist / onTrack.length),
        onTrack is Curve ? ((Curve) onTrack).getRotation(onDist / onTrack.length) : onTrack.rot)
    {
        this.onTrack = onTrack;
        this.onDist = onDist;
        speed = 15;
    }

    protected Bogie(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        onTrack = (Track) info.GetValue(KEY_ON_TRACK, typeof(Track));
        _onDist = info.GetSingle(KEY_ON_DIST);
        speed = info.GetSingle(KEY_SPEED);
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(KEY_ON_TRACK, onTrack);
        info.AddValue(KEY_ON_DIST, _onDist);
        info.AddValue(KEY_SPEED, speed);
    }

    public override void generate()
    {
        if (entity == null)
            (entity = new GameObject("bogie").AddComponent<MapEntity>()).init(this);
        else
            reloadEntity();
    }

    public override void reloadEntity()
    {
        if (entity == null)
            return;

        pos = onTrack.getPoint(onDist / onTrack.length);
        rot = onTrack is Curve ? ((Curve) onTrack).getRotation(onDist / onTrack.length) : onTrack.rot;

        LineRenderer renderer = entity.GetComponent<LineRenderer>();
        if (renderer == null)
            renderer = entity.gameObject.AddComponent<LineRenderer>();
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        renderer.startWidth = RENDER_WIDTH;
        renderer.endWidth = 0;
        renderer.endColor = renderer.startColor = Color.white;
        /*if (Main.main.selection == this)
            renderer.material = Main.main.selection_line_mat;
        else*/
        renderer.material = Main.main.bogie_mat;
        renderer.SetPositions(new Vector3[] {pos, pos + rot * Vector3.forward * Mathf.Repeat(Time.time, 1)});

        reloadCollider();

        base.reloadEntity();
    }

    public virtual void reloadCollider()
    {
        /*BoxCollider collider = entity.GetComponent<BoxCollider>();
        if (collider == null)
            collider = entity.gameObject.AddComponent<BoxCollider>();
        collider.isTrigger = true;
        collider.center = Vector3.forward * length / 2;
        collider.size = new Vector3(COLLIDER_WIDTH, COLLIDER_HEIGHT, length);
        collider.enabled = enableCollider;*/
    }

    public virtual void fixedUpdate()
    {
        onDist += speed * 10 * Time.deltaTime / 36;
    }
}
