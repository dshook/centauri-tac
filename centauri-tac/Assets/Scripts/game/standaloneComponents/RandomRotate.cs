using UnityEngine;

[ExecuteInEditMode]
public class RandomRotate : MonoBehaviour
{
    public Vector3 rotation = Vector3.zero;

    private Quaternion originalRotation;

    void Start()
    {
        originalRotation = transform.rotation;
    }

    void Update()
    {
        transform.Rotate(transform.up, rotation.y * Time.deltaTime);
        transform.Rotate(transform.right, rotation.x * Time.deltaTime);
        transform.Rotate(transform.forward, rotation.z * Time.deltaTime);
    }
}
