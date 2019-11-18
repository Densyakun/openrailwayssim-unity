using System;
using System.Runtime.Serialization;
using UnityEngine;

/// <summary>
/// マップピン
/// </summary>
[Serializable]
public class MapPin : MapObject
{

    public const string KEY_TITLE = "TITLE";
    public const string KEY_DESCRIPTION = "DESCRIPTION";

    public string title;
    public string description;

    public TextEntity textEntity;

    public MapPin(Map map, Vector3 pos) : base(map, pos, Quaternion.identity)
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
        if (entity)
            reloadEntity();
        else
            (entity = new GameObject("MapPin").AddComponent<MapEntity>()).init(this);
    }

    public override void reloadEntity()
    {
        if (entity == null)
            return;

        if (textEntity == null)
            (textEntity = new GameObject("MapPinText").AddComponent<TextEntity>()).obj = this;
        textEntity.str = string.IsNullOrEmpty(title) ? "・" : title;

        base.reloadEntity();
    }

    public override void destroy()
    {
        GameObject.Destroy(textEntity.gameObject);

        base.destroy();
    }
}
