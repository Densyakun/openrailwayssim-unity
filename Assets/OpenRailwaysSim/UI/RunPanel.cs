using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RunPanel : GamePanel
{

    public static string speedText_DEF = "速度";
    public Text speedText;

    DirectController controller;

    void OnEnable()
    {
        GameCanvas.playingPanel.show(false);
        foreach (var obj in Main.selectingObjs)
        {
            if (obj is DirectController)
            {
                controller = (DirectController)obj;
                break;
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            show(false);
            GameCanvas.playingPanel.show(true);
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.A))
                controller.notch = 0;
            else if (Input.GetKeyDown(KeyCode.Z))
                controller.notch = 1;

            float a = 0;
            if (controller.axles.Count > 0)
            {
                foreach (var axle in controller.axles)
                    a += axle.speed;
                a /= controller.axles.Count;
            }
            speedText.text = speedText_DEF + ": " + (controller.axles.Count == 0 ? "NaN" : a.ToString() + " km/h");
        }
    }
}
