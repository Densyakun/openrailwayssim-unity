using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// カメラを制御するクラス
/// </summary>
public class CameraMover : MonoBehaviour
{

    public static float maxDistance = 5f; // カメラの追跡が遅れたときに対象から離れない距離
    public static float min_t = 1f / 5f; // カメラの追跡力(0=動かない、1=瞬時に追跡)
    public static float free_time = 0.8f; // 手動回転後に自動回転するまでの時間
    public static CameraMover INSTANCE;

    //Transform target;
    Vector3 pos;
    Quaternion rot;
    //float f = free_time;
    Vector3 lastMousePos = Vector3.zero;
    Vector3 rotStartEuler = Vector3.zero;
    //bool a = true;
    //float lv = 0f;
    public bool dragging { get; private set; }

    void Awake()
    {
        INSTANCE = this;
    }

    // カメラは後からついてくる挙動になっており、カメラが一定距離以上離れないようになっているため、乗り物向けなカメラになっている。
    void LateUpdate()
    {
        bool c = Main.playingmap != null && !Main.pause && !Main.INSTANCE.runPanel.isShowing(); // 操作可能か
        if (c && EventSystem.current.currentSelectedGameObject != null)
        {
            InputField input = EventSystem.current.currentSelectedGameObject.GetComponent<InputField>();
            if (input != null && input.isFocused)
                c = false;
        }
        if (c)
        {
            //if (target == null) {
            /*if (Main.masterPlayer != null && Main.masterPlayer.playerEntity != null)
                target = Main.masterPlayer.playerEntity.transform;*/

            Vector3 move = Vector3.zero;
            if (Input.GetKey(KeyCode.A))
                move += Vector3.left;
            if (Input.GetKey(KeyCode.D))
                move += Vector3.right;
            if (Input.GetKey(KeyCode.S))
                move += Vector3.back;
            if (Input.GetKey(KeyCode.W))
                move += Vector3.forward;
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                move += Vector3.down;
            if (Input.GetKey(KeyCode.Space))
                move += Vector3.up;
            transform.Translate(move * Main.cameraMoveSpeed * Time.deltaTime);
            //}

            //float h = Input.GetAxis ("Horizontal");
            //float v = Input.GetAxis ("Vertical");

            if (Input.GetMouseButton(0))
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Cursor.lockState = CursorLockMode.Confined;
                    lastMousePos = Input.mousePosition;
                    rotStartEuler = rot.eulerAngles;
                    //a = false;
                    //f = 0;
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
            {
                dragging = false;
                /*if (h == 0.0f && (lv < 0.0f) != (v < 0.0f)) {
					a = false;
					f = 0;
				} else {
					a = true;
				}*/
            }

            /*if (f >= free_time) {
				if (v < 0.0f) {
					rot = Quaternion.Euler (new Vector3 (target.rotation.eulerAngles.x, Mathf.Repeat (target.rotation.eulerAngles.y, 360f) - 180f, target.rotation.eulerAngles.z));
				} else {
					rot = target.rotation;
				}
			} else if (a) {
				f += Time.deltaTime;
			}

			pos = target.position + rot * CAMERA_POS;

			float x = pos.x - transform.position.x;
			float y = pos.y - transform.position.y;
			float z = pos.z - transform.position.z;
			float t = Mathf.Max (min_t, 1f - maxDistance / Mathf.Sqrt (x * x + y * y + z * z));
			transform.position = new Vector3 (Mathf.Lerp (transform.position.x, pos.x, t), Mathf.Lerp (transform.position.y, pos.y, t), Mathf.Lerp (transform.position.z, pos.z, t));
			transform.eulerAngles = new Vector3 (Mathf.Repeat (transform.eulerAngles.x + CAMERA_ANGLE.x + (Mathf.Repeat (rot.eulerAngles.x - transform.eulerAngles.x - CAMERA_ANGLE.x + 180f, 360f) - 180f) / 2 + 180f, 360f) - 180f,
				Mathf.Repeat (transform.eulerAngles.y + CAMERA_ANGLE.y + (Mathf.Repeat (rot.eulerAngles.y - transform.eulerAngles.y - CAMERA_ANGLE.y + 180f, 360f) - 180f) / 2 + 180f, 360f) - 180f,
				Mathf.Repeat (transform.eulerAngles.z + CAMERA_ANGLE.z + (Mathf.Repeat (rot.eulerAngles.z - transform.eulerAngles.z - CAMERA_ANGLE.z + 180f, 360f) - 180f) / 2 + 180f, 360f) - 180f);

			lv = v;*/

            transform.eulerAngles = new Vector3(Mathf.Repeat(transform.eulerAngles.x + (Mathf.Repeat(rot.eulerAngles.x - transform.eulerAngles.x + 180f, 360f) - 180f) / 2 + 180f, 360f) - 180f,
                Mathf.Repeat(transform.eulerAngles.y + (Mathf.Repeat(rot.eulerAngles.y - transform.eulerAngles.y + 180f, 360f) - 180f) / 2 + 180f, 360f) - 180f,
                Mathf.Repeat(transform.eulerAngles.z + (Mathf.Repeat(rot.eulerAngles.z - transform.eulerAngles.z + 180f, 360f) - 180f) / 2 + 180f, 360f) - 180f);

            /*if (Input.GetMouseButtonUp (0)) {
				Cursor.lockState = CursorLockMode.None;
			}*/
        }
    }
}
