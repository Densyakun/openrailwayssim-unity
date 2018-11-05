using System;
using System.Runtime.Serialization;
using UnityEngine;

//マップピン
[Serializable]
public class Structure : MapObject
{
    public const string KEY_PATH = "PATH";

    public string path;

    //public MapPinEntity textEntity;

    public Structure(Map map, Vector3 pos) : base(map, pos, new Quaternion())
    {
    }

    protected Structure(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        path = info.GetString(KEY_PATH);
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

        //TODO

        base.reloadEntity();
    }
}
