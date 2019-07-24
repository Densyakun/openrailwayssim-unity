using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using UnityEngine;

public class X3DImporter
{

    public List<GameObject> objects
    {
        get;
        private set;
    }

    public void load(string path, Transform parent = null)
    {
        objects = new List<GameObject>();

        XmlReaderSettings settings = new XmlReaderSettings();
        settings.ConformanceLevel = ConformanceLevel.Document;
        settings.XmlResolver = null;
        settings.CloseInput = true;
        settings.IgnoreWhitespace = false;
        settings.ProhibitDtd = false;
        XmlReader reader = XmlReader.Create(path, settings);
        var xml = XDocument.Load(reader);
        var x3d = xml.Element("X3D");
        var scene = x3d.Element("Scene");
        loadE(scene, parent);
    }

    void loadE(XElement e, Transform parent = null, Vector3 pos = default(Vector3), Quaternion rot = default(Quaternion), Vector3? scale = null)
    {
        var e_ = e.Elements("Transform");
        if (!e_.Any())
            e_ = e.Elements("Group");

        foreach (var e__ in e_)
        {
            var p_ = pos;
            var r_ = rot;
            var s_ = scale;
            if (e__.Name == "Transform")
            {
                var p = e__.Attribute("translation").Value.Trim().Split(' ');
                p_ += new Vector3(-float.Parse(p[0]), float.Parse(p[1]), float.Parse(p[2]));
                var r = e__.Attribute("rotation").Value.Trim().Split(' ');
                r_ = r_ * new Quaternion(float.Parse(r[0]), float.Parse(r[1]), float.Parse(r[2]), float.Parse(r[3]));
                var s = e__.Attribute("scale").Value.Trim().Split(' ');
                s_ = Vector3.Scale(s_ == null ? Vector3.one : (Vector3)s_, new Vector3(float.Parse(s[0]), float.Parse(s[1]), float.Parse(s[2])));
            }
            var shape = e__.Element("Shape");
            if (shape != null)
            {
                parent = loadS(shape, parent).transform;
                parent.localPosition = p_;
                parent.localRotation = r_;
                parent.localScale = s_ == null ? Vector3.one : (Vector3)s_;
                p_ = default(Vector3);
                r_ = default(Quaternion);
                s_ = Vector3.one;
            }
            loadE(e__, parent, p_, r_, s_);
        }
    }

    GameObject loadS(XElement e, Transform parent = null)
    {
        var o = new GameObject();
        var filter = o.AddComponent<MeshFilter>();
        var renderer = o.AddComponent<MeshRenderer>();

        var mesh = new Mesh();

        var indexedFaceSet = e.Element("IndexedFaceSet");
        var c = indexedFaceSet.Attribute("coordIndex").Value.Trim().Split(' ');
        var p = indexedFaceSet.Element("Coordinate").Attribute("point").Value.Trim().Split(' ');

        int i;

        var v = new Vector3[p.Length / 3];
        for (i = 0; i < v.Length; i++)
            v[i] = new Vector3(-float.Parse(p[i * 3]), float.Parse(p[i * 3 + 1]), float.Parse(p[i * 3 + 2]));
        mesh.vertices = v;

        var s = new List<int>(4);
        var t = new List<int>();
        foreach (var c_ in c)
        {
            var c__ = int.Parse(c_);
            if (c__ == -1)
            {
                if (s.Count == 4)
                {
                    t.Add(s[0]);
                    t.Add(s[1]);
                    t.Add(s[3]);
                    t.Add(s[1]);
                    t.Add(s[2]);
                    t.Add(s[3]);
                }
                else
                    t.AddRange(s);
                s.Clear();
            }
            else
                s.Add(c__);
        }
        mesh.triangles = t.ToArray();

        filter.mesh = mesh;

        if (parent)
            o.transform.SetParent(parent);

        objects.Add(o);
        return o;
    }

    public static bool isX3DFile(string path)
    {
        return Path.GetExtension(path).ToLower() == ".x3d";
    }
}
