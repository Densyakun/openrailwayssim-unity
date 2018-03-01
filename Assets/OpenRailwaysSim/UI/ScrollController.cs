using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollController : MonoBehaviour {
	public interface Listener
	{
		void Select (ScrollController sc);
	}
	public ScrollItem prefab;
	public int n = -1;
	public int height = 30;
	public List<Listener> listeners = new List<Listener> ();

	public void setContents (string[] contents) {
		RectTransform[] a = gameObject.GetComponentsInChildren<RectTransform> ();
		for (int b = 1; b < a.Length; b++) {
			Destroy (a [b].gameObject);
		}
		((RectTransform)transform).sizeDelta = new Vector2 (((RectTransform)transform).sizeDelta.x, height * contents.Length);
		for (int n = 0; n < contents.Length; n++) {
			ScrollItem item = GameObject.Instantiate (prefab);
			item.name += contents [n];
			item.sc = this;
			item.n = n;

			item.transform.SetParent (transform, false);
			((RectTransform)item.transform).anchoredPosition = new Vector2 (((RectTransform)item.transform).anchoredPosition.x, -height * n);

			Text text = item.GetComponentInChildren<Text> ();
			if (text != null) {
				text.text = contents [n];
			}
		}
	}

	public void select (int n) {
		this.n = n;
		for (int a = 0; a < listeners.Count; a++) {
			listeners [a].Select (this);
		}
	}
}
