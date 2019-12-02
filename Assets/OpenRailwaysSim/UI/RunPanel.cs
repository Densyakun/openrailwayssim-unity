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

    // settings by Inspector
    public Text speedText;
    public Text notchText;
    public Text reverserText;

    public Body body; // 操作する運転台のある車体

    void OnEnable()
    {
        Main.INSTANCE.playingPanel.show(false);
        Main.INSTANCE.reloadGrid();
    }

    void OnDisable()
    {
        body = null;
    }

    public void controlOnUpdate()
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
                if (body.reverser < 1)
                    body.reverser += 1;
            }
            else if (Input.GetKeyDown(KeyCode.A))
            {
                if (body.reverser > -1)
                    body.reverser -= 1;
            }

            int scroll = Mathf.FloorToInt(Input.GetAxis("Mouse ScrollWheel") * 10f);
            for (var b = 0; b < (Input.GetKeyDown(KeyCode.W) ? 1 : scroll); b++)
            {
                if (body.brakeNotchs > -body.notch)
                    body.notch -= 1;
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                if (body.notch != 0)
                    body.notch += body.notch < 0 ? 1 : -1;
            }
            for (var b = 0; b < (Input.GetKeyDown(KeyCode.X) ? 1 : -scroll); b++)
            {
                if (body.powerNotchs > body.notch)
                    body.notch += 1;
            }

            float a = 0;
            if (body.motors.Count > 0)
            {
                foreach (var axle in body.motors)
                    a += axle.speed;
                a /= body.motors.Count;
            }
            speedText.text = speedText_DEF + ": " + (body.motors.Count == 0 ? "NaN" : Mathf.Abs(a).ToString("F1") + " km/h");
            notchText.text = notchText_DEF + ": " + body.notch;
            reverserText.text = reverserText_DEF + ": " + (body.reverser == 1 ? "前" : body.reverser == 0 ? "切" : "後");
        }
    }
}
