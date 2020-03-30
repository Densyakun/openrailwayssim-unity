using System;
using System.Runtime.Serialization;

/// <summary>
/// 運転台
/// </summary>
[Serializable]
public class Cab : ISerializable
{

    public const string KEY_IS_FRONT = "IS_FRONT";
    public const string KEY_REVERSER = "REVERSER";
    public const string KEY_NOTCH = "NOTCH";

    /// <summary>
    /// 運転台が前方であるか
    /// </summary>
    public bool isFront;
    private int _reverser;
    /// <summary>
    /// 逆転器
    /// </summary>
    public int reverser
    {
        get { return _reverser; }
        set
        {
            _reverser = value;
            changed();
        }
    }
    private int _notch;
    /// <summary>
    /// ノッチ
    /// </summary>
    public int notch
    {
        get { return _notch; }
        set
        {
            _notch = value;
            changed();
        }
    }

    [NonSerialized]
    public Body body;

    public Cab(Body body, bool isFront)
    {
        this.body = body;
        this.isFront = isFront;
        _reverser = 0;
        _notch = 0;
    }

    protected Cab(SerializationInfo info, StreamingContext context)
    {
        if (info == null)
            throw new ArgumentNullException("info");
        isFront = info.GetBoolean(KEY_IS_FRONT);
        _reverser = info.GetInt32(KEY_REVERSER);
        _notch = info.GetInt32(KEY_NOTCH);
    }

    public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        if (info == null)
            throw new ArgumentNullException("info");
        info.AddValue(KEY_IS_FRONT, isFront);
        info.AddValue(KEY_REVERSER, _reverser);
        info.AddValue(KEY_NOTCH, _notch);
    }

    private void changed()
    {
        body.power = 0 < notch && reverser != 0 ? (float)(reverser == 1 ? notch : -notch) / (isFront ? body.powerNotchs : -body.powerNotchs) : 0f;
        body.brake = notch < 0 ? -(float)notch / body.brakeNotchs : 0f;
    }
}
