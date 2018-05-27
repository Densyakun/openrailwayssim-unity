using UnityEngine;

//画面を管理するスクリプト
public class GamePanel : MonoBehaviour {
	
	public void show (bool show) {
		gameObject.SetActive (show);
	}

	public bool isShowing () {
		return gameObject.activeInHierarchy;
	}
}
