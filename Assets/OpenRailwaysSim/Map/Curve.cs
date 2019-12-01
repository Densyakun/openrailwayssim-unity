using System;
using System.Runtime.Serialization;
using UnityEngine;

/// <summary>
/// 曲線（現在未使用）
/// </summary>
[Serializable]
public class Curve : Track
{

    public const string KEY_RADIUS = "R";
    public const string KEY_IS_VERTICAL_CURVE = "IS_V_C";
    public const string KEY_CANT = "C";
    public const string KEY_CANT_ROTATION = "C_R";
    public const float FINENESS_DISTANCE = 5f;

    public float radius;
    public bool isVerticalCurve = false;
    public float cant = 0f;
    public bool cantRotation = false;

    public BoxCollider[] colliders = new BoxCollider[0];

    public Curve(Map map, Vector3 pos) : base(map, pos)
    {
    }

    public Curve(Map map, Vector3 pos, Quaternion rot) : base(map, pos, rot)
    {
    }

    protected Curve(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        radius = info.GetSingle(KEY_RADIUS);
        isVerticalCurve = info.GetBoolean(KEY_IS_VERTICAL_CURVE);
        cant = info.GetSingle(KEY_CANT);
        cantRotation = info.GetBoolean(KEY_CANT_ROTATION);
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(KEY_RADIUS, radius);
        info.AddValue(KEY_IS_VERTICAL_CURVE, isVerticalCurve);
        info.AddValue(KEY_CANT, cant);
        info.AddValue(KEY_CANT_ROTATION, cantRotation);
    }

    public override void generate()
    {
        if (entity)
            reloadEntity();
        else
            (entity = new GameObject("Curve").AddComponent<MapEntity>()).init(this);
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
                    railRenderers[a].sharedMaterial = Main.INSTANCE.selecting_track_mat;
                else if (Main.focused == this)
                    railRenderers[a].sharedMaterial = Main.INSTANCE.focused_track_mat;
                else
                    railRenderers[a].sharedMaterial = Main.INSTANCE.rail_mat;

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
            b.transform.localRotation = r_ * Quaternion.LookRotation(getPointCanted((a + 1f) / (railModelObjs.Length / 2f)) - p) * Quaternion.Euler(0f, 0f, -Mathf.Atan(cant / gauge) * Mathf.Rad2Deg);
        }
        for (var a = 0; a < railModelObjs.Length / 2; a++)
        {
            railModelObjs[a + railModelObjs.Length / 2] = b = GameObject.Instantiate(Main.INSTANCE.railRModel);
            b.transform.parent = entity.transform;
            setLOD(b, LOD_DISTANCE);
            var d = a / (railModelObjs.Length / 2f);
            var p = getPointCanted(d);
            b.transform.localPosition = r_ * (p - pos);
            b.transform.localRotation = r_ * Quaternion.LookRotation(getPointCanted((a + 1f) / (railModelObjs.Length / 2f)) - p) * Quaternion.Euler(0f, 0f, -Mathf.Atan(cant / gauge) * Mathf.Rad2Deg);
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
                var o = new GameObject();
                colliders[a] = o.AddComponent<BoxCollider>();
                o.transform.parent = entity.transform;
            }
            colliders[a].isTrigger = true;

            var b = Quaternion.Inverse(rot);
            var c = b * (getPointCanted((float)a / l) - pos);
            var d = b * (getPointCanted((a + 1f) / l) - pos);
            colliders[a].transform.localPosition = (c + d) / 2f;
            colliders[a].transform.localRotation = b * getRotationCanted((a + 0.5f) / l);
            colliders[a].size = new Vector3(COLLIDER_WIDTH, COLLIDER_HEIGHT, Vector3.Distance(c, d));
            colliders[a].enabled = enableCollider;
        }
    }

    /// <summary>
    /// 軌道上の位置(0-1)の座標を返す
    /// </summary>
    /// <param name="a">位置(0-1)</param>
    public override Vector3 getPoint(float a)
    {
        var d = length * a;
        var d1 = d * Mathf.Cos(rot.eulerAngles.x * Mathf.Deg2Rad) / Mathf.Abs(radius);
        d *= Mathf.Cos(rot.eulerAngles.x * Mathf.Deg2Rad);
        return isVerticalCurve ?
        pos + rot * new Vector3(0f, (1f - Mathf.Cos(d1)) * radius, Mathf.Sin(d1) * Mathf.Abs(radius)) :
        pos + Quaternion.Euler(0f, rot.eulerAngles.y, 0f) * new Vector3((1f - Mathf.Cos(d1)) * radius, Mathf.Sin(-rot.eulerAngles.x * Mathf.Deg2Rad) * d, Mathf.Sin(d1) * Mathf.Abs(radius));
    }

    /// <summary>
    /// 軌道上の位置(0-1)の座標をカント付きで返す
    /// </summary>
    /// <param name="a">位置(0-1)</param>
    public virtual Vector3 getPointCanted(float a)
    {
        var d = length * a;
        var d1 = d * Mathf.Cos(rot.eulerAngles.x * Mathf.Deg2Rad) / Mathf.Abs(radius);
        d *= Mathf.Cos(rot.eulerAngles.x * Mathf.Deg2Rad);
        if (isVerticalCurve)
            return pos + rot * new Vector3(0f, (1f - Mathf.Cos(d1)) * radius, Mathf.Sin(d1) * Mathf.Abs(radius));
        else
        {
            var p = cantRotation ? Vector3.zero : Vector3.up * cant / 2f;
            return pos + Quaternion.Euler(0f, rot.eulerAngles.y, 0f) * (new Vector3((1f - Mathf.Cos(d1)) * radius, Mathf.Sin(-rot.eulerAngles.x * Mathf.Deg2Rad) * d, Mathf.Sin(d1) * Mathf.Abs(radius)) + p);
        }
    }

    /// <summary>
    /// 軌道上の位置(0-1)の回転を返す
    /// </summary>
    /// <param name="a">位置(0-1)</param>
    public override Quaternion getRotation(float a)
    {
        return isVerticalCurve ?
        rot * Quaternion.Euler(-length * a * Mathf.Rad2Deg / radius, 0f, 0f) :
        Quaternion.Euler(0f, rot.eulerAngles.y, 0f) * Quaternion.Euler(rot.eulerAngles.x, length * a * Mathf.Rad2Deg / radius, 0f);
    }

    /// <summary>
    /// 軌道上の位置(0-1)の回転をカント付きで返す
    /// </summary>
    /// <param name="a">位置(0-1)</param>
    public virtual Quaternion getRotationCanted(float a)
    {
        return isVerticalCurve ?
        rot * Quaternion.Euler(-length * a * Mathf.Rad2Deg / radius, 0f, 0f) :
        Quaternion.Euler(0f, rot.eulerAngles.y, 0f) * Quaternion.Euler(rot.eulerAngles.x, length * a * Mathf.Rad2Deg / radius, -Mathf.Atan(cant / gauge) * Mathf.Rad2Deg);
    }

    /// <summary>
    /// 座標から軌道上の位置を求める
    /// </summary>
    /// <param name="pos">座標</param>
    public override float getLength(Vector3 pos)
    {
        var f = Quaternion.Inverse(rot);
        var a = f * (pos - this.pos);
        var r = radius;
        float A;

        if (isVerticalCurve)
        {
            if (r < 0f)
            {
                r = -r;
                a.y = -a.y;
            }
            A = Mathf.Atan(a.z / (r - a.y));

            if (A < 0f)
                A = Mathf.PI + A;
            if (a.z < 0f)
                A += Mathf.PI;
            return A * r;
        }
        else
        {
            var b = f * (getPoint(1f) - this.pos);

            if (r < 0f)
            {
                r = -r;
                a.x = -a.x;
                b.x = -b.x;
            }
            A = Mathf.Atan(a.z / (r - a.x));
            var A1 = Mathf.Atan(b.z / (r - b.x));
            if (A1 < 0f)
                A1 = Mathf.PI + A1;
            if (b.z < 0f)
                A1 += Mathf.PI;

            if (A < 0f)
                A = Mathf.PI + A;
            if (a.z < 0f)
                A += Mathf.PI;
            return A * length / A1;
        }
    }

    public virtual bool isLinear()
    {
        return length / radius <= Mathf.PI * 2f && rot == getRotation(1f);
    }
}
