using System;
using System.IO;
using System.Runtime.Serialization;
using UnityEngine;

//マップピン
[Serializable]
public class Structure : MapObject
{
    public const string KEY_PATH = "PATH";

    public string path;

    Mesh mesh;

    public Structure(Map map, Vector3 pos) : base(map, pos, new Quaternion())
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
        if (entity == null)
            (entity = new GameObject("Structure").AddComponent<MapEntity>()).init(this);
        else
            reloadEntity();
    }

    public override void reloadEntity()
    {
        if (entity == null)
            return;

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

    public void importFile()
    {
        try
        {
            mesh = new ObjImporter().ImportFile(Path.Combine(Application.persistentDataPath, path));
        }
        catch (UnauthorizedAccessException) { }
        catch (FileNotFoundException) { }
    }
}
