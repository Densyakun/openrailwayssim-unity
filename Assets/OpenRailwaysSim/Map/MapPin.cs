using System;
using System.Runtime.Serialization;
using UnityEngine;

//マップピン
[Serializable]
public class MapPin : MapObject
{
    public const string KEY_TITLE = "TITLE";
    public const string KEY_DESCRIPTION = "DESCRIPTION";

    public string title;
    public string description;

    public MapPinEntity textEntity;

    public MapPin(Map map, Vector3 pos) : base(map, pos, new Quaternion())
    {
    }

    protected MapPin(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        title = info.GetString(KEY_TITLE);
        description = info.GetString(KEY_DESCRIPTION);
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(KEY_TITLE, title);
        info.AddValue(KEY_DESCRIPTION, description);
    }

    public override void generate()
    {
        if (entity == null)
            (entity = new GameObject("mapPin").AddComponent<MapEntity>()).init(this);
        else
            reloadEntity();
    }

    public override void reloadEntity()
    {
        if (entity == null)
            return;

        if (textEntity == null)
        {
            (textEntity = new GameObject("mapPinText").AddComponent<MapPinEntity>()).mapPin = this;
            textEntity.transform.SetParent(GameCanvas.canvas.transform);
        }

        base.reloadEntity();
    }

    public override void destroy()
    {
        GameObject.Destroy(textEntity.gameObject);
    }
}
