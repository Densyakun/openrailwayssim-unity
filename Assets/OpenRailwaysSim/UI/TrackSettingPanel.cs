using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//軌道の設定画面
public class TrackSettingPanel : GamePanel
{
    public List<SegmentSettingPanel> segmentPanels = new List<SegmentSettingPanel>();
    public List<SegmentSettingPanel> verticalSegmentPanels = new List<SegmentSettingPanel>();
    private List<float> curveLength;
    private List<float> curveRadius;
    private List<float> verticalCurveLength;
    private List<float> verticalCurveRadius;

    private List<float> lastCurveLength;
    private List<float> lastCurveRadius;
    private List<float> lastVerticalCurveLength;
    private List<float> lastVerticalCurveRadius;

    void Update()
    {
        reflect();
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            InputField input = null;
            if (EventSystem.current.currentSelectedGameObject != null)
                input = EventSystem.current.currentSelectedGameObject.GetComponent<InputField>();
            if (input != null && input.isFocused)
            {
                for (var n = 0; n < segmentPanels.Count; n++)
                {
                    if (input == segmentPanels[n].lengthInput)
                        EventSystem.current.SetSelectedGameObject(segmentPanels[n].radiusInput.gameObject);
                    else if (input == segmentPanels[n].radiusInput)
                        EventSystem.current.SetSelectedGameObject((n == segmentPanels.Count - 1 ? verticalSegmentPanels.Count > 0 ? verticalSegmentPanels[0] : segmentPanels[0] : segmentPanels[n + 1]).lengthInput.gameObject);
                }
                for (var n = 0; n < verticalSegmentPanels.Count; n++)
                {
                    if (input == verticalSegmentPanels[n].lengthInput)
                        EventSystem.current.SetSelectedGameObject(verticalSegmentPanels[n].radiusInput.gameObject);
                    else if (input == verticalSegmentPanels[n].radiusInput)
                        EventSystem.current.SetSelectedGameObject((n == verticalSegmentPanels.Count - 1 ? segmentPanels.Count > 0 ? segmentPanels[0] : verticalSegmentPanels[0] : verticalSegmentPanels[n + 1]).lengthInput.gameObject);
                }
            }
            else
            {
                if (segmentPanels.Count > 0)
                    EventSystem.current.SetSelectedGameObject(segmentPanels[0].lengthInput.gameObject);
                else if (verticalSegmentPanels.Count > 0)
                    EventSystem.current.SetSelectedGameObject(verticalSegmentPanels[0].lengthInput.gameObject);
            }
        }
    }

    public void load()
    {
        curveLength = lastCurveLength = Main.editingTracks[0].curveLength;
        curveRadius = lastCurveRadius = Main.editingTracks[0].curveRadius;
        verticalCurveLength = lastVerticalCurveLength = Main.editingTracks[0].verticalCurveLength;
        verticalCurveRadius = lastVerticalCurveRadius = Main.editingTracks[0].verticalCurveRadius;
        reloadSegmentPanels();
    }

    new public void show(bool show)
    {
        if (show)
            load();

        base.show(show);
    }

    public void reflect()
    {
        if (!isShowing())
            return;

        try
        {
            var lengthL = new List<float>();
            var radiusL = new List<float>();
            foreach (var p in segmentPanels)
            {
                lengthL.Add(float.Parse(p.lengthInput.text));
                radiusL.Add(float.Parse(p.radiusInput.text));
            }
            curveLength = lengthL;
            curveRadius = radiusL;
            lengthL = new List<float>();
            radiusL = new List<float>();
            for (var n = 0; n < curveLength.Count; n++)
            {
                if (curveLength[n] != 0f)
                {
                    lengthL.Add(curveLength[n]);
                    radiusL.Add(curveRadius[n]);
                }
            }
            Main.editingTracks[0].curveLength = lengthL;
            Main.editingTracks[0].curveRadius = radiusL;

            lengthL = new List<float>();
            radiusL = new List<float>();
            foreach (var p in verticalSegmentPanels)
            {
                lengthL.Add(float.Parse(p.lengthInput.text));
                radiusL.Add(float.Parse(p.radiusInput.text));
            }
            verticalCurveLength = lengthL;
            verticalCurveRadius = radiusL;
            lengthL = new List<float>();
            radiusL = new List<float>();
            for (var n = 0; n < verticalCurveLength.Count; n++)
            {
                if (verticalCurveLength[n] != 0f)
                {
                    lengthL.Add(verticalCurveLength[n]);
                    radiusL.Add(verticalCurveRadius[n]);
                }
            }
            Main.editingTracks[0].verticalCurveLength = lengthL;
            Main.editingTracks[0].verticalCurveRadius = radiusL;

            Main.editingTracks[0].reloadLength(); // TODO 正しく適応されていない？セーブされると長さが変わる
            Main.editingTracks[0].reloadEntity();
        }
        catch (FormatException) { }
        catch (OverflowException) { }
    }

    public void save()
    {
        reflect();
        Main.gauge = Main.editingTracks[0].gauge;
        Main.main.trackEdited0();
        Main.editingTracks.Clear();
        Main.editingRot = null;

        if (!Input.GetKeyDown(KeyCode.Escape))
            show(false);
    }

    public void cancel()
    {
        Main.editingTracks[0].curveLength = lastCurveLength;
        Main.editingTracks[0].curveRadius = lastCurveRadius;
        Main.editingTracks[0].verticalCurveLength = lastVerticalCurveLength;
        Main.editingTracks[0].verticalCurveRadius = lastVerticalCurveRadius;

        Main.editingTracks[0].reloadLength();
        Main.editingTracks[0].reloadEntity();

        show(false);
    }

    private void reloadSegmentPanels()
    {
        foreach (var p in segmentPanels)
            Destroy(p.gameObject);
        segmentPanels.Clear();
        for (var n = 0; n < curveLength.Count; n++)
        {
            var p = Instantiate(Main.main.segmentSettingPanelPrefab);
            p.n = n;
            segmentPanels.Add(p);
            p.transform.SetParent(transform);
            p.transform.localPosition = new Vector3(0f, -30f - 90f * n);
            p.titleText.text = SegmentSettingPanel.segmentText_DEF + " " + (n + 1) + "/" + curveLength.Count;
            p.lengthInput.text = curveLength[n].ToString();
            p.radiusInput.text = curveRadius[n].ToString();
        }
        foreach (var p in verticalSegmentPanels)
            Destroy(p.gameObject);
        verticalSegmentPanels.Clear();
        for (var n = 0; n < verticalCurveLength.Count; n++)
        {
            var p = Instantiate(Main.main.segmentSettingPanelPrefab);
            p.n = n;
            p.isVertical = true;
            verticalSegmentPanels.Add(p);
            p.transform.SetParent(transform);
            p.transform.localPosition = new Vector3(0f, -30f - 90f * (n + curveLength.Count));
            p.titleText.text = SegmentSettingPanel.verticalSegmentText_DEF + " " + (n + 1) + "/" + verticalCurveLength.Count;
            p.lengthText.text = SegmentSettingPanel.lengthText_DEF + ": ";
            p.radiusText.text = SegmentSettingPanel.radiusText_DEF + ": ";
            p.lengthInput.text = verticalCurveLength[n].ToString();
            p.radiusInput.text = verticalCurveRadius[n].ToString();
        }
    }

    public void addSegment(int n = -1)
    {
        if (n == -1)
        {
            curveLength.Add(0f);
            curveRadius.Add(0f);
        }
        else
        {
            curveLength.Insert(n, 0f);
            curveRadius.Insert(n, 0f);
        }
        reloadSegmentPanels();
        reflect();
    }

    public void removeSegment(int n)
    {
        curveLength.RemoveAt(n);
        curveRadius.RemoveAt(n);
        reloadSegmentPanels();
        reflect();
    }

    public void addVerticalSegment(int n = -1)
    {
        if (n == -1)
        {
            verticalCurveLength.Add(0f);
            verticalCurveRadius.Add(0f);
        }
        else
        {
            verticalCurveLength.Insert(n, 0f);
            verticalCurveRadius.Insert(n, 0f);
        }
        reloadSegmentPanels();
        reflect();
    }

    public void removeVerticalSegment(int n)
    {
        verticalCurveLength.RemoveAt(n);
        verticalCurveRadius.RemoveAt(n);
        reloadSegmentPanels();
        reflect();
    }
}
