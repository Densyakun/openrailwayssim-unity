using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

/// <summary>
/// 信号機
/// </summary>
[Serializable]
public class Signal : MapObject
{

    /// <summary>
    /// 現示
    /// </summary>
    public enum Aspect
    {
        /// <summary>
        /// 高速進行(GG)
        /// </summary>
        DoubleGreen,
        /// <summary>
        /// 進行(G)
        /// </summary>
        Green,
        /// <summary>
        /// 抑速(YGF)
        /// </summary>
        YellowGreenFlashing,
        /// <summary>
        /// 減速(YG)
        /// </summary>
        YellowGreen,
        /// <summary>
        /// 注意(Y)
        /// </summary>
        Yellow,
        /// <summary>
        /// 警戒(YY)
        /// </summary>
        DoubleYellow,
        /// <summary>
        /// 停止(R)
        /// </summary>
        Red
    }

    public const string KEY_TRACK = "TRACK";
    public const string KEY_DIST = "DIST";
    public const string KEY_OFFSET = "OFFSET";
    public const string KEY_ASPECT = "ASPECT";
    public const string KEY_ENABLE_GG = "ENABLE_GG";
    public const string KEY_ENABLE_YGF = "ENABLE_YGF";
    public const string KEY_BLOCK_TRACKS = "BLOCK_TRACKS";

    public const float FLASHING_SPEED = 80f / 60f;

    /// <summary>
    /// 信号機を設置する軌道
    /// </summary>
    public Track track;
    /// <summary>
    /// 信号機を設置する軌道の位置
    /// </summary>
    public float dist;
    /// <summary>
    /// 軌道からのオフセット
    /// </summary>
    public Vector3 offset;
    /// <summary>
    /// 現示
    /// </summary>
    public Aspect aspect;
    /// <summary>
    /// GG現示が有効かどうか
    /// </summary>
    public bool enableGG;
    /// <summary>
    /// YGF現示が有効かどうか
    /// </summary>
    public bool enableYGF;
    /// <summary>
    /// 信号機を設置している軌道
    /// </summary>
    public List<Track> blockTracks;
    // TODO マテリアル（オブジェクト）とライト

    public GameObject modelObj;

    private float tick = 0f;

    public Signal(Map map, Track track, float dist) : base(map)
    {
        this.track = track;
        this.dist = dist;
        offset = Vector3.zero;
        aspect = Aspect.Red;
        enableGG = false;
        enableYGF = false;
        blockTracks = new List<Track>();
    }

    protected Signal(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        track = (Track)info.GetValue(KEY_TRACK, typeof(Track));
        dist = info.GetSingle(KEY_DIST);
        offset = ((SerializableVector3)info.GetValue(KEY_OFFSET, typeof(SerializableVector3))).toVector3();
        blockTracks = (List<Track>)info.GetValue(KEY_BLOCK_TRACKS, typeof(List<Track>));
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(KEY_TRACK, track);
        info.AddValue(KEY_DIST, dist);
        info.AddValue(KEY_OFFSET, offset);
        info.AddValue(KEY_BLOCK_TRACKS, blockTracks);
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

        if (modelObj == null)
        {
            (modelObj = GameObject.Instantiate(Main.INSTANCE.axleModel)).transform.parent = entity.transform;
            modelObj.transform.localPosition = Vector3.zero;
            reloadCollider();
        }
        reloadMaterial();
        //modelObj.transform.localEulerAngles = new Vector3(rotX, 0f);

        base.reloadEntity();
    }

    public override void reloadMaterial()
    {
        reloadMaterial(modelObj);
    }

    public void reloadCollider()
    {
        var collider = entity.GetComponent<BoxCollider>();
        if (collider == null)
            collider = entity.gameObject.AddComponent<BoxCollider>();
        collider.isTrigger = true;
        collider.size = Vector3.one;
    }
}
