using System;
using System.Runtime.Serialization;
using UnityEngine;

/// <summary>
/// マップ上に存在するオブジェクトを管理するクラス
/// </summary>
[Serializable]
public class MapObject : ISerializable
{

    public const string KEY_POS = "POS";
    public const string KEY_ROTATION = "ROT";

    public Vector3 pos;
    public Quaternion rot;

    [NonSerialized]
    public MapEntity entity;
    [NonSerialized]
    public Map map;

    public bool useSelectingMat = false;

    public MapObject(Map map) : this(map, new Vector3(), Quaternion.identity)
    {
    }

    public MapObject(Map map, Vector3 pos) : this(map, pos, Quaternion.identity)
    {
    }

    public MapObject(Map map, Vector3 pos, Quaternion rot)
    {
        this.map = map;
        this.pos = pos;
        this.rot = rot;
    }

    protected MapObject(SerializationInfo info, StreamingContext context)
    {
        if (info == null)
            throw new ArgumentNullException("info");
        pos = ((SerializableVector3)info.GetValue(KEY_POS, typeof(SerializableVector3))).toVector3();
        rot = ((SerializableQuaternion)info.GetValue(KEY_ROTATION, typeof(SerializableQuaternion))).toQuaternion();
    }

    public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        if (info == null)
            throw new ArgumentNullException("info");
        SyncFromEntity();
        info.AddValue(KEY_POS, new SerializableVector3(pos));
        info.AddValue(KEY_ROTATION, new SerializableQuaternion(rot));
    }

    public virtual void generate()
    {
        if (entity)
            reloadEntity();
        else
            (entity = new GameObject("MapObj").AddComponent<MapEntity>()).init(this);
    }

    public virtual void update()
    {
    }

    public virtual void fixedUpdate()
    {
    }

    public virtual void destroy()
    {
        entity = null;
    }

    public virtual void reloadEntity()
    {
        if (!entity)
            return;
        entity.transform.position = pos;
        entity.transform.rotation = rot;
    }

    public void reloadMaterial(GameObject obj)
    {
        if (useSelectingMat && !Main.INSTANCE.runPanel.isShowing())
        {
            Renderer[] b = obj.GetComponentsInChildren<Renderer>();
            foreach (var c in b)
            {
                if (c.sharedMaterials[c.sharedMaterials.Length - 1] != Main.INSTANCE.selecting_track_mat)
                {
                    Material[] d =
                        new Material[c.sharedMaterials[c.sharedMaterials.Length - 1] == Main.INSTANCE.focused_track_mat
                            ? c.sharedMaterials.Length
                            : c.sharedMaterials.Length + 1];
                    for (int e = 0; e < d.Length - 1; e++)
                        d[e] = c.sharedMaterials[e];
                    d[d.Length - 1] = Main.INSTANCE.selecting_track_mat;
                    c.sharedMaterials = d;
                }
            }
        }
        else if (Main.focused == this && !Main.INSTANCE.runPanel.isShowing())
        {
            Renderer[] b = obj.GetComponentsInChildren<Renderer>();
            foreach (var c in b)
            {
                if (c.sharedMaterials[c.sharedMaterials.Length - 1] != Main.INSTANCE.focused_track_mat)
                {
                    Material[] d =
                        new Material[c.sharedMaterials[c.sharedMaterials.Length - 1] == Main.INSTANCE.selecting_track_mat
                            ? c.sharedMaterials.Length
                            : c.sharedMaterials.Length + 1];
                    for (int e = 0; e < d.Length - 1; e++)
                        d[e] = c.sharedMaterials[e];
                    d[d.Length - 1] = Main.INSTANCE.focused_track_mat;
                    c.sharedMaterials = d;
                }
            }
        }
        else
        {
            Renderer[] b = obj.GetComponentsInChildren<Renderer>();
            foreach (var c in b)
            {
                if (c.sharedMaterials.Length >= 1 &&
                    (c.sharedMaterials[c.sharedMaterials.Length - 1] == Main.INSTANCE.selecting_track_mat ||
                     c.sharedMaterials[c.sharedMaterials.Length - 1] == Main.INSTANCE.focused_track_mat))
                {
                    Material[] d = new Material[c.sharedMaterials.Length - 1];
                    for (int e = 0; e < d.Length; e++)
                        d[e] = c.sharedMaterials[e];
                    c.sharedMaterials = d;
                }
            }
        }
    }

    /// <summary>
    /// 時間が経過するメソッド。ticksには経過時間を指定。
    /// </summary>
    public virtual void TimePasses(long ticks)
    {
    }

    public virtual void SyncFromEntity()
    {
        if (entity)
        {
            pos = entity.transform.position;
            rot = entity.transform.rotation;
        }
    }

    public virtual void SyncToEntity()
    {
        if (entity)
        {
            entity.transform.position = pos;
            entity.transform.rotation = rot;
        }
    }
}
