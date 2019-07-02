using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

/// <summary>
/// 平面曲線と縦断曲線を設定する軌道。距離に勾配は関係しない
/// </summary>
[Serializable]
public class Shape : Track
{
    public const string KEY_CURVE_LENGTH = "CURVE_L";
    public const string KEY_CURVE_RADIUS = "CURVE_R";
    public const string KEY_VERTICAL_CURVE_LENGTH = "V_CURVE_L";
    public const string KEY_VERTICAL_CURVE_RADIUS = "V_CURVE_R";
    public const float MIN_RADIUS = 1f;
    public const float FINENESS_DISTANCE = 5f;

    public List<float> curveLength;
    public List<float> curveRadius;
    public List<float> verticalCurveLength;
    public List<float> verticalCurveRadius;
    public BoxCollider[] colliders = new BoxCollider[0];

    public Shape(Map map, Vector3 pos) : base(map, pos)
    {
        curveLength = new List<float>();
        curveRadius = new List<float>();
        verticalCurveLength = new List<float>();
        verticalCurveRadius = new List<float>();
    }

    public Shape(Map map, Vector3 pos, Quaternion rot) : base(map, pos, rot)
    {
        curveLength = new List<float>();
        curveRadius = new List<float>();
        verticalCurveLength = new List<float>();
        verticalCurveRadius = new List<float>();
    }

    protected Shape(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        curveLength = (List<float>)info.GetValue(KEY_CURVE_LENGTH, typeof(List<float>));
        curveRadius = (List<float>)info.GetValue(KEY_CURVE_RADIUS, typeof(List<float>));
        verticalCurveLength = (List<float>)info.GetValue(KEY_VERTICAL_CURVE_LENGTH, typeof(List<float>));
        verticalCurveRadius = (List<float>)info.GetValue(KEY_VERTICAL_CURVE_RADIUS, typeof(List<float>));
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(KEY_CURVE_LENGTH, curveLength);
        info.AddValue(KEY_CURVE_RADIUS, curveRadius);
        info.AddValue(KEY_VERTICAL_CURVE_LENGTH, verticalCurveLength);
        info.AddValue(KEY_VERTICAL_CURVE_RADIUS, verticalCurveRadius);
    }

    public override void generate()
    {
        if (entity == null)
            (entity = new GameObject("shape").AddComponent<MapEntity>()).init(this);
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
            var f = getPoint(((float)a + 1) / railModelObjs.Length) - p;
            if (f.sqrMagnitude != 0f)
                railModelObjs[a].transform.localRotation = r_ * Quaternion.LookRotation(f);
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

    // TODO 長さに勾配を考慮する（TrackSettingPanelも）
    public override Vector3 getPoint(float a)
    {
        var p = pos;
        var r = rot;

        // 平面曲線を計算
        var rad = 0f;
        var l = 0f;
        bool b = true;
        float c;
        float _l;
        for (var n = 0; n < curveLength.Count; n++)
        {
            b = _length * a <= l + curveLength[n];
            c = (b ? (_length * a - l) / curveLength[n] : 1f);
            _l = curveLength[n] * c;
            if ((rad = curveRadius[n]) == 0f)
                p += r * Vector3.forward * _l;
            else
            {
                var d = _l;
                var d1 = d / Mathf.Abs(rad);
                p += Quaternion.Euler(0f, r.eulerAngles.y, 0f) * new Vector3((1f - Mathf.Cos(d1)) * rad, Mathf.Sin(-r.eulerAngles.x * Mathf.Deg2Rad) * d, Mathf.Sin(d1) * Mathf.Abs(rad));
                r = Quaternion.Euler(0f, r.eulerAngles.y, 0f) * Quaternion.Euler(r.eulerAngles.x, curveLength[n] * c * Mathf.Rad2Deg / rad, 0f);
            }
            l += _l;

            if (b)
                break;
        }

        // 縦断曲線を計算
        var vr = rot;
        rad = 0f;
        l = 0f;
        for (var n = 0; n < verticalCurveLength.Count; n++)
        {
            b = _length * a <= l + verticalCurveLength[n];
            c = (b ? (_length * a - l) / verticalCurveLength[n] : 1f);
            _l = verticalCurveLength[n] * c;
            if ((rad = verticalCurveRadius[n]) == 0f)
                p.y += (vr * Vector3.forward * _l).y;
            else
            {
                var d = _l;
                var d1 = d / Mathf.Abs(rad);
                p.y += (vr * new Vector3(0f, (1f - Mathf.Cos(d1)) * rad, Mathf.Sin(d1) * Mathf.Abs(rad))).y;
                vr *= Quaternion.Euler(-verticalCurveLength[n] * c * Mathf.Rad2Deg / rad, 0f, 0f);
            }
            l += _l;

            if (b)
                break;
        }
        if (!b)
            p.y += (vr * Vector3.forward * (_length * a - l)).y;

        return p;
    }

    public virtual Quaternion getRotation(float a)
    {
        var r = rot;

        // 平面曲線を計算
        var rad = 0f;
        var l = 0f;
        bool b;
        float c;
        float _l;
        for (var n = 0; n < curveLength.Count; n++)
        {
            b = _length * a <= l + curveLength[n];
            c = (b ? (_length * a - l) / curveLength[n] : 1f);
            _l = curveLength[n] * c;
            if ((rad = curveRadius[n]) != 0f)
                r = Quaternion.Euler(0f, r.eulerAngles.y, 0f) * Quaternion.Euler(r.eulerAngles.x, _l * Mathf.Rad2Deg / rad, 0f);
            l += _l;

            if (b)
                break;
        }

        // 縦断曲線を計算
        var vr = rot;
        rad = 0f;
        l = 0f;
        for (var n = 0; n < verticalCurveLength.Count; n++)
        {
            b = _length * a <= l + verticalCurveLength[n];
            c = (b ? (_length * a - l) / verticalCurveLength[n] : 1f);
            _l = verticalCurveLength[n] * c;
            if ((rad = verticalCurveRadius[n]) != 0f)
                vr *= Quaternion.Euler(-_l * Mathf.Rad2Deg / rad, 0f, 0f);
            l += _l;

            if (b)
                break;
        }

        return Quaternion.Euler(0f, r.eulerAngles.y, 0f) * Quaternion.Euler(vr.eulerAngles.x, 0f, 0f);
    }
}
