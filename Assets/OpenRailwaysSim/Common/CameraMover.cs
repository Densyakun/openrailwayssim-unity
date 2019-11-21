using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// カメラを制御するクラス
/// </summary>
public class CameraMover : MonoBehaviour
{

    public static CameraMover INSTANCE;

    Quaternion rot;
    Vector3 lastMousePos = Vector3.zero;
    Vector3 rotStartEuler = Vector3.zero;
    public bool dragging { get; private set; }

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
                {
                    Cursor.lockState = CursorLockMode.Confined;
                    lastMousePos = Input.mousePosition;
                    rotStartEuler = rot.eulerAngles;
                }

                Vector3 m = (Input.mousePosition - lastMousePos) * Main.dragRotSpeed; // カメラの移動量
                if (m != Vector3.zero)
                {
                    dragging = true;

                    Quaternion r = Quaternion.Euler(new Vector3(Mathf.Clamp(Mathf.Repeat(rotStartEuler.x + 180, 360) - m.y, 90, 270) - 180, rotStartEuler.y + m.x));
                    if (r.eulerAngles.z == 0)
                        rot = r;
                }
            }
            else
                dragging = false;

            transform.eulerAngles = new Vector3(Mathf.Repeat(transform.eulerAngles.x + (Mathf.Repeat(rot.eulerAngles.x - transform.eulerAngles.x + 180f, 360f) - 180f) / 2 + 180f, 360f) - 180f,
                Mathf.Repeat(transform.eulerAngles.y + (Mathf.Repeat(rot.eulerAngles.y - transform.eulerAngles.y + 180f, 360f) - 180f) / 2 + 180f, 360f) - 180f,
                Mathf.Repeat(transform.eulerAngles.z + (Mathf.Repeat(rot.eulerAngles.z - transform.eulerAngles.z + 180f, 360f) - 180f) / 2 + 180f, 360f) - 180f);
        }
    }
}
