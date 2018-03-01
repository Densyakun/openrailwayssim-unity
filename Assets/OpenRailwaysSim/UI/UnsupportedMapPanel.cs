using UnityEngine;

public class UnsupportedMapPanel : GamePanel {

	void Update () {
		if (Input.GetKeyDown (KeyCode.Escape))
			show (false);
	}
}
