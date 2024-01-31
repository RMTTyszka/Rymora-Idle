using UnityEngine;

public class CameraManager : MonoBehaviour
{
 
    public int Speed = 2;
    public float zoomlimit = -125;
    public float zoomlimitIn = -80;
    public float leftLimit = -10;
    public float rightLimit = 10;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float xAxisValue = Input.GetAxis("Horizontal") * Speed;
        float zAxisValue = Input.GetAxis("Vertical") * Speed;
 
        float zoomDistance = transform.position.z + zAxisValue;
        float viewDistance = transform.position.x + xAxisValue;
        if (zoomDistance > zoomlimit && zoomDistance < zoomlimitIn && viewDistance > leftLimit && viewDistance < rightLimit)
        {
            transform.position = new Vector3(transform.position.x + xAxisValue, transform.position.y, zoomDistance);
        }
    }
}
