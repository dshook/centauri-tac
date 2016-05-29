using UnityEngine;
using System.Collections;
using TMPro;

[ExecuteInEditMode]
public class SplineText : MonoBehaviour
{

    public float curveScale = 1.0f;
    //public float length; // Currently Only Informational
    public BezierSpline vertexCurve;
    public TextMeshPro m_TextComponent;

    void Awake()
    {
        // Make sure I have the thigs I need to get the data to deform text
        if (m_TextComponent == null)
            m_TextComponent = gameObject.GetComponent<TextMeshPro>();
        if (vertexCurve == null)
            vertexCurve = gameObject.GetComponent<BezierSpline>();
    }
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }


    void OnRenderObject()
    {
        // Make sure I have the thigs I need to get the data to deform text
        if (m_TextComponent == null)
            m_TextComponent = gameObject.GetComponent<TextMeshPro>();
        if (vertexCurve == null)
            vertexCurve = gameObject.GetComponent<BezierSpline>();

        if (m_TextComponent)
        {

            Vector3[] vertexPositions;

            m_TextComponent.renderMode = TextRenderFlags.Render;
            m_TextComponent.ForceMeshUpdate();
            m_TextComponent.renderMode = TextRenderFlags.DontRender;

            TMP_TextInfo textInfo = m_TextComponent.textInfo;
            int characterCount = textInfo.characterCount;

            if (characterCount >= 0)
            {
                vertexPositions = textInfo.meshInfo[0].vertices;

                float boundsMaxX = m_TextComponent.rectTransform.rect.width * 0.5f;
                float boundsMinX = -boundsMaxX;

                for (int i = 0; i < characterCount; i++)
                {
                    if (!textInfo.characterInfo[i].isVisible)
                        continue;

                    int vertexIndex = textInfo.characterInfo[i].vertexIndex;

                    // Compute the baseline mid point for each character
                    Vector3 offsetToMidBaseline = new Vector3((vertexPositions[vertexIndex + 0].x + vertexPositions[vertexIndex + 2].x) / 2, textInfo.characterInfo[i].baseLine);

                    Vector3 c0 = vertexPositions[vertexIndex + 0] - offsetToMidBaseline;
                    Vector3 c1 = vertexPositions[vertexIndex + 1] - offsetToMidBaseline;
                    Vector3 c2 = vertexPositions[vertexIndex + 2] - offsetToMidBaseline;
                    Vector3 c3 = vertexPositions[vertexIndex + 3] - offsetToMidBaseline;

                    float t = (offsetToMidBaseline.x - boundsMinX) / (boundsMaxX - boundsMinX);
                    Vector3 point = transform.InverseTransformPoint(vertexCurve.GetPoint(t)) * curveScale;
                    Vector3 xAxis = transform.InverseTransformDirection(vertexCurve.GetVelocity(t)).normalized;
                    Vector3 yAxis = (Vector3.up - xAxis * xAxis.y).normalized;

                    vertexPositions[vertexIndex + 0] = point + c0.x * xAxis + c0.y * yAxis;
                    vertexPositions[vertexIndex + 1] = point + c1.x * xAxis + c1.y * yAxis;
                    vertexPositions[vertexIndex + 2] = point + c2.x * xAxis + c2.y * yAxis;
                    vertexPositions[vertexIndex + 3] = point + c3.x * xAxis + c3.y * yAxis;
                }

                // Upload the mesh with the revised information
                m_TextComponent.mesh.vertices = vertexPositions;
                m_TextComponent.mesh.uv = textInfo.meshInfo[0].uvs0;
                m_TextComponent.mesh.uv2 = textInfo.meshInfo[0].uvs2;
                m_TextComponent.mesh.colors32 = textInfo.meshInfo[0].colors32;

                m_TextComponent.mesh.RecalculateBounds(); // We need to update the bounds of the text object.
            }
        }
    }

}