using UnityEngine;

public class GamePanel : MonoBehaviour {
	
	public void show (bool show) {
		gameObject.SetActive (show);
	}

	public bool isShowing () {
		return gameObject.activeInHierarchy;
	}
}
