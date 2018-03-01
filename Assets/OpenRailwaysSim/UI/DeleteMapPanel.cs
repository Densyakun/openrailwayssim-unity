using UnityEngine;

public class DeleteMapPanel : GamePanel {

	void Update () {
		if (Input.GetKeyDown (KeyCode.Escape))
			show (false);
	}

	public void OKButton () {
		GameCanvas.selectMapPanel.Delete ();
		show (false);
	}
}
