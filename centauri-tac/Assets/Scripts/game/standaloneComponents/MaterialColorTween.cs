using UnityEngine;

public class MaterialColorTween : MonoBehaviour
{
    public Material mat;
    public Color desiredColor;
    public float time = 1f;

    Color originalColor;
    float timeAccum = 0f;
    void Start()
    {
        originalColor = mat.color;
    }

    void Update()
    {
        if (mat.color == desiredColor)
        {
            Destroy(this);
            return;
        }
        timeAccum += Time.deltaTime;
        var progress = timeAccum / time;
        progress = Mathf.Min(1f, progress);
        mat.color = Color.Lerp(originalColor, desiredColor, progress);

        if (progress == 1f)
        {
            Destroy(this);
        }
    }
}
