using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

/// <summary>
/// ダイヤグラム
/// </summary>
[Serializable]
public class Diagram : ISerializable
{

    public const string KEY_TRACK = "TRACK";
    public const string KEY_DIST = "DIST";
    public const string KEY_ARRIVAL_TIME = "ARR";
    public const string KEY_DEPERTURE_TIME = "DEP";
    public const string KEY_STOPPAGE_TIME = "STOPPAGE";

    public List<Track> track;
    public List<float> dist;
    public List<int> arr;
    public List<int> dep;
    public List<int> stop;

    public Diagram()
    {
    }

    protected Diagram(SerializationInfo info, StreamingContext context)
    {
        if (info == null)
            throw new ArgumentNullException("info");
        track = (List<Track>)info.GetValue(KEY_TRACK, typeof(List<Track>));
        dist = (List<float>)info.GetValue(KEY_DIST, typeof(List<float>));
        arr = (List<int>)info.GetValue(KEY_ARRIVAL_TIME, typeof(List<int>));
        dep = (List<int>)info.GetValue(KEY_DEPERTURE_TIME, typeof(List<int>));
        stop = (List<int>)info.GetValue(KEY_STOPPAGE_TIME, typeof(List<int>));
    }

    public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        if (info == null)
            throw new ArgumentNullException("info");
        info.AddValue(KEY_TRACK, track);
        info.AddValue(KEY_DIST, dist);
        info.AddValue(KEY_ARRIVAL_TIME, arr);
        info.AddValue(KEY_DEPERTURE_TIME, dep);
        info.AddValue(KEY_STOPPAGE_TIME, stop);
    }
}
