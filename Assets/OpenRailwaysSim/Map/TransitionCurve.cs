using System;
using System.Runtime.Serialization;
using UnityEngine;

//緩和曲線 (クロソイド曲線)
[Serializable]
public class TransitionCurve : Track
{

    public const string KEY_CURVATURE1 = "CURVATURE1";
    public const string KEY_CURVATURE2 = "CURVATURE2";
    public const string KEY_IS_VERTICAL_CURVE = "IS_VERTICAL_CURVE";
    public const float FINENESS_DISTANCE = 5f;

    private float _curvature1 = Curve.MIN_RADIUS;
    public float curvature1
    {
        get
        {
            return _curvature1;
        }
        set
        {
            if (value > 0)
                _curvature1 = Mathf.Max(Curve.MIN_RADIUS, value);
            else
                _curvature1 = Mathf.Min(-Curve.MIN_RADIUS, value);
        }
    }
    private float _curvature2 = Curve.MIN_RADIUS;
    public float curvature2
    {
        get
        {
            return _curvature2;
        }
        set
        {
            if (value > 0)
                _curvature2 = Mathf.Max(Curve.MIN_RADIUS, value);
            else
                _curvature2 = Mathf.Min(-Curve.MIN_RADIUS, value);
        }
    }
    public bool isVerticalCurve = false;
    public BoxCollider[] colliders = new BoxCollider[0];

    public TransitionCurve(Map map, Vector3 pos) : base(map, pos)
    {
    }

    public TransitionCurve(Map map, Vector3 pos, Quaternion rot) : base(map, pos, rot)
    {
    }

    protected TransitionCurve(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        _curvature1 = info.GetSingle(KEY_CURVATURE1);
        _curvature2 = info.GetSingle(KEY_CURVATURE2);
        isVerticalCurve = info.GetBoolean(KEY_IS_VERTICAL_CURVE);
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(KEY_CURVATURE1, _curvature1);
        info.AddValue(KEY_CURVATURE2, _curvature2);
        info.AddValue(KEY_IS_VERTICAL_CURVE, isVerticalCurve);
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
        if (!GameCanvas.runPanel.isShowing() && Main.main.showGuide)
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
        var r_ = Quaternion.Inverse(rot);
        railModelObjs = new GameObject[Mathf.CeilToInt(length / RAIL_MODEL_INTERVAL)];
        for (int a = 0; a < railModelObjs.Length; a++)
        {
            (railModelObjs[a] = GameObject.Instantiate(Main.main.railModel)).transform.parent = entity.transform;
            setLOD(railModelObjs[a], LOD_DISTANCE);
            var d = (float)a / railModelObjs.Length;
            var p = getPoint(d);
            railModelObjs[a].transform.localPosition = r_ * (p - pos);
            railModelObjs[a].transform.localRotation = r_ * Quaternion.LookRotation(getPoint(((float)a + 1) / railModelObjs.Length) - p);
        }

        if (tieModelObjs != null)
            foreach (var r in tieModelObjs)
                GameObject.Destroy(r.gameObject);
        tieModelObjs = new GameObject[Mathf.CeilToInt(length / TIE_MODEL_INTERVAL)];
        for (int a = 0; a < tieModelObjs.Length; a++)
        {
            (tieModelObjs[a] = GameObject.Instantiate(Main.main.tieModel)).transform.parent = entity.transform;
            setLOD(tieModelObjs[a], LOD_DISTANCE);
            var d = (float)a / tieModelObjs.Length;
            tieModelObjs[a].transform.localPosition = r_ * (getPoint(d) - pos);
            tieModelObjs[a].transform.localRotation = r_ * getRotation(d);
        }
    }

    public override void reloadCollider()
    {
        int l = Mathf.CeilToInt(_length / FINENESS_DISTANCE);
        if (colliders.Length != l)
        {
            for (int a = 0; a < colliders.Length; a++)
                if (colliders[a])
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
        var b = pos;
        var c = rot;

        var f = 3; // 分割数
        for (var e = 0; e < f && e / f < a; e++)
        {
            var d = Mathf.Max(_length / f, _length * (a - Mathf.Floor(f * a) / f));
            var r = 1 / getCurvature((e + 0.5f) / f);
            var d1 = d / Mathf.Abs(r);
            b += isVerticalCurve ? c * new Vector3(0f, (1f - Mathf.Cos(d1)) * r, Mathf.Sin(d1) * Mathf.Abs(r)) : Quaternion.Euler(0, c.eulerAngles.y, 0) * new Vector3((1f - Mathf.Cos(d1)) * r, Mathf.Sin(-c.eulerAngles.x * Mathf.Deg2Rad) * d, Mathf.Sin(d1) * Mathf.Abs(r));
            c *= getRotation(e / f);
        }

        return b;
    }

    public virtual Quaternion getRotation(float a)
    {
        var re = rot;

        var f = 3; // 分割数
        for (var e = 0; e < f && e / f < a; e++)
        {
            var d = Mathf.Max(_length / f, _length * (a - Mathf.Floor(f * a) / f));
            var r = 1 / getCurvature((e + 0.5f) / f);
            re *= isVerticalCurve ? Quaternion.Euler(-d * Mathf.Rad2Deg / r, 0, 0) : Quaternion.Euler(0, re.eulerAngles.y, 0) * Quaternion.Euler(re.eulerAngles.x, d * Mathf.Rad2Deg / r, 0);
        }

        return re;
    }

    public virtual float getCurvature(float a)
    {
        return Mathf.Lerp(curvature1, curvature2, a);
    }
}
