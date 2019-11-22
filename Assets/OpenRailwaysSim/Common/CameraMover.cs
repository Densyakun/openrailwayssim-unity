using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// カメラを制御するクラス
/// </summary>
public class CameraMover : MonoBehaviour
{

    public static CameraMover INSTANCE;

    Vector3 lastMousePos = Vector3.zero;
    public bool dragging = false;

    void Awake()
    {
        INSTANCE = this;
    }

    void LateUpdate()
    {
        bool c = Main.playingmap != null &&
            !Main.INSTANCE.pausePanel.isShowing() &&
            !Main.INSTANCE.shapeSettingPanel.isShowing() &&
            !Main.INSTANCE.couplerSettingPanel.isShowing() &&
            !Main.INSTANCE.mapPinSettingPanel.isShowing() &&
            !Main.INSTANCE.structureSettingPanel.isShowing() &&
            !Main.INSTANCE.runPanel.isShowing(); // 操作可能か
        if (c && EventSystem.current.currentSelectedGameObject != null)
        {
            InputField input = EventSystem.current.currentSelectedGameObject.GetComponent<InputField>();
            if (input != null && input.isFocused)
                c = false;
        }
        if (c)
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            Vector3 move = new Vector3(h, 0f, v);
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                move += Vector3.down;
            if (Input.GetKey(KeyCode.Space))
                move += Vector3.up;
            transform.Translate(move * Main.cameraMoveSpeed * Time.deltaTime);

            if (Input.GetMouseButton(0))
            {
                if (Input.GetMouseButtonDown(0))
                    Cursor.lockState = CursorLockMode.Confined;
                else
                {
                    Vector3 m = (Input.mousePosition - lastMousePos) * Main.dragRotSpeed; // カメラの移動量
                    if (m != Vector3.zero)
                    {
                        dragging = true;
                        transform.eulerAngles = new Vector3(Mathf.Clamp(Mathf.Repeat(transform.eulerAngles.x + 180f - m.y, 360f) - 180f, -90f, 90f), transform.eulerAngles.y + m.x);
                    }
                }
                lastMousePos = Input.mousePosition;
            }
            else
            {
                dragging = false;
                Cursor.lockState = CursorLockMode.None;
            }
        }
        else
            Cursor.lockState = CursorLockMode.None;
    }
}
