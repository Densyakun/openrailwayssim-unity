using UnityEngine;

/// <summary>
// MapObjectの実体
/// </summary>
public class MapEntity : MonoBehaviour
{

    public virtual MapObject obj { get; protected set; }

    void Start()
    {
        obj.reloadEntity();
    }

    void Update()
    {
        if (!Main.pause)
            obj.update();
    }

    void FixedUpdate()
    {
        obj.fixedUpdate();
    }

    /// <summary>
    /// Startメソッドが実行される前に、MapObjectを設定する
    /// </summary>
    public virtual void init(MapObject obj)
    {
        this.obj = obj;
    }

    public virtual void Destroy()
    {
        obj.SyncFromEntity();
        obj.destroy();
        Destroy(gameObject);
    }
}
