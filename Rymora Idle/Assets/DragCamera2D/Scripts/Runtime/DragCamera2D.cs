using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DragCamera2D : MonoBehaviour
{
    /*
     *TODO: 
     *  DONE: replace dolly with bezier dolly system
     *  DONE: add dolly track smoothing (Pro Feature)
     *  DONE: add dolly track straightening  (Pro Feature)
     *  DONE: Dolly track + gizmo colours
     *  DONE: add non tracked constant speed dolly system(continuous movement based on time)  (Pro Feature)
     *  WONTDO: [REPLACED BY FEATURE BELOW] add button to split dolly track evenly (between start and end) for time based dolly movement
     *  DONE: button to adjust times on all waypoints so camera moves at a constant speed  (Pro Feature)
     *  DONE: add per waypoint time (seconds on this segment)  (Pro Feature)
     *  DONE: add scaler for time to next waypoint in scene viewe gui  (Pro Feature)
     *  improve GUI elements (full custon editor inspector)
     *  DONE:    add waypoint gui  scene view button  (Pro Feature)
     *  DONE: better designed example scenes
     *  DONE: option to lock camera to track even if object escapes area
     *  add multiple dolly tracks to allow creating loops etc
     *  add track change triggers
     *  add bounds ids for multiple bounds
     *  add bounds triggers(e.g. small bounds until x event(obtain key etc) then larger bounds
    */

    public Camera cam;

    [Header("Camera Movement")]

    [Tooltip("Allow the Camera to be dragged.")]
    public bool dragEnabled = true;
    [Range(-5, 5)]
    [Tooltip("Speed the camera moves when dragged.")]
    public float dragSpeed = -0.06f;

    [Tooltip("Pixel Border to trigger edge scrolling")]
    public int edgeBoundary = 20;
    [Range(0, 10)]
    [Tooltip("Speed the camera moves Mouse enters screen edge.")]
    public float edgeSpeed = 1f;

    [Header("Touch & Keyboard Input")]
    [Tooltip("Enable or disable Keyboard input")]
    public bool keyboardInput = false;
    [Tooltip("Invert keyboard direction")]
    public bool inverseKeyboard = false;
    [Tooltip("Enable or disable touch input (Pro Only)")]
    public bool touchEnabled = false;
    [Tooltip("Drag Speed for touch controls (Pro Only)")]
    [Range(-5,5)]
    public float touchDragSpeed = -0.03f;

    [Header("Zoom")]
    [Tooltip("Enable or disable zooming")]
    public bool zoomEnabled = true;
    [Tooltip("Scale drag movement with zoom level")]
    public bool linkedZoomDrag = true;
    [Tooltip("Maximum Zoom Level")]
    public float maxZoom = 10;
    [Tooltip("Minimum Zoom Level")]
    [Range(0.01f, 10)]
    public float minZoom = 0.5f;
    [Tooltip("The Speed the zoom changes")]
    [Range(0.1f, 10f)]
    public float zoomStepSize = 0.5f;
    [Tooltip("Enable Zooming to mouse pointer (Pro Only)")]
    public bool zoomToMouse = false;

   


    [Header("Follow Object")]
    public GameObject followTarget;
    [Range(0.01f,1f)]
    public float lerpSpeed = 0.5f;
    public Vector3 offset = new Vector3(0,0,-10);


    [Header("Camera Bounds")]
    public bool clampCamera = true;
    public CameraBounds bounds; 
    public Dc2dDolly dollyRail;


    // private vars
    Vector3 bl;
    Vector3 tr;
    private Vector2 touchOrigin = -Vector2.one;

    void Start() {
        if (cam == null) {
            cam = Camera.main;
        }
    }

    void Update() {
        if (dragEnabled) {
            panControl();
        }

        if (edgeBoundary > 0) {
            edgeScroll();
        }


        if (zoomEnabled) {
            zoomControl();
        }

        if (followTarget != null) {
            transform.position = Vector3.Lerp(transform.position + offset, followTarget.transform.position + offset, lerpSpeed);
        }


        if (clampCamera) {
            cameraClamp();
        }

        if (touchEnabled) {
            doTouchControls();
        }

        if(dollyRail != null) {
            stickToDollyRail();
        }
       
    }

    private void edgeScroll() {
        float x = 0;
        float y = 0;
        if (Input.mousePosition.x >= Screen.width - edgeBoundary) {
            // Move the camera
            x = Time.deltaTime * edgeSpeed;
        }
        if (Input.mousePosition.x <= 0 + edgeBoundary) {
            // Move the camera
            x = Time.deltaTime * -edgeSpeed;
        }
        if (Input.mousePosition.y >= Screen.height - edgeBoundary) {
            // Move the camera
            y = Time.deltaTime * edgeSpeed
;
        }
        if (Input.mousePosition.y <= 0 + edgeBoundary) {
            // Move the camera
            y =  Time.deltaTime * -edgeSpeed
;
        }
        transform.Translate(x, y, 0);
    }

    public void addCameraDolly() {
        if (dollyRail == null) {
            GameObject go = new GameObject("Dolly");
            Dc2dDolly dolly = go.AddComponent<Dc2dDolly>();

            Dc2dWaypoint wp1 = new Dc2dWaypoint();
            wp1.position = new Vector3(0, 0, 0);

            Dc2dWaypoint wp2 = new Dc2dWaypoint();
            wp2.position = new Vector3(1, 0, 0);

            Dc2dWaypoint[] dc2dwaypoints = new Dc2dWaypoint[2];
            dc2dwaypoints[0] = wp1;
            dc2dwaypoints[1] = wp2;
            wp1.endPosition = wp2.position;

            dolly.allWaypoints = dc2dwaypoints;

            this.dollyRail = dolly;

            Selection.activeGameObject = go;
            SceneView.FrameLastActiveSceneView();
        }
    }

    public void addCameraBounds() {
        if (bounds == null) {
            GameObject go = new GameObject("CameraBounds");
            CameraBounds cb = go.AddComponent<CameraBounds>();
            cb.guiColour = new Color(0,0,1f,0.1f);
            cb.pointa = new Vector3(20,20,0);
            this.bounds = cb;
            EditorUtility.SetDirty(this);
        }
    }

    public void doTouchControls() {
        // Pro
    }

    //click and drag
    public void panControl() {
        // if keyboard input is allowed
        if (keyboardInput) {
            float x = -Input.GetAxis("Horizontal") * dragSpeed;
            float y = -Input.GetAxis("Vertical") * dragSpeed;

            if (linkedZoomDrag) {
                x *= Camera.main.orthographicSize;
                y *= Camera.main.orthographicSize;
            }

            if (inverseKeyboard) {
                x = -x;
                y = -y;
            }
            transform.Translate(x, y, 0);
        }



       // if mouse is down
        if (Input.GetMouseButton(0)) {
            float x = Input.GetAxis("Mouse X") * dragSpeed;
            float y = Input.GetAxis("Mouse Y") * dragSpeed;

            if (linkedZoomDrag) {
                x *= Camera.main.orthographicSize;
                y *= Camera.main.orthographicSize;
            }

            transform.Translate(x, y, 0);
        }

        
    }

    private void clampZoom() {
        Camera.main.orthographicSize =  Mathf.Clamp(Camera.main.orthographicSize, minZoom, maxZoom);
        Mathf.Max(cam.orthographicSize, 0.1f);


    }

    void ZoomOrthoCamera(Vector3 zoomTowards, float amount) {
        // Calculate how much we will have to move towards the zoomTowards position
        float multiplier = (1.0f / Camera.main.orthographicSize * amount);
        // Move camera
        transform.position += (zoomTowards - transform.position) * multiplier;
        // Zoom camera
        Camera.main.orthographicSize -= amount;
        // Limit zoom
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, minZoom, maxZoom);
    }

    // managae zooming
    public void zoomControl() {
        if (Input.GetAxis("Mouse ScrollWheel") > 0) // forward
        {
            Camera.main.orthographicSize = Camera.main.orthographicSize - zoomStepSize;
        }

        if (Input.GetAxis("Mouse ScrollWheel") < 0) // back            
        {
            Camera.main.orthographicSize = Camera.main.orthographicSize + zoomStepSize;
        }
        clampZoom();
    }

    // Clamp Camera to bounds
    private void cameraClamp() {
        tr = cam.ScreenToWorldPoint(new Vector3(cam.pixelWidth, cam.pixelHeight, -transform.position.z));
        bl = cam.ScreenToWorldPoint(new Vector3(0, 0, -transform.position.z));

        if(bounds == null) {
            Debug.Log("Clamp Camera Enabled but no Bounds has been set.");
            return;
        }

        float boundsMaxX = bounds.pointa.x;
        float boundsMinX = bounds.transform.position.x;
        float boundsMaxY = bounds.pointa.y;
        float boundsMinY = bounds.transform.position.y;

        if (tr.x > boundsMaxX) {
            transform.position = new Vector3(transform.position.x - (tr.x - boundsMaxX), transform.position.y, transform.position.z);
        }

        if (tr.y > boundsMaxY) {
            transform.position = new Vector3(transform.position.x, transform.position.y - (tr.y - boundsMaxY), transform.position.z);
        }

        if (bl.x < boundsMinX) {
            transform.position = new Vector3(transform.position.x + (boundsMinX - bl.x), transform.position.y, transform.position.z);
        }

        if (bl.y < boundsMinY) {
            transform.position = new Vector3(transform.position.x, transform.position.y + (boundsMinY - bl.y), transform.position.z);
        }
    }

    public void stickToDollyRail() {
        if(dollyRail != null && followTarget != null) {
            Vector3 campos = dollyRail.getPositionOnTrack(followTarget.transform.position);
            transform.position = new Vector3(campos.x, campos.y, transform.position.z);
        }
    }
}
