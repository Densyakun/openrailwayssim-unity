using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 運転中に表示される画面。運転中の操作はここに追加する
/// </summary>
public class RunPanel : GamePanel
{

    public static string speedText_DEF = "速度";
    public static string notchText_DEF = "ノッチ";
    public static string reverserText_DEF = "レバーサー";
    public Text speedText;
    public Text notchText;
    public Text reverserText;

    DirectController controller;

    void OnEnable()
    {
        Main.INSTANCE.playingPanel.show(false);
        Main.INSTANCE.reloadGrid();
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
            Main.INSTANCE.reloadGrid();
            Main.INSTANCE.playingPanel.show(true);
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                if (controller.reverser < 1)
                    controller.reverser += 1;
            }
            else if (Input.GetKeyDown(KeyCode.A))
            {
                if (controller.reverser > -1)
                    controller.reverser -= 1;
            }

            int scroll = Mathf.FloorToInt(Input.GetAxis("Mouse ScrollWheel") * 10f);
            for (var b = 0; b < (Input.GetKeyDown(KeyCode.W) ? 1 : scroll); b++)
            {
                if (controller.brakeNotchs > -controller.notch)
                    controller.notch -= 1;
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                if (controller.notch != 0)
                    controller.notch += controller.notch < 0 ? 1 : -1;
            }
            for (var b = 0; b < (Input.GetKeyDown(KeyCode.X) ? 1 : -scroll); b++)
            {
                if (controller.powerNotchs > controller.notch)
                    controller.notch += 1;
            }

            float a = 0;
            if (controller.axles.Count > 0)
            {
                foreach (var axle in controller.axles)
                    a += axle.speed;
                a /= controller.axles.Count;
            }
            speedText.text = speedText_DEF + ": " + (controller.axles.Count == 0 ? "NaN" : Mathf.Abs(a).ToString("F1") + " km/h");
            notchText.text = notchText_DEF + ": " + controller.notch;
            reverserText.text = reverserText_DEF + ": " + (controller.reverser == 1 ? "前" : controller.reverser == 0 ? "切" : "後");
        }
    }
}
