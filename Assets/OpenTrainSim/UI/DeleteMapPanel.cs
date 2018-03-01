using UnityEngine;

public class DeleteMapPanel : BPPanel {

	void Update () {
		if (Input.GetKeyDown (KeyCode.Escape))
			show (false);
	}

	public void OKButton () {
		BPCanvas.selectMapPanel.Delete ();
		show (false);
	}
}
