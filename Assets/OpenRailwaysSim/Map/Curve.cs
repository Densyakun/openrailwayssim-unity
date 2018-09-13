using System;
using System.Runtime.Serialization;
using UnityEngine;

//曲線
[Serializable]
public class Curve : Track
{
    public const string KEY_RADIUS = "RADIUS";
    public const float MIN_RADIUS = 1f;
    public const float FINENESS_DISTANCE = 5f;

    public override float length
    {
        get { return _length; }
        set { _length = Mathf.Max(MIN_TRACK_LENGTH, Mathf.Min(value, 2 * Mathf.PI * Mathf.Abs(radius))); }
    }

    private float _radius = MIN_RADIUS;
    public float radius
    {
        get
        {
            return _radius;
        }
        set
        {
            if (value > 0)
                _radius = Mathf.Max(MIN_RADIUS, value);
            else
                _radius = Mathf.Min(-MIN_RADIUS, value);
        }
    }
    public BoxCollider[] colliders = new BoxCollider[0];

    public Curve(Map map, Vector3 pos) : base(map, pos)
    {
    }

    public Curve(Map map, Vector3 pos, Quaternion rot) : base(map, pos, rot)
    {
    }

    protected Curve(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        _radius = info.GetSingle(KEY_RADIUS);
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(KEY_RADIUS, _radius);
    }

    public override void generate()
    {
        if (entity == null)
            (entity = new GameObject("curve").AddComponent<MapEntity>()).init(this);
        else
            reloadEntity();
    }

    public override void reloadTrackRendererPositions()
    {
        int l = Mathf.CeilToInt(_length / FINENESS_DISTANCE);
        Vector3[] p = new Vector3[l + 1];
        p[0] = pos;
        for (int a = 1; a <= l; a++)
            p[a] = getPoint((float)a / (float)l);
        trackRenderer.positionCount = p.Length;
        trackRenderer.SetPositions(p);
    }

    public override void reloadRailRenderers()
    {
        if (railRenderers != null)
            foreach (var r in railRenderers)
                GameObject.Destroy(r.gameObject);
        if (Main.main.showGuide)
        {
            railRenderers = new LineRenderer[rails.Count];
            for (int a = 0; a < rails.Count; a++)
            {
                GameObject o = new GameObject();
                railRenderers[a] = o.AddComponent<LineRenderer>();
                o.transform.parent = entity.transform;
                railRenderers[a].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                railRenderers[a].receiveShadows = false;
                railRenderers[a].endWidth = railRenderers[a].startWidth = RAIL_RENDER_WIDTH;
                railRenderers[a].endColor = railRenderers[a].startColor = Color.white;
                if (useSelectingMat)
                    railRenderers[a].sharedMaterial = Main.main.selecting_track_mat;
                else if (Main.focused == this)
                    railRenderers[a].sharedMaterial = Main.main.focused_track_mat;
                else
                    railRenderers[a].sharedMaterial = Main.main.rail_mat;

                int l = Mathf.CeilToInt(_length / FINENESS_DISTANCE);
                Vector3[] p = new Vector3[l + 1];
                p[0] = pos + rot * Vector3.right * rails[a];
                for (int b = 1; b <= l; b++)
                    p[b] = getPoint((float)b / (float)l) + getRotation((float)b / (float)l) * Vector3.right * rails[a];
                railRenderers[a].positionCount = p.Length;
                railRenderers[a].SetPositions(p);
            }
        }
        else
            railRenderers = null;
    }

    public override void reloadModels()
    {
        if (railModelObjs != null)
            foreach (var r in railModelObjs)
                GameObject.Destroy(r.gameObject);
        railModelObjs = new GameObject[Mathf.CeilToInt(length / RAIL_MODEL_INTERVAL)];
        for (int a = 0; a < railModelObjs.Length; a++)
        {
            (railModelObjs[a] = GameObject.Instantiate(Main.main.railModel)).transform.parent = entity.transform;
            setLOD(railModelObjs[a], 0.02f);
            var d = (float)a / railModelObjs.Length;
            var p = getPoint(d);
            var r_ = Quaternion.Inverse(rot);
            railModelObjs[a].transform.localPosition = r_ * (p - pos);
            railModelObjs[a].transform.localRotation = Quaternion.LookRotation(getPoint(((float)a + 1) / railModelObjs.Length) - p) * r_;
        }

        if (tieModelObjs != null)
            foreach (var r in tieModelObjs)
                GameObject.Destroy(r.gameObject);
        tieModelObjs = new GameObject[Mathf.CeilToInt(length / TIE_MODEL_INTERVAL)];
        for (int a = 0; a < tieModelObjs.Length; a++)
        {
            (tieModelObjs[a] = GameObject.Instantiate(Main.main.tieModel)).transform.parent = entity.transform;
            setLOD(tieModelObjs[a], 0.02f);
            var d = (float)a / tieModelObjs.Length;
            var r_ = Quaternion.Inverse(rot);
            tieModelObjs[a].transform.localPosition = r_ * (getPoint(d) - pos);
            tieModelObjs[a].transform.localRotation = getRotation(d) * r_;
        }
    }

    public override void reloadCollider()
    {
        int l = Mathf.CeilToInt(_length / FINENESS_DISTANCE);
        if (colliders.Length != l)
        {
            for (int a = 0; a < colliders.Length; a++)
                GameObject.Destroy(colliders[a].gameObject);
            colliders = new BoxCollider[l];
        }
        for (int a = 0; a < l; a++)
        {
            if (colliders[a] == null)
            {
                GameObject o = new GameObject();
                colliders[a] = o.AddComponent<BoxCollider>();
                o.transform.parent = entity.transform;
            }
            colliders[a].isTrigger = true;

            Quaternion b = Quaternion.Inverse(rot);
            Vector3 c = b * (getPoint((float)a / (float)l) - pos);
            Vector3 d = b * (getPoint(((float)a + 1) / (float)l) - pos);
            colliders[a].transform.localPosition = (c + d) / 2;
            colliders[a].transform.localRotation = b * getRotation(((float)a + 1f / 2) / (float)l);
            colliders[a].size = new Vector3(COLLIDER_WIDTH, COLLIDER_HEIGHT, Vector3.Distance(c, d));
            colliders[a].enabled = enableCollider;
        }
    }

    public override Vector3 getPoint(float a)
    {
        return pos + rot * (Vector3.right + Vector3.left * Mathf.Cos(_length * a / Mathf.Abs(_radius))) * _radius +
            rot * Vector3.forward * Mathf.Sin(_length * a / Mathf.Abs(_radius)) * Mathf.Abs(_radius);
    }

    public virtual Quaternion getRotation(float a)
    {
        return rot * Quaternion.Euler(new Vector3(0, _length * a * Mathf.Rad2Deg / _radius));
    }

    public virtual bool isLinear()
    {
        return length / radius <= Mathf.PI * 2 && rot == getRotation(1);
    }
}
