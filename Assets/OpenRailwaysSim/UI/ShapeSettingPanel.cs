using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 線形の設定画面
/// </summary>
public class ShapeSettingPanel : GamePanel
{

    public SegmentSettingPanel segmentSettingPanelPrefab;
    public VerticalSegmentSettingPanel verticalSegmentSettingPanelPrefab;
    public RectTransform segmentParent;
    public float segmentHeight = 150f;
    public float verticalSegmentHeight = 90f;

    private List<SegmentSettingPanel> segmentPanels = new List<SegmentSettingPanel>();
    private List<VerticalSegmentSettingPanel> verticalSegmentPanels = new List<VerticalSegmentSettingPanel>();

    private List<float> curveLength;
    private List<float> curveRadius;
    private List<float> cant;
    private List<bool> cantRotation;
    private List<float> verticalCurveLength;
    private List<float> verticalCurveRadius;

    private List<float> lastCurveLength;
    private List<float> lastCurveRadius;
    private List<float> lastCant;
    private List<bool> lastCantRotation;
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

            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                if (input != null && input.isFocused)
                {
                    for (var n = 0; n < segmentPanels.Count; n++)
                    {
                        if (input == segmentPanels[n].lengthInput)
                        {
                            EventSystem.current.SetSelectedGameObject((n == 0 ? verticalSegmentPanels.Count > 0 ? verticalSegmentPanels[verticalSegmentPanels.Count - 1].radiusInput : segmentPanels[segmentPanels.Count - 1].cantInput : segmentPanels[n - 1].cantInput).gameObject);
                            break;
                        }
                        else if (input == segmentPanels[n].radiusInput)
                        {
                            EventSystem.current.SetSelectedGameObject(segmentPanels[n].lengthInput.gameObject);
                            break;
                        }
                        else if (input == segmentPanels[n].cantInput)
                        {
                            EventSystem.current.SetSelectedGameObject(segmentPanels[n].radiusInput.gameObject);
                            break;
                        }
                    }
                    for (var n = 0; n < verticalSegmentPanels.Count; n++)
                    {
                        if (input == verticalSegmentPanels[n].lengthInput)
                        {
                            EventSystem.current.SetSelectedGameObject((n == 0 ? segmentPanels.Count > 0 ? segmentPanels[segmentPanels.Count - 1].cantInput : verticalSegmentPanels[verticalSegmentPanels.Count - 1].radiusInput : verticalSegmentPanels[n - 1].radiusInput).gameObject);
                            break;
                        }
                        else if (input == verticalSegmentPanels[n].radiusInput)
                        {
                            EventSystem.current.SetSelectedGameObject(verticalSegmentPanels[n].lengthInput.gameObject);
                            break;
                        }
                    }
                }
                else if (verticalSegmentPanels.Count > 0)
                    EventSystem.current.SetSelectedGameObject(verticalSegmentPanels[verticalSegmentPanels.Count - 1].radiusInput.gameObject);
                else if (segmentPanels.Count > 0)
                    EventSystem.current.SetSelectedGameObject(segmentPanels[segmentPanels.Count - 1].cantInput.gameObject);
            }
            else
            {
                if (input != null && input.isFocused)
                {
                    for (var n = 0; n < segmentPanels.Count; n++)
                    {
                        if (input == segmentPanels[n].lengthInput)
                        {
                            EventSystem.current.SetSelectedGameObject(segmentPanels[n].radiusInput.gameObject);
                            break;
                        }
                        else if (input == segmentPanels[n].radiusInput)
                        {
                            EventSystem.current.SetSelectedGameObject(segmentPanels[n].cantInput.gameObject);
                            break;
                        }
                        else if (input == segmentPanels[n].cantInput)
                        {
                            EventSystem.current.SetSelectedGameObject((n == segmentPanels.Count - 1 ? verticalSegmentPanels.Count > 0 ? verticalSegmentPanels[0].lengthInput : segmentPanels[0].lengthInput : segmentPanels[n + 1].lengthInput).gameObject);
                            break;
                        }
                    }
                    for (var n = 0; n < verticalSegmentPanels.Count; n++)
                    {
                        if (input == verticalSegmentPanels[n].lengthInput)
                        {
                            EventSystem.current.SetSelectedGameObject(verticalSegmentPanels[n].radiusInput.gameObject);
                            break;
                        }
                        else if (input == verticalSegmentPanels[n].radiusInput)
                        {
                            EventSystem.current.SetSelectedGameObject((n == verticalSegmentPanels.Count - 1 ? segmentPanels.Count > 0 ? segmentPanels[0].lengthInput : verticalSegmentPanels[0].lengthInput : verticalSegmentPanels[n + 1].lengthInput).gameObject);
                            break;
                        }
                    }
                }
                else if (segmentPanels.Count > 0)
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
        cant = lastCant = Main.editingTracks[0].cant;
        cantRotation = lastCantRotation = Main.editingTracks[0].cantRotation;
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
            var cantL = new List<float>();
            var cantRotationL = new List<bool>();
            foreach (var p in segmentPanels)
            {
                lengthL.Add(Mathf.Max(0f, float.Parse(p.lengthInput.text)));
                radiusL.Add(float.Parse(p.radiusInput.text));
                cantL.Add(Mathf.Min(float.Parse(p.cantInput.text), Mathf.PI / 2f));
                cantRotationL.Add(p.cantRotationToggle.isOn);
            }
            curveLength = lengthL;
            curveRadius = radiusL;
            cant = cantL;
            cantRotation = cantRotationL;
            lengthL = new List<float>();
            radiusL = new List<float>();
            cantL = new List<float>();
            cantRotationL = new List<bool>();
            for (var n = 0; n < curveLength.Count; n++)
            {
                if (curveLength[n] != 0f)
                {
                    lengthL.Add(curveLength[n]);
                    radiusL.Add(curveRadius[n]);
                    cantL.Add(cant[n]);
                    cantRotationL.Add(cantRotation[n]);
                }
            }
            Main.editingTracks[0].curveLength = lengthL;
            Main.editingTracks[0].curveRadius = radiusL;
            Main.editingTracks[0].cant = cantL;
            Main.editingTracks[0].cantRotation = cantRotationL;

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

            Main.editingTracks[0].reloadLength();
            Main.editingTracks[0].reloadEntity();
        }
        catch (FormatException) { }
        catch (OverflowException) { }
    }

    public void save()
    {
        reflect();

        Main.gauge = Main.editingTracks[0].gauge;
        if (Main.editingTracks[0].curveLength.Count == 0)
            Main.INSTANCE.cancelEditingTracks();
        else
            Main.INSTANCE.trackEdited0();
        Main.editingTracks.Clear();
        Main.editingRot = null;

        if (!Input.GetKeyDown(KeyCode.Escape))
            show(false);
    }

    public void cancel()
    {
        Main.editingTracks[0].curveLength = lastCurveLength;
        Main.editingTracks[0].curveRadius = lastCurveRadius;
        Main.editingTracks[0].cant = lastCant;
        Main.editingTracks[0].cantRotation = lastCantRotation;
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
            var p = Instantiate(segmentSettingPanelPrefab);
            p.n = n;
            segmentPanels.Add(p);
            p.transform.SetParent(segmentParent);
            p.transform.localPosition = new Vector3(0f, -n * segmentHeight);
            p.titleText.text = SegmentSettingPanel.segmentText_DEF + " " + (n + 1) + "/" + curveLength.Count;
            p.lengthText.text = SegmentSettingPanel.lengthText_DEF + ": ";
            p.radiusText.text = SegmentSettingPanel.radiusText_DEF + ": ";
            p.cantText.text = SegmentSettingPanel.cantText_DEF + ": ";
            p.cantRotationText.text = SegmentSettingPanel.cantRotationText_DEF + ": ";
            p.lengthInput.text = curveLength[n].ToString();
            p.radiusInput.text = curveRadius[n].ToString();
            p.cantInput.text = cant[n].ToString();
            p.cantRotationToggle.isOn = cantRotation[n];
        }
        foreach (var p in verticalSegmentPanels)
            Destroy(p.gameObject);
        verticalSegmentPanels.Clear();
        for (var n = 0; n < verticalCurveLength.Count; n++)
        {
            var p = Instantiate(verticalSegmentSettingPanelPrefab);
            p.n = n;
            verticalSegmentPanels.Add(p);
            p.transform.SetParent(segmentParent);
            p.transform.localPosition = new Vector3(0f, -curveLength.Count * segmentHeight - n * verticalSegmentHeight);
            p.titleText.text = VerticalSegmentSettingPanel.verticalSegmentText_DEF + " " + (n + 1) + "/" + verticalCurveLength.Count;
            p.lengthText.text = VerticalSegmentSettingPanel.lengthText_DEF + ": ";
            p.radiusText.text = VerticalSegmentSettingPanel.radiusText_DEF + ": ";
            p.lengthInput.text = verticalCurveLength[n].ToString();
            p.radiusInput.text = verticalCurveRadius[n].ToString();
        }
        var size = segmentParent.sizeDelta;
        size.y = curveLength.Count * segmentHeight + verticalCurveLength.Count * verticalSegmentHeight;
        segmentParent.sizeDelta = size;
    }

    public void addSegment(int n = -1)
    {
        if (n == -1)
        {
            curveLength.Add(0f);
            curveRadius.Add(0f);
            cant.Add(0f);
            cantRotation.Add(false);
        }
        else
        {
            curveLength.Insert(n, 0f);
            curveRadius.Insert(n, 0f);
            cant.Insert(n, 0f);
            cantRotation.Insert(n, false);
        }
        reloadSegmentPanels();
        reflect();
    }

    public void removeSegment(int n)
    {
        curveLength.RemoveAt(n);
        curveRadius.RemoveAt(n);
        cant.RemoveAt(n);
        cantRotation.RemoveAt(n);
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
