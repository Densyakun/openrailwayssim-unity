using System;
using System.IO;
using System.Runtime.Serialization;
using UnityEngine;

/// <summary>
/// ストラクチャ
/// </summary>
[Serializable]
public class Structure : MapObject
{

    public const string KEY_PATH = "PATH";

    public string path;

    public Mesh mesh;
    public TextEntity textEntity;

    public Structure(Map map, Vector3 pos) : base(map, pos)
    {
        path = "";

        importFile();
    }

    protected Structure(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        path = info.GetString(KEY_PATH);

        importFile();
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(KEY_PATH, path);
    }

    public override void generate()
    {
        if (entity)
            reloadEntity();
        else
            (entity = new GameObject("Structure").AddComponent<MapEntity>()).init(this);
    }

    public override void reloadEntity()
    {
        if (entity == null)
            return;

        if (textEntity == null)
        {
            (textEntity = new GameObject("StructureText").AddComponent<TextEntity>()).obj = this;
            textEntity.str = "・";
            textEntity.normalColor = new Color(0f, 1f, 0f, 0.75f);
        }

        var filter = entity.GetComponent<MeshFilter>();
        if (!filter)
            filter = entity.gameObject.AddComponent<MeshFilter>();
        if (mesh)
            filter.sharedMesh = mesh;
        var renderer = entity.GetComponent<MeshRenderer>();
        if (!renderer)
            renderer = entity.gameObject.AddComponent<MeshRenderer>();
        var collider = entity.GetComponent<BoxCollider>();
        if (!collider)
            collider = entity.gameObject.AddComponent<BoxCollider>();

        reloadMaterial(entity.gameObject);

        base.reloadEntity();
    }

    public override void destroy()
    {
        GameObject.Destroy(textEntity.gameObject);

        base.destroy();
    }

    public void importFile()
    {
        if (entity)
            foreach (var t in entity.GetComponentsInChildren<Transform>())
                if (t != entity.transform)
                    GameObject.Destroy(t.gameObject);
        var p = Path.Combine(Application.persistentDataPath, path);
        try
        {
            mesh = new ObjImporter().ImportFile(p);
        }
        catch (UnauthorizedAccessException) { }
        catch (FileNotFoundException) { }
    }
}
