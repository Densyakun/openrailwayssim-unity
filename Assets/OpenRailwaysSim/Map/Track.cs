using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

/// <summary>
/// 軌道（直線）
/// </summary>
[Serializable]
public class Track : MapObject
{

    public const string KEY_LENGTH = "L";
    public const string KEY_GAUGE = "G";
    public const string KEY_NEXT_TRACKS = "N_T";
    public const string KEY_PREV_TRACKS = "P_T";
    public const string KEY_CONNECTING_NEXT_TRACKS = "C_N_T";
    public const string KEY_CONNECTING_PREV_TRACKS = "C_P_T";

    /// <summary>
    /// 許容する誤差
    /// </summary>
    public const float ALLOWABLE_RANGE = 0.0001f;
    public const float RENDER_WIDTH = 0.25f;
    public const float RAIL_RENDER_WIDTH = 0.05f;
    public const float COLLIDER_WIDTH = 2f;
    public const float COLLIDER_HEIGHT = 1f / 8;
    public const float RAIL_MODEL_INTERVAL = 1f;
    public const float TIE_MODEL_INTERVAL = 25f / 37f;
    public const float LOD_DISTANCE = 0.03f;

    public static float defaultGauge = 1.435f;

    public float length;
    public float gauge;
    [NonSerialized]
    private List<Track> _nextTracks;
    public List<Track> nextTracks
    {
        get { return _nextTracks; }
        set
        {
            if ((_nextTracks = value).Count <= _connectingNextTrack)
                _connectingNextTrack = -1;
        }
    }
    [NonSerialized]
    private List<Track> _prevTracks;
    public List<Track> prevTracks
    {
        get { return _prevTracks; }
        set
        {
            if ((_prevTracks = value).Count <= _connectingPrevTrack)
                _connectingPrevTrack = -1;
        }
    }
    private int _connectingNextTrack;
    public int connectingNextTrack
    {
        get { return _connectingNextTrack; }
        set
        {
            _connectingNextTrack = value <
                                   -1 || nextTracks.Count <= value
                ? -1
                : value;
        }
    }
    private int _connectingPrevTrack;
    public int connectingPrevTrack
    {
        get { return _connectingPrevTrack; }
        set
        {
            _connectingPrevTrack = value <
                                   -1 || prevTracks.Count <= value
                ? -1
                : value;
        }
    }

    public LineRenderer trackRenderer;
    public LineRenderer[] railRenderers;
    public bool enableCollider = true;
    public GameObject[] railModelObjs;
    public GameObject[] tieModelObjs;

    public Track(Map map, Vector3 pos) : this(map, pos, Quaternion.identity)
    {
    }

    public Track(Map map, Vector3 pos, Quaternion rot) : base(map, pos, rot)
    {
        gauge = defaultGauge;
        _nextTracks = new List<Track>();
        _prevTracks = new List<Track>();
        _connectingPrevTrack = _connectingNextTrack = -1;
    }

    protected Track(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        length = info.GetSingle(KEY_LENGTH);
        gauge = info.GetSingle(KEY_GAUGE);
        _nextTracks = (List<Track>)info.GetValue(KEY_NEXT_TRACKS, typeof(List<Track>));
        _prevTracks = (List<Track>)info.GetValue(KEY_PREV_TRACKS, typeof(List<Track>));
        _connectingNextTrack = info.GetInt32(KEY_CONNECTING_NEXT_TRACKS);
        _connectingPrevTrack = info.GetInt32(KEY_CONNECTING_PREV_TRACKS);
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(KEY_LENGTH, length);
        info.AddValue(KEY_GAUGE, gauge);
        info.AddValue(KEY_NEXT_TRACKS, _nextTracks);
        info.AddValue(KEY_PREV_TRACKS, _prevTracks);
        info.AddValue(KEY_CONNECTING_NEXT_TRACKS, _connectingNextTrack);
        info.AddValue(KEY_CONNECTING_PREV_TRACKS, _connectingPrevTrack);
    }

    public override void generate()
    {
        if (entity)
            reloadEntity();
        else
            (entity = new GameObject("Track").AddComponent<MapEntity>()).init(this);
    }

    public override void reloadEntity()
    {
        if (entity == null)
            return;

        reloadMaterial();
        reloadModels();
        reloadCollider();

        base.reloadEntity();
    }

    public override void reloadMaterial()
    {
        reloadTrackRenderer();
        reloadRailRenderers();
    }

    public void reloadTrackRenderer()
    {
        if (Main.INSTANCE.grid.activeSelf)
        {
            if (trackRenderer == null)
                trackRenderer = entity.gameObject.AddComponent<LineRenderer>();
            trackRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            trackRenderer.receiveShadows = false;
            trackRenderer.endWidth = trackRenderer.startWidth = RENDER_WIDTH;
            trackRenderer.endColor = trackRenderer.startColor = Color.white;
            if (useSelectingMat)
                trackRenderer.sharedMaterial = Main.INSTANCE.selectingMat;
            else if (Main.focused == this)
                trackRenderer.sharedMaterial = Main.INSTANCE.focusedMat;
            else
                trackRenderer.sharedMaterial = Main.INSTANCE.trackMat;

            reloadTrackRendererPositions();
        }
        else if (trackRenderer != null)
        {
            GameObject.Destroy(trackRenderer);
            trackRenderer = null;
        }
    }

    public virtual void reloadTrackRendererPositions()
    {
        trackRenderer.SetPositions(new Vector3[] { pos, getPoint(1) });
    }

    public virtual void reloadRailRenderers()
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

                var b = rot * Vector3.right * (a == 0 ? -gauge / 2f : gauge / 2f);
                railRenderers[a].SetPositions(new Vector3[] { pos + b, getPoint(1) + b });
            }
        }
        else
            railRenderers = null;
    }

    public virtual void reloadModels()
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
            b.transform.localPosition = r_ * (getPoint((float)a / (railModelObjs.Length / 2)) - pos);
            b.transform.localEulerAngles = new Vector3();
        }
        for (var a = 0; a < railModelObjs.Length / 2; a++)
        {
            railModelObjs[a + railModelObjs.Length / 2] = b = GameObject.Instantiate(Main.INSTANCE.railRModel);
            b.transform.parent = entity.transform;
            setLOD(b, LOD_DISTANCE);
            b.transform.localPosition = r_ * (getPoint((float)a / (railModelObjs.Length / 2)) - pos);
            b.transform.localEulerAngles = new Vector3();
        }

        if (tieModelObjs != null)
            foreach (var r in tieModelObjs)
                GameObject.Destroy(r.gameObject);
        tieModelObjs = new GameObject[Mathf.CeilToInt(length / TIE_MODEL_INTERVAL)];
        for (var a = 0; a < tieModelObjs.Length; a++)
        {
            (tieModelObjs[a] = GameObject.Instantiate(Main.INSTANCE.tieModel)).transform.parent = entity.transform;
            setLOD(tieModelObjs[a], LOD_DISTANCE);
            tieModelObjs[a].transform.localPosition = r_ * (getPoint((float)a / tieModelObjs.Length) - pos);
            tieModelObjs[a].transform.localEulerAngles = new Vector3();
        }
    }

    public virtual void reloadCollider()
    {
        var collider = entity.GetComponent<BoxCollider>();
        if (collider == null)
            collider = entity.gameObject.AddComponent<BoxCollider>();
        collider.isTrigger = true;
        collider.center = Vector3.forward * length / 2f;
        collider.size = new Vector3(COLLIDER_WIDTH, COLLIDER_HEIGHT, length);
        collider.enabled = enableCollider;
    }

    /// <summary>
    /// 軌道上の位置(0-1)の座標を返す
    /// </summary>
    /// <param name="a">位置(0-1)</param>
    public virtual Vector3 getPoint(float a)
    {
        return pos + rot * Vector3.forward * length * a;
    }

    /// <summary>
    /// 軌道上の位置(0-1)の回転を返す
    /// </summary>
    /// <param name="a">位置(0-1)</param>
    public virtual Quaternion getRotation(float a)
    {
        return rot;
    }

    /// <summary>
    /// 座標から軌道上の位置を求める
    /// </summary>
    /// <param name="pos">座標</param>
    public virtual float getLength(Vector3 pos)
    {
        return (Quaternion.Inverse(rot) * (pos - this.pos)).z;
    }

    public void connectingTrack(Track track)
    {
        var a0 = (track.pos - pos).sqrMagnitude < ALLOWABLE_RANGE;
        var a1 = (track.rot.eulerAngles - rot.eulerAngles).sqrMagnitude < ALLOWABLE_RANGE;
        var a2 = (track.pos - getPoint(1f)).sqrMagnitude < ALLOWABLE_RANGE;
        var a3 = (track.rot.eulerAngles - getRotation(1f).eulerAngles).sqrMagnitude < ALLOWABLE_RANGE;
        var a4 = (track.getPoint(1f) - pos).sqrMagnitude < ALLOWABLE_RANGE;
        var a5 = (track.getRotation(1f).eulerAngles - rot.eulerAngles).sqrMagnitude < ALLOWABLE_RANGE;
        var a6 = (track.getPoint(1f) - getPoint(1f)).sqrMagnitude < ALLOWABLE_RANGE;
        var a7 = (track.getRotation(1f).eulerAngles - getRotation(1f).eulerAngles).sqrMagnitude < ALLOWABLE_RANGE;

        if (a0 && a1 || a2 && a3)
        {
            track.prevTracks.Add(this);
            if (track.prevTracks.Count == 1)
                track.connectingPrevTrack = 0;
        }
        if (a4 && a5 || a6 && a7)
        {
            track.nextTracks.Add(this);
            if (track.nextTracks.Count == 1)
                track.connectingNextTrack = 0;
        }

        if (a0 && a1 || a4 && a5)
        {
            prevTracks.Add(track);
            if (prevTracks.Count == 1)
                connectingPrevTrack = 0;
        }
        if (a2 && a3 || a6 && a7)
        {
            nextTracks.Add(track);
            if (nextTracks.Count == 1)
                connectingNextTrack = 0;
        }
    }

    public void removeConnects()
    {
        foreach (var t in nextTracks)
        {
            var a = t.prevTracks;
            a.Remove(this);
            t.prevTracks = a;
            a = t.nextTracks;
            a.Remove(this);
            t.nextTracks = a;
        }

        foreach (var t in prevTracks)
        {
            var a = t.nextTracks;
            a.Remove(this);
            t.nextTracks = a;
            a = t.prevTracks;
            a.Remove(this);
            t.prevTracks = a;
        }

        _nextTracks.Clear();
        _prevTracks.Clear();
        _connectingNextTrack = -1;
        _connectingPrevTrack = -1;
    }

    public void setLOD(GameObject obj, float screenRelativeTransitionHeight)
    {
        LODGroup lodGroup = obj.AddComponent<LODGroup>();
        List<Renderer> renderers = new List<Renderer>(obj.GetComponents<Renderer>());
        foreach (Transform t in obj.GetComponentsInChildren<Transform>())
            renderers.AddRange(t.GetComponents<Renderer>());
        LOD[] lods = new LOD[] { new LOD(screenRelativeTransitionHeight, renderers.ToArray()) };
        lodGroup.SetLODs(lods);
    }
}
