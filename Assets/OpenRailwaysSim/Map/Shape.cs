using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;

/// <summary>
/// 平面曲線と縦断曲線を設定する軌道
/// - 同一Shapeで平面交差してはいけない。
/// </summary>
[Serializable]
public class Shape : Track
{

    public const string KEY_CURVE_LENGTH = "CURVE_L";
    public const string KEY_CURVE_RADIUS = "CURVE_R";
    public const string KEY_CANT = "C";
    public const string KEY_CANT_ROTATION = "C_R";
    public const string KEY_VERTICAL_CURVE_LENGTH = "V_CURVE_L";
    public const string KEY_VERTICAL_CURVE_RADIUS = "V_CURVE_R";
    public const float FINENESS_DISTANCE = 5f;

    public List<float> curveLength;
    public List<float> curveRadius;
    public List<float> cant;
    public List<bool> cantRotation;
    public List<float> verticalCurveLength;
    public List<float> verticalCurveRadius;

    public float flatLength;
    public BoxCollider[] colliders = new BoxCollider[0];

    public Shape(Map map, Vector3 pos) : base(map, pos)
    {
        curveLength = new List<float>();
        curveRadius = new List<float>();
        cant = new List<float>();
        cantRotation = new List<bool>();
        verticalCurveLength = new List<float>();
        verticalCurveRadius = new List<float>();
        flatLength = 0f;
    }

    public Shape(Map map, Vector3 pos, Quaternion rot) : base(map, pos, rot)
    {
        curveLength = new List<float>();
        curveRadius = new List<float>();
        cant = new List<float>();
        cantRotation = new List<bool>();
        verticalCurveLength = new List<float>();
        verticalCurveRadius = new List<float>();
        flatLength = 0f;
    }

    protected Shape(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        curveLength = (List<float>)info.GetValue(KEY_CURVE_LENGTH, typeof(List<float>));
        curveRadius = (List<float>)info.GetValue(KEY_CURVE_RADIUS, typeof(List<float>));
        cant = (List<float>)info.GetValue(KEY_CANT, typeof(List<float>));
        cantRotation = (List<bool>)info.GetValue(KEY_CANT_ROTATION, typeof(List<bool>));
        verticalCurveLength = (List<float>)info.GetValue(KEY_VERTICAL_CURVE_LENGTH, typeof(List<float>));
        verticalCurveRadius = (List<float>)info.GetValue(KEY_VERTICAL_CURVE_RADIUS, typeof(List<float>));
        flatLength = curveLength.Sum();
        reloadLength();
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(KEY_CURVE_LENGTH, curveLength);
        info.AddValue(KEY_CURVE_RADIUS, curveRadius);
        info.AddValue(KEY_CANT, cant);
        info.AddValue(KEY_CANT_ROTATION, cantRotation);
        info.AddValue(KEY_VERTICAL_CURVE_LENGTH, verticalCurveLength);
        info.AddValue(KEY_VERTICAL_CURVE_RADIUS, verticalCurveRadius);
    }

    public override void generate()
    {
        if (entity)
            reloadEntity();
        else
            (entity = new GameObject("Shape").AddComponent<MapEntity>()).init(this);
    }

    public override void reloadTrackRendererPositions()
    {
        var l = Mathf.CeilToInt(length / FINENESS_DISTANCE);
        var p = new Vector3[l + 1];
        p[0] = pos;
        for (var a = 1; a <= l; a++)
            p[a] = getPoint((float)a / l);
        trackRenderer.positionCount = p.Length;
        trackRenderer.SetPositions(p);
    }

    public override void reloadRailRenderers()
    {
        if (railRenderers != null)
            foreach (var r in railRenderers)
                GameObject.Destroy(r.gameObject);
        if (Main.INSTANCE.grid.activeSelf)
        {
            railRenderers = new LineRenderer[2];
            for (var a = 0; a < 2; a++)
            {
                var o = new GameObject();
                railRenderers[a] = o.AddComponent<LineRenderer>();
                o.transform.parent = entity.transform;
                railRenderers[a].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                railRenderers[a].receiveShadows = false;
                railRenderers[a].endWidth = railRenderers[a].startWidth = RAIL_RENDER_WIDTH;
                railRenderers[a].endColor = railRenderers[a].startColor = Color.white;
                if (useSelectingMat)
                    railRenderers[a].sharedMaterial = Main.INSTANCE.selectingMat;
                else if (Main.focused == this)
                    railRenderers[a].sharedMaterial = Main.INSTANCE.focusedMat;
                else
                    railRenderers[a].sharedMaterial = Main.INSTANCE.railMat;

                var l = Mathf.CeilToInt(length / FINENESS_DISTANCE);
                var p = new Vector3[l + 1];
                for (var b = 0; b <= l; b++)
                    p[b] = getPointCanted((float)b / l) + getRotationCanted((float)b / l) * Vector3.right * (a == 0 ? -gauge / 2f : gauge / 2f);
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
        railModelObjs = new GameObject[Mathf.CeilToInt(length / RAIL_MODEL_INTERVAL) * 2];
        GameObject b;
        for (var a = 0; a < railModelObjs.Length / 2; a++)
        {
            railModelObjs[a] = b = GameObject.Instantiate(Main.INSTANCE.railLModel);
            b.transform.parent = entity.transform;
            setLOD(b, LOD_DISTANCE);
            var d = a / (railModelObjs.Length / 2f);
            var p = getPointCanted(d);
            b.transform.localPosition = r_ * (p - pos);
            var f = getPointCanted((a + 1f) / (railModelObjs.Length / 2f)) - p;
            if (f.sqrMagnitude != 0f)
                b.transform.localRotation = r_ * Quaternion.LookRotation(f) * Quaternion.Euler(0f, 0f, -Mathf.Asin(getCant(a / (railModelObjs.Length / 2f)) / gauge) * Mathf.Rad2Deg);
        }
        for (var a = 0; a < railModelObjs.Length / 2; a++)
        {
            railModelObjs[a + railModelObjs.Length / 2] = b = GameObject.Instantiate(Main.INSTANCE.railRModel);
            b.transform.parent = entity.transform;
            setLOD(b, LOD_DISTANCE);
            var d = a / (railModelObjs.Length / 2f);
            var p = getPointCanted(d);
            b.transform.localPosition = r_ * (p - pos);
            var f = getPointCanted((a + 1f) / (railModelObjs.Length / 2f)) - p;
            if (f.sqrMagnitude != 0f)
                b.transform.localRotation = r_ * Quaternion.LookRotation(f) * Quaternion.Euler(0f, 0f, -Mathf.Asin(getCant(a / (railModelObjs.Length / 2f)) / gauge) * Mathf.Rad2Deg);
        }

        if (tieModelObjs != null)
            foreach (var r in tieModelObjs)
                GameObject.Destroy(r.gameObject);
        tieModelObjs = new GameObject[Mathf.CeilToInt(length / TIE_MODEL_INTERVAL)];
        for (var a = 0; a < tieModelObjs.Length; a++)
        {
            (tieModelObjs[a] = GameObject.Instantiate(Main.INSTANCE.tieModel)).transform.parent = entity.transform;
            setLOD(tieModelObjs[a], LOD_DISTANCE);
            var d = (float)a / tieModelObjs.Length;
            tieModelObjs[a].transform.localPosition = r_ * (getPointCanted(d) - pos);
            tieModelObjs[a].transform.localRotation = r_ * getRotationCanted(d);
        }
    }

    public override void reloadCollider()
    {
        var l = Mathf.CeilToInt(length / FINENESS_DISTANCE);
        if (colliders.Length != l)
        {
            for (var a = 0; a < colliders.Length; a++)
                if (colliders[a])
                    GameObject.Destroy(colliders[a].gameObject);
            colliders = new BoxCollider[l];
        }
        for (var a = 0; a < l; a++)
        {
            if (colliders[a] == null)
            {
                GameObject o = new GameObject();
                colliders[a] = o.AddComponent<BoxCollider>();
                o.transform.parent = entity.transform;
            }
            colliders[a].isTrigger = true;

            Quaternion b = Quaternion.Inverse(rot);
            Vector3 c = b * (getPointCanted((float)a / l) - pos);
            Vector3 d = b * (getPointCanted((a + 1f) / l) - pos);
            colliders[a].transform.localPosition = (c + d) / 2f;
            colliders[a].transform.localRotation = b * getRotationCanted((a + 0.5f) / l);
            colliders[a].size = new Vector3(COLLIDER_WIDTH, COLLIDER_HEIGHT, Vector3.Distance(c, d));
            colliders[a].enabled = enableCollider;
        }
    }

    /// <summary>
    /// 軌道上の位置(0-1)から座標を返す
    /// </summary>
    /// <param name="a">位置(0-1)</param>
    public override Vector3 getPoint(float a)
    {
        return getPointFlat(LengthToFlatLength(a * length) / flatLength);
    }

    /// <summary>
    /// 軌道上の位置(0-1)の座標をカント付きで返す
    /// </summary>
    /// <param name="a">位置(0-1)</param>
    public virtual Vector3 getPointCanted(float a)
    {
        var l = LengthToFlatLength(a * length);
        var c = 0f;
        var l1 = 0f;
        for (var n = 0; n < curveLength.Count; n++)
        {
            if (l1 <= l && !cantRotation[n])
                c = cant[n] / 2f;
            l1 += curveLength[n];
        }

        return getPointFlat(l / l1) + Vector3.up * c;
    }

    /// <summary>
    /// 平面における位置(0-1)から座標を返す
    /// </summary>
    /// <param name="a">平面における位置(0-1)</param>
    public Vector3 getPointFlat(float a)
    {
        var p = pos;
        var r = rot;

        // 平面曲線を計算
        var rad = 0f;
        var l2 = 0f;
        bool b;
        float l3;
        for (var n = 0; n < curveLength.Count; n++)
        {
            b = flatLength * a < l2 + curveLength[n];
            l3 = curveLength[n] * (b ? (flatLength * a - l2) / curveLength[n] : 1f);
            if ((rad = curveRadius[n]) == 0f)
                p += Quaternion.Euler(0f, r.eulerAngles.y, 0f) * Vector3.forward * l3;
            else
            {
                var d1 = l3 / Mathf.Abs(rad);
                p += Quaternion.Euler(0f, r.eulerAngles.y, 0f) * new Vector3((1f - Mathf.Cos(d1)) * rad, Mathf.Sin(-r.eulerAngles.x * Mathf.Deg2Rad) * l3, Mathf.Sin(d1) * Mathf.Abs(rad));
                r = Quaternion.Euler(0f, r.eulerAngles.y, 0f) * Quaternion.Euler(r.eulerAngles.x, l3 * Mathf.Rad2Deg / rad, 0f);
            }
            l2 += l3;

            if (b)
                break;
        }

        // 縦断曲線を計算
        r = rot;
        rad = 0f;
        l2 = 0f;
        b = false;
        for (var n = 0; n < verticalCurveLength.Count; n++)
        {
            b = flatLength * a < l2 + verticalCurveLength[n];
            l3 = verticalCurveLength[n] * (b ? (flatLength * a - l2) / verticalCurveLength[n] : 1f);
            if ((rad = verticalCurveRadius[n]) == 0f)
                p.y += Mathf.Tan(-r.eulerAngles.x * Mathf.Deg2Rad) * l3;
            else
            {
                var t = -r.eulerAngles.x * Mathf.Deg2Rad; // 縦曲線始点の角度。負数なら始点は下り勾配
                var l4 = Mathf.Tan(t) * rad; // 曲線中心を基準とした縦曲線始点の横位置
                var l5 = l4 + l3; // 曲線中心を基準とした縦曲線終点の横位置

                var r1 = Mathf.Abs(rad);
                var h = Mathf.Sqrt(r1 * r1 - l4 * l4); // 曲線中心と縦曲線始点の高低差の絶対値
                if (!float.IsNaN(h))
                {
                    var h1 = Mathf.Sqrt(r1 * r1 - l5 * l5); // 曲線中心と縦曲線終点の高低差の絶対値
                    if (!float.IsNaN(h1))
                    {
                        p.y += rad < 0f ? h1 - h : h - h1;

                        var t1 = Mathf.Asin(l5 / rad); // 曲線中心から縦曲線終点までの角度
                        r *= Quaternion.Euler(-(t1 - t) * Mathf.Rad2Deg, 0f, 0f);
                    }
                }
            }
            l2 += l3;

            if (b)
                break;
        }
        if (!b)
            p.y += Mathf.Tan(-r.eulerAngles.x * Mathf.Deg2Rad) * (flatLength * a - l2);

        return p;
    }

    /// <summary>
    /// 軌道上の位置(0-1)から回転を返す
    /// </summary>
    /// <param name="a">位置(0-1)</param>
    public override Quaternion getRotation(float a)
    {
        return getRotationFlat(LengthToFlatLength(a * length) / flatLength);
    }

    /// <summary>
    /// 軌道上の位置(0-1)の回転をカント付きで返す
    /// </summary>
    /// <param name="a">位置(0-1)</param>
    public virtual Quaternion getRotationCanted(float a)
    {
        var l = LengthToFlatLength(a * length);
        var c = 0f;
        var l1 = 0f;
        for (var n = 0; n < curveLength.Count; n++)
        {
            if (l1 <= l)
                c = cant[n];
            l1 += curveLength[n];
        }

        var r = getRotationFlat(l / l1);
        return Quaternion.Euler(r.eulerAngles.x, r.eulerAngles.y, -Mathf.Asin(c / gauge) * Mathf.Rad2Deg);
    }

    /// <summary>
    /// 平面における位置(0-1)から回転を返す
    /// </summary>
    /// <param name="a">平面における位置(0-1)</param>
    public Quaternion getRotationFlat(float a)
    {
        var r = rot;

        // 平面曲線を計算
        var rad = 0f;
        var l2 = 0f;
        bool b;
        float l3;
        for (var n = 0; n < curveLength.Count; n++)
        {
            b = flatLength * a < l2 + curveLength[n];
            l3 = curveLength[n] * (b ? (flatLength * a - l2) / curveLength[n] : 1f);
            if ((rad = curveRadius[n]) != 0f)
                r = Quaternion.Euler(0f, r.eulerAngles.y, 0f) * Quaternion.Euler(r.eulerAngles.x, l3 * Mathf.Rad2Deg / rad, 0f);
            l2 += l3;

            if (b)
                break;
        }

        // 縦断曲線を計算
        var vr = rot;
        rad = 0f;
        l2 = 0f;
        for (var n = 0; n < verticalCurveLength.Count; n++)
        {
            b = flatLength * a < l2 + verticalCurveLength[n];
            l3 = verticalCurveLength[n] * (b ? (flatLength * a - l2) / verticalCurveLength[n] : 1f);
            if ((rad = verticalCurveRadius[n]) != 0f)
            {
                var t = -vr.eulerAngles.x * Mathf.Deg2Rad;
                var l5 = Mathf.Tan(t) * rad + l3;

                var f = l5 / rad;
                if (-1f <= f && f <= 1f)
                {
                    var t1 = Mathf.Asin(l5 / rad);
                    vr *= Quaternion.Euler(-(t1 - t) * Mathf.Rad2Deg, 0f, 0f);
                }
            }
            l2 += l3;

            if (b)
                break;
        }

        return Quaternion.Euler(0f, r.eulerAngles.y, 0f) * Quaternion.Euler(vr.eulerAngles.x, 0f, 0f);
    }

    /// <summary>
    /// 座標から軌道上の位置を求める
    /// </summary>
    /// <param name="pos">座標</param>
    public override float getLength(Vector3 pos)
    {
        return FlatLengthToLength(getLengthFlat(pos));
    }

    /// <summary>
    /// 座標から平面における位置を求める
    /// </summary>
    /// <param name="pos">座標</param>
    public float getLengthFlat(Vector3 pos)
    {
        var c = new List<Vector3>();
        var d = new List<float>();
        var p = this.pos;
        var r = rot;
        var l = 0f;

        for (var n = 0; n < curveLength.Count; n++)
        {
            var r1 = Quaternion.Inverse(r);

            var rad = curveRadius[n];
            if (rad == 0f)
            {
                var l3 = Mathf.Clamp((r1 * (pos - p)).z, 0f, curveLength[n]);
                c.Add(p + Quaternion.Euler(0f, r.eulerAngles.y, 0f) * Vector3.forward * l3);
                d.Add(l + l3);
                p += Quaternion.Euler(0f, r.eulerAngles.y, 0f) * Vector3.forward * curveLength[n];
            }
            else
            {
                var a = r1 * (pos - p);
                var b = r1 * (getPointFlat((l + curveLength[n]) / flatLength) - p);
                if (rad < 0f)
                {
                    rad = -rad;
                    a.x = -a.x;
                    b.x = -b.x;
                }
                var A = Mathf.Atan(a.z / (rad - a.x));
                if (A != 0f)
                {
                    var A1 = Mathf.Atan(b.z / (rad - b.x));
                    if (A1 != 0f)
                    {
                        if (A1 < 0f)
                            A1 = Mathf.PI + A1;
                        if (b.z < 0f)
                            A1 += Mathf.PI;

                        if (A < 0f)
                            A = Mathf.PI + A;
                        if (a.z < 0f)
                            A += Mathf.PI;

                        var l3 = Mathf.Clamp(curveLength[n] * A / A1, 0f, curveLength[n]);
                        var d1 = l3 / rad;
                        var d2 = curveLength[n] / rad;
                        c.Add(p + Quaternion.Euler(0f, r.eulerAngles.y, 0f) * new Vector3((1f - Mathf.Cos(d1)) * curveRadius[n], Mathf.Sin(-r.eulerAngles.x * Mathf.Deg2Rad) * l3, Mathf.Sin(d1) * rad));
                        d.Add(l + l3);

                        p += Quaternion.Euler(0f, r.eulerAngles.y, 0f) * new Vector3((1f - Mathf.Cos(d2)) * curveRadius[n], Mathf.Sin(-r.eulerAngles.x * Mathf.Deg2Rad) * curveLength[n], Mathf.Sin(d2) * rad);
                        r = Quaternion.Euler(0f, r.eulerAngles.y, 0f) * Quaternion.Euler(r.eulerAngles.x, curveLength[n] * Mathf.Rad2Deg / curveRadius[n], 0f);
                    }
                }
            }
            l += curveLength[n];
        }

        if (d.Count != 0)
        {
            var d1 = (pos - c[0]).magnitude;
            var n = 0;
            for (var n1 = 1; n1 < c.Count; n1++)
            {
                var d2 = (pos - c[n1]).magnitude;
                if (d2 < d1)
                {
                    d1 = d2;
                    n = n1;
                }
            }
            return d[n];
        }

        return l;
    }

    /// <summary>
    /// 軌道上の位置(0-1)からカントを返す
    /// </summary>
    /// <param name="a">位置(0-1)</param>
    public float getCant(float a)
    {
        return getCantFlat(LengthToFlatLength(a * length) / flatLength);
    }

    /// <summary>
    /// 平面における位置(0-1)からカントを返す
    /// </summary>
    /// <param name="a">平面における位置(0-1)</param>
    public float getCantFlat(float a)
    {
        var l2 = 0f;
        for (var n = 0; n < curveLength.Count; n++)
        {
            if (flatLength * a <= l2 + curveLength[n])
                return cant[n];
            l2 += curveLength[n];
        }

        return 0f;
    }

    /// <summary>
    /// 軌道上の位置を平面における位置に変換する
    /// </summary>
    /// <param name="l">軌道上の位置</param>
    public float LengthToFlatLength(float l)
    {
        var l1 = 0f;

        var vr = rot;
        var rad = 0f;
        var l2 = 0f;
        var b = false;
        float l3;
        float l6;
        for (var n = 0; n < verticalCurveLength.Count; n++)
        {
            l6 = 0f;
            if ((rad = verticalCurveRadius[n]) == 0f)
                l6 = verticalCurveLength[n] * Mathf.Cos(vr.eulerAngles.x * Mathf.Deg2Rad);
            else
            {
                var t = -vr.eulerAngles.x * Mathf.Deg2Rad;
                var l5 = Mathf.Tan(t) * rad + verticalCurveLength[n];

                var f = l5 / rad;
                if (-1f <= f && f <= 1f)
                {
                    var r1 = Mathf.Abs(rad);
                    var t1 = Mathf.Asin(l5 / r1);
                    l6 = Mathf.Abs(Mathf.Repeat(Mathf.Abs(t1 - t) + Mathf.PI / 2f, Mathf.PI) - Mathf.PI / 2f) * r1;
                }
            }

            b = l < l2 + l6;
            l3 = l6 * (b ? (l - l2) / l6 : 1f);
            if ((rad = verticalCurveRadius[n]) == 0f)
                l1 += l3 * Mathf.Cos(vr.eulerAngles.x * Mathf.Deg2Rad);
            else
            {
                var t = -vr.eulerAngles.x * Mathf.Deg2Rad;
                var l5 = Mathf.Tan(t) * rad + l3 * verticalCurveLength[n] / l6;

                var f = l5 / rad;
                if (-1f <= f && f <= 1f)
                {
                    var r1 = Mathf.Abs(rad);
                    var t1 = Mathf.Asin(l5 / r1);
                    vr *= Quaternion.Euler(-(t1 - t) * Mathf.Rad2Deg, 0f, 0f);
                    l1 += Mathf.Sin(Mathf.Abs(t1 - t)) * r1;
                }
            }
            l2 += l3;

            if (b)
                break;
        }
        if (!b)
            l1 += (l - l2) * Mathf.Cos(vr.eulerAngles.x * Mathf.Deg2Rad);

        return l1;
    }

    /// <summary>
    /// 平面における位置を軌道上の位置に変換する
    /// </summary>
    /// <param name="l">平面における位置</param>
    public float FlatLengthToLength(float l)
    {
        var l1 = 0f;

        var vr = rot;
        var rad = 0f;
        var l2 = 0f;
        var b = false;
        float l3;
        for (var n = 0; n < verticalCurveLength.Count; n++)
        {
            b = l < l2 + verticalCurveLength[n];
            l3 = verticalCurveLength[n] * (b ? (l - l2) / verticalCurveLength[n] : 1f);
            if ((rad = verticalCurveRadius[n]) == 0f)
                l1 += l3 / Mathf.Cos(vr.eulerAngles.x * Mathf.Deg2Rad);
            else
            {
                var t = -vr.eulerAngles.x * Mathf.Deg2Rad;
                var l5 = Mathf.Tan(t) * rad + l3;

                var f = l5 / rad;
                if (-1f <= f && f <= 1f)
                {
                    var r1 = Mathf.Abs(rad);
                    var t1 = Mathf.Asin(l5 / r1);
                    vr *= Quaternion.Euler(-(t1 - t) * Mathf.Rad2Deg, 0f, 0f);
                    l1 += Mathf.Abs(Mathf.Repeat(Mathf.Abs(t1 - t) + Mathf.PI / 2f, Mathf.PI) - Mathf.PI / 2f) * r1;
                }
            }
            l2 += l3;

            if (b)
                break;
        }
        if (!b)
            l1 += (l - l2) / Mathf.Cos(vr.eulerAngles.x * Mathf.Deg2Rad);

        return l1;
    }

    /// <summary>
    /// 軌道の実際の長さを更新する
    /// </summary>
    public void reloadLength()
    {
        flatLength = curveLength.Sum();

        var l = 0f;

        var vr = rot;
        var rad = 0f;
        var l2 = 0f;
        for (var n = 0; n < verticalCurveLength.Count; n++)
        {
            if ((rad = verticalCurveRadius[n]) == 0f)
                l += verticalCurveLength[n] / Mathf.Cos(vr.eulerAngles.x * Mathf.Deg2Rad);
            else
            {
                var t = -vr.eulerAngles.x * Mathf.Deg2Rad;
                var l5 = Mathf.Tan(t) * rad + verticalCurveLength[n];

                var f = l5 / rad;
                if (-1f <= f && f <= 1f)
                {
                    var r1 = Mathf.Abs(rad);
                    var t1 = Mathf.Asin(l5 / r1);
                    vr *= Quaternion.Euler(-(t1 - t) * Mathf.Rad2Deg, 0f, 0f);
                    l += Mathf.Abs(Mathf.Repeat(Mathf.Abs(t1 - t) + Mathf.PI / 2f, Mathf.PI) - Mathf.PI / 2f) * r1;
                }
            }
            l2 += verticalCurveLength[n];
        }

        l += (flatLength - l2) / Mathf.Cos(vr.eulerAngles.x * Mathf.Deg2Rad);
        length = l;
    }
}
