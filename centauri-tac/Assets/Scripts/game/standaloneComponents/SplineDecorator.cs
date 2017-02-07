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
        float stepSize =  calcStepSize(frequency, items.Length);

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

    private float calcStepSize(float freq, int numItems)
    {
        float stepSize = freq * numItems;
        if (spline.Loop || stepSize == 1) {
            stepSize = 1f / stepSize;
        }
        else {
            stepSize = 1f / (stepSize - 1);
        }
        return stepSize;
    }

    public void SetFrequency(int newFrequency)
    {
        if(newFrequency == frequency) return;

        if (newFrequency > frequency)
        {
            for (int f = 0; f < newFrequency - frequency; f++) {
                for (int i = 0; i < items.Length; i++) {
                    Transform item = Instantiate(items[i]) as Transform;
                    item.transform.parent = transform;

                    if (addWalker)
                    {
                        var walker = item.gameObject.AddComponent<SplineWalker>();
                        walker.lookForward = lookForward;
                        walker.mode = mode;
                        walker.spline = spline;
                        walker.scaleInOut = scaleInOut;
                        walker.scaleMargin = scaleMargin;
                    }
                }
            }
        }
        else
        {
            int curChildCount = transform.childCount;
            for (int i = curChildCount - 1; i >= newFrequency * items.Length; i--)
            {
                GameObject.DestroyImmediate(transform.GetChild(i).gameObject);
            }
        }

        //update everyone's position
        int childCount = transform.childCount;
        float stepSize =  calcStepSize(newFrequency, items.Length);
        for (int i = 0; i < childCount; i++)
        {
            var child = transform.GetChild(i).gameObject;

            Vector3 position = spline.GetPoint(i * stepSize);
            child.transform.position = position;
            if (lookForward) {
                child.transform.LookAt(position + spline.GetDirection(i * stepSize));
            }
            var walker = child.GetComponent<SplineWalker>();
            if (walker != null)
            {
                walker.progress = i * stepSize;
            }
        }

        frequency = newFrequency;
    }
}