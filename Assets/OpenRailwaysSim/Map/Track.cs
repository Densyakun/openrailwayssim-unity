using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

//軌道(直線)
[Serializable]
public class Track : MapObject
{
    public const string KEY_LENGTH = "L";
    public const string KEY_GAUGE = "G";
    public const string KEY_NEXT_TRACKS = "N_T";
    public const string KEY_PREV_TRACKS = "P_T";
    public const string KEY_CONNECTING_NEXT_TRACKS = "C_N_T";
    public const string KEY_CONNECTING_PREV_TRACKS = "C_P_T";

    public const float MIN_TRACK_LENGTH = 1f;
    public const float MAX_TRACK_LENGTH = 1024f;
    public const float RENDER_WIDTH = 0.25f;
    public const float RAIL_RENDER_WIDTH = 0.05f;
    public const float COLLIDER_WIDTH = 2f;
    public const float COLLIDER_HEIGHT = 1f / 8;
    public const float RAIL_MODEL_INTERVAL = 1f;
    public const float TIE_MODEL_INTERVAL = 25f / 37f;
    public const float LOD_DISTANCE = 0.03f;

    public static float defaultGauge = 1.435f;

    protected float _length = MIN_TRACK_LENGTH;

    public virtual float length
    {
        get { return _length; }
        set { _length = Mathf.Max(MIN_TRACK_LENGTH, Mathf.Min(MAX_TRACK_LENGTH, value)); }
    }
    public float gauge;

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
        _length = info.GetSingle(KEY_LENGTH);
        gauge = info.GetSingle(KEY_GAUGE);
        _nextTracks = (List<Track>)info.GetValue(KEY_NEXT_TRACKS, typeof(List<Track>));
        _prevTracks = (List<Track>)info.GetValue(KEY_PREV_TRACKS, typeof(List<Track>));
        _connectingNextTrack = info.GetInt32(KEY_CONNECTING_NEXT_TRACKS);
        _connectingPrevTrack = info.GetInt32(KEY_CONNECTING_PREV_TRACKS);
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(KEY_LENGTH, _length);
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

        reloadTrackRenderer();
        reloadRailRenderers();
        reloadModels();
        reloadCollider();

        base.reloadEntity();
    }

    public void reloadTrackRenderer()
    {
        if (!GameCanvas.runPanel.isShowing() && Main.main.showGuide)
        {
            if (trackRenderer == null)
                trackRenderer = entity.gameObject.AddComponent<LineRenderer>();
            trackRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            trackRenderer.receiveShadows = false;
            trackRenderer.endWidth = trackRenderer.startWidth = RENDER_WIDTH;
            trackRenderer.endColor = trackRenderer.startColor = Color.white;
            if (useSelectingMat)
                trackRenderer.sharedMaterial = Main.main.selecting_track_mat;
            else if (Main.focused == this)
                trackRenderer.sharedMaterial = Main.main.focused_track_mat;
            else
                trackRenderer.sharedMaterial = Main.main.track_mat;

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
        if (!GameCanvas.runPanel.isShowing() && Main.main.showGuide)
        {
            railRenderers = new LineRenderer[2];
            for (int a = 0; a < 2; a++)
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

                Vector3 b = rot * Vector3.right * (a == 0 ? -gauge / 2f : gauge / 2f);
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
        railModelObjs = new GameObject[Mathf.CeilToInt(_length / RAIL_MODEL_INTERVAL) * 2];
        GameObject b;
        for (int a = 0; a < railModelObjs.Length / 2; a++)
        {
            railModelObjs[a] = b = GameObject.Instantiate(Main.main.railLModel);
            b.transform.parent = entity.transform;
            setLOD(b, LOD_DISTANCE);
            b.transform.localPosition = r_ * (getPoint((float)a / (railModelObjs.Length / 2)) - pos);
            b.transform.localEulerAngles = new Vector3();
        }
        for (int a = 0; a < railModelObjs.Length / 2; a++)
        {
            railModelObjs[a + railModelObjs.Length / 2] = b = GameObject.Instantiate(Main.main.railRModel);
            b.transform.parent = entity.transform;
            setLOD(b, LOD_DISTANCE);
            b.transform.localPosition = r_ * (getPoint((float)a / (railModelObjs.Length / 2)) - pos);
            b.transform.localEulerAngles = new Vector3();
        }

        if (tieModelObjs != null)
            foreach (var r in tieModelObjs)
                GameObject.Destroy(r.gameObject);
        tieModelObjs = new GameObject[Mathf.CeilToInt(_length / TIE_MODEL_INTERVAL)];
        for (int a = 0; a < tieModelObjs.Length; a++)
        {
            (tieModelObjs[a] = GameObject.Instantiate(Main.main.tieModel)).transform.parent = entity.transform;
            setLOD(tieModelObjs[a], LOD_DISTANCE);
            tieModelObjs[a].transform.localPosition = r_ * (getPoint((float)a / tieModelObjs.Length) - pos);
            tieModelObjs[a].transform.localEulerAngles = new Vector3();
        }
    }

    public virtual void reloadCollider()
    {
        BoxCollider collider = entity.GetComponent<BoxCollider>();
        if (collider == null)
            collider = entity.gameObject.AddComponent<BoxCollider>();
        collider.isTrigger = true;
        collider.center = Vector3.forward * _length / 2;
        collider.size = new Vector3(COLLIDER_WIDTH, COLLIDER_HEIGHT, _length);
        collider.enabled = enableCollider;
    }

    public virtual Vector3 getPoint(float a)
    {
        return pos + rot * Vector3.forward * _length * a;
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
