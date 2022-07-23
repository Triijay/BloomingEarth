using UnityEngine;

public class Billboard : MonoBehaviour
{
    // Update is called once per frame
    void LateUpdate()
    {
        // Rotate Transform Front to Camera
        transform.LookAt(Camera.main.transform.position, Vector3.up);

    }
}
