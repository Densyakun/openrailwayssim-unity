using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

[Serializable]
public class Car : MapObject {

	public const string KEY_ON_TRACK = "ON_TRACK";
    public const string KEY_CAR_POS = "CAR_POS";

    public Track onTrack { get; protected set; }
    
    protected float _car_pos = 0;

    public float car_pos
    {
        get { return _car_pos; }
        set { _car_pos = Mathf.Min(onTrack.length, Mathf.Max(0, value)); }
    }

    public Car(Map map, Vector3 pos) : this(map, pos, new Quaternion())
    {
    }

    public Car(Map map, Vector3 pos, Quaternion rot) : base(map, pos, rot)
    {
    }

    protected Car(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        onTrack = (Track) info.GetValue(KEY_ON_TRACK, typeof(Track));
        _car_pos = info.GetSingle(KEY_CAR_POS);
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(KEY_ON_TRACK, onTrack);
        info.AddValue(KEY_CAR_POS, _car_pos);
    }

    public override void generate()
    {
        if (entity == null)
            (entity = new GameObject("car").AddComponent<MapEntity>()).init(this);
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
        renderer.endWidth = renderer.startWidth = 5;
        renderer.endColor = renderer.startColor = Color.white;
        /*if (Main.main.selection == this)
            renderer.material = Main.main.selection_line_mat;
        else
            renderer.material = Main.main.line_mat;*/
        renderer.SetPositions(new Vector3[] {pos,});

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
}
