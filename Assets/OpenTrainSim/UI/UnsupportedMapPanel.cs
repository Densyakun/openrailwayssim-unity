using UnityEngine;

public class UnsupportedMapPanel : BPPanel {

	void Update () {
		if (Input.GetKeyDown (KeyCode.Escape))
			show (false);
	}
}
