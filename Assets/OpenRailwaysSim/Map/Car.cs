using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

[Serializable]
public class Car : MapObject {

	public const string KEY_LENGTH = "LENGTH";
    public const string KEY_NEXT_TRACKS = "NEXT_TRACKS";
    public const string KEY_PREV_TRACKS = "PREV_TRACKS";
    public const float MIN_TRACK_LENGTH = 1f;
    public const float RENDER_WIDTH = 1f;
    public const float COLLIDER_WIDTH = 2f;
    public const float COLLIDER_HEIGHT = 1f / 8;

    protected float _length = MIN_TRACK_LENGTH;
    public float length { get { return _length; } set { _length = Mathf.Max(MIN_TRACK_LENGTH, value); } }
    public bool enableCollider = true;
    public List<Track> nextTracks;
    public List<Track> prevTracks;

    public Car(Map map, Vector3 pos) : this(map, pos, new Quaternion())
    {
    }

    public Car(Map map, Vector3 pos, Quaternion rot) : base(map, pos, rot)
    {
        nextTracks = new List<Track>();
        prevTracks = new List<Track>();
    }

    protected Car(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        _length = info.GetSingle(KEY_LENGTH);
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(KEY_LENGTH, _length);
        info.AddValue(KEY_NEXT_TRACKS, nextTracks);
        info.AddValue(KEY_PREV_TRACKS, prevTracks);
    }

    public override void generate()
    {
        if (entity == null)
            (entity = new GameObject("track").AddComponent<MapEntity>()).init(this);
        else
            reloadEntity();
    }

    public override void reloadEntity()
    {
        if (entity == null)
            return;

        LineRenderer renderer = entity.GetComponent<LineRenderer>();
        if (renderer == null)
            renderer = entity.gameObject.AddComponent<LineRenderer>();
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        renderer.endWidth = renderer.startWidth = RENDER_WIDTH;
        renderer.endColor = renderer.startColor = Color.white;
        /*if (Main.main.selection == this)
            renderer.material = Main.main.selection_line_mat;
        else
            renderer.material = Main.main.line_mat;*/
        reloadLineRendererPositions(renderer);

        reloadCollider();

        base.reloadEntity();
    }

    public virtual void reloadLineRendererPositions(LineRenderer renderer)
    {
        renderer.SetPositions(new Vector3[] { pos, getPoint(1) });
    }

    public virtual void reloadCollider()
    {
        BoxCollider collider = entity.GetComponent<BoxCollider>();
        if (collider == null)
            collider = entity.gameObject.AddComponent<BoxCollider>();
        collider.isTrigger = true;
        collider.center = Vector3.forward * length / 2;
        collider.size = new Vector3(COLLIDER_WIDTH, COLLIDER_HEIGHT, length);
        collider.enabled = enableCollider;
    }

    public virtual Vector3 getPoint(float a)
    {
        return pos + rot * Vector3.forward * _length * a;
    }
}
