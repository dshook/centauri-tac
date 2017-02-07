using UnityEngine;

public class SplineDecorator : MonoBehaviour {

    public BezierSpline spline;

    public int frequency;

    public bool lookForward;

    public Transform[] items;

    public bool addWalker;
    public SplineWalkerMode mode;

    [Range(0, 1f)]
    public float scaleMargin = 0.1f;
    public bool scaleInOut = false;

    private void Awake () {
        if (frequency <= 0 || items == null || items.Length == 0) {
            return;
        }
        float stepSize = frequency * items.Length;
        if (spline.Loop || stepSize == 1) {
            stepSize = 1f / stepSize;
        }
        else {
            stepSize = 1f / (stepSize - 1);
        }
        for (int p = 0, f = 0; f < frequency; f++) {
            for (int i = 0; i < items.Length; i++, p++) {
                Transform item = Instantiate(items[i]) as Transform;
                Vector3 position = spline.GetPoint(p * stepSize);
                item.transform.position = position;
                if (lookForward) {
                    item.transform.LookAt(position + spline.GetDirection(p * stepSize));
                }
                item.transform.parent = transform;

                if (addWalker)
                {
                    var walker = item.gameObject.AddComponent<SplineWalker>();
                    walker.lookForward = lookForward;
                    walker.mode = mode;
                    walker.spline = spline;
                    walker.progress = p * stepSize;
                    walker.scaleInOut = scaleInOut;
                    walker.scaleMargin = scaleMargin;
                }
            }
        }
    }
}