using UnityEngine;
using UnityEngine.UI;

public class ScrollItem : MonoBehaviour {
	public int n = -1;
	public ScrollController sc;

	public void click () {
		sc.select (n);
	}
}
