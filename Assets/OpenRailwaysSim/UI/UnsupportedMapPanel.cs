using UnityEngine;

//対応していないマップを開いたときに表示される画面
public class UnsupportedMapPanel : GamePanel {

	void Update () {
		if (Input.GetKeyDown (KeyCode.Escape))
			show (false);
	}
}
