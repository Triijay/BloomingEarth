using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// CameraControls is all about the Camera and how the User is able to control it
/// </summary>
public class CameraControls : MonoBehaviour {

    #region Variables
    /// <summary>
    /// Speed at wich the Camera Moves. This is a Constant Multiplicator
    /// </summary>
    [Tooltip("Speed at wich the Camera Moves")]
    [Range(0.5f, 20.0f)]
    public float moveSpeed;

    /// <summary>
    /// How much the Height of the Camera influences the Cameraspeed
    /// </summary>
    [Tooltip("How much the Height of the Camera influences the Cameraspeed")]
    [Range(0.2f, 2.0f)]
    public float heightSpeedRatio;

    /// <summary>
    /// Min Height evaluated by heightSpeedRatio - So the camery will not be completely slow on lower Y Values
    /// </summary>
    [Tooltip("Min Height evaluated by heightSpeedRatio - So the camery will not be completely slow on lower Y Values")]
    [Range(10f, 20f)]
    public float minHeightSpeed;

    /// <summary>
    /// Speed at wich the Camera Zooms. This is a Constant Multiplicator
    /// </summary>
    [Tooltip("Speed at wich the Camera Zooms")]
    [Range(0.03f, 0.3f)]
    public float zoomSpeed;

    /// <summary>
    /// Constraints for zooming (for y position of Camera)
    /// </summary>
    [Tooltip("Constraints for zooming (for y position of Camera)")]
    public int zoomMin,
               zoomMax;

    /// <summary>
    /// Map Boundaries
    /// </summary>
    [Tooltip("Map Boundaries")]
    public int BoundaryMinX,
               BoundaryMaxX,
               BoundaryMinZ,
               BoundaryMaxZ;

    /// <summary>
    /// Map Boundaries for Device Orientation
    /// </summary>
    private int BoundaryMinZ_Orientation,
                BoundaryMaxZ_Orientation;


    /// <summary>
    /// The amount the Rotation makes
    /// </summary>
    [Tooltip("The amount the Rotation makes")]
    [Range(0.1f, 0.95f)]
    public float rotationAmount;

    /// <summary>
    /// Lower Value: Camera is more top-down on highest zoom
    /// Higher Value: Camera is more flat on highest zoom
    /// </summary>
    [Tooltip("When the Rotation of the camera starts when zooming")]
    [Range(0, 45)]
    public float rotationTransformOffset;

    /// <summary>
    /// When we are zooming in and the camera is rotating simultaneously, camera are flying into the horizon,
    /// because the orientationpoint of the camera is raising
    /// with that correction we are forcing the camera on zoom towards the ground 
    /// </summary>
    [Tooltip("Y Axis Correction on Zoom")]
    [Range(0.1f, 0.95f)]
    public float rotationCorrectionY;

    /// <summary>
    /// The Default CameraXRotation used in many Calculations
    /// </summary>
    private float defaultCameraXRotation;

    /// <summary>
    /// This is our Rigidbody Object for the Camera
    /// </summary>
    public new Rigidbody rigidbody;

    /// <summary>
    /// This is our Camera Component
    /// </summary>
    public Camera camComponent;

    /// <summary>
    /// If our animation is still running this is true
    /// </summary>
    private bool moving = false;

    /// <summary>
    /// Animation speed for the zoomin animation (additional building info panel)
    /// </summary>
    [Tooltip("Animation speed for the zoomin animation (additional building info panel)")]
    public float animationSpeed = 0.6f;

    /// <summary>
    /// The Vector to where we are currently animating
    /// </summary>
    private Vector3 movingTo;

    /// <summary>
    /// The Rotation we are currently animating to
    /// </summary>
    private float rotatexTo;

    /// <summary>
    /// The Magnitude between the start and end Vector to use in the speed calculation
    /// to keep both animations (movement and rotation) synchron
    /// </summary>
    private float magspeedTo;

    /// <summary>
    /// The Angle between the start and end Quaternion to use in the speed calculation
    /// to keep both animations (movement and rotation) synchron
    /// </summary>
    private float rotationmagspeedTo;

    /// <summary>
    /// Disable Cameracontrols while zoomed into a building
    /// </summary>
    private bool zoomedToBuilding = false;

    /// <summary>
    /// TODO Descr
    /// </summary>
    private const int movetoBuildingX = 30;

    /// <summary>
    /// The Camera Position before we are zooming into BuildingMenu
    /// </summary>
    private Vector3 moveToBuildingOriginalCameraPosition;

    /// <summary>
    /// Vector 3 with the additional translation, when the camera movesToBuilding
    /// </summary>
    private Vector3 moveToBuildingVec3Translation = new Vector3(movetoBuildingX, 5, 12);

    /// <summary>
    /// Vector 3 with the desired Ending-Position, when the camera movesToBuilding
    /// </summary>
    private Vector3 moveToBuildingVec3;
    
    /// <summary>
    /// When in Portrait, the User is able to zoom out more, so that he can see more
    /// </summary>
    private int portraitBonusMaxZoom = 0;


    // Performance Optimization
    private List<RaycastResult> raycastResults = new List<RaycastResult>();
    private PointerEventData pointer;
    private Touch touchZero, touchOne;
    private Vector2 touchZeroPrevPos, touchOnePrevPos;
    private Vector3 directionVector, initialPanHit;
    float prevTouchDeltaMag, touchDeltaMag, deltaMagnitudeDiff, heightBasedRotationAdditionX, rotateX, angle, magnitude, clampedTransformPositionY, planeEnter;
    bool touchStartedOnUI = true;
    Ray ray;
    Plane plane;
    Building buildingMovedTo;
    Quaternion rotateto;

    #endregion

    /// <summary>
    /// Start is called before the first frame update
    /// </summary>
    void Start() {

        // Set a plane on zero to check where the touch enters
        plane = new Plane(Vector3.up, Vector3.zero);
    }


    private void OnGUI() {
        // If an animation is running
        if (moving) {
            // Call the next step of the animation
            moveTo(movingTo, magspeedTo, rotatexTo, rotationmagspeedTo);
        }
    }

    private void Awake() {
        defaultCameraXRotation = transform.rotation.eulerAngles.x;
        // See Reference #1.1
        transform.eulerAngles = new Vector3(defaultCameraXRotation + (transform.position.y - rotationTransformOffset) * rotationAmount, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
        clampedTransformPositionY = transform.position.y;

        // Orientation Boundaries
        BoundaryMinZ_Orientation = BoundaryMinZ;
        BoundaryMaxZ_Orientation = BoundaryMaxZ;
    }

    /// <summary>
    /// Calculate new Camera Position when there is a Touch input
    /// </summary>
    void Update() {

        // Check if UI was touched
        // Neccessary so that the Camera won´t move if Panels are touched
        pointer = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
        raycastResults.Clear();
        EventSystem.current.RaycastAll(pointer, raycastResults);
        if (raycastResults.Count > 0) {
            return;
        }
  


        // Handle Touch
        switch (Input.touchCount) {
            case 1: // Panning
                PanCamera();
                break;
            case 2: // Zooming
                ZoomCamera();
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Enforce Boundaries
    /// </summary>
    void LateUpdate() {
        // If we are zoomed into a building for more information the boundarys do not apply!
        if (zoomedToBuilding || moving) {
            return;
        }

        directionVector = transform.position;

        // Clamp our Position to our Boundaries so we cant go over them
        directionVector.x = Mathf.Clamp(directionVector.x, BoundaryMinX + directionVector.y * 0.75f, BoundaryMaxX + directionVector.y * 0.75f);
        directionVector.y = Mathf.Clamp(directionVector.y, zoomMin, zoomMax + portraitBonusMaxZoom);
        directionVector.z = Mathf.Clamp(directionVector.z, BoundaryMinZ_Orientation + directionVector.y * 0.4f, BoundaryMaxZ_Orientation - directionVector.y * 0.4f);

        // Set the new Position
        transform.position = directionVector;
    }


    // -- Camera Move with 1 finger on the Screen
    void PanCamera() {
        if (!zoomedToBuilding && !moving) {

            touchZero = Input.GetTouch(0);

            if (touchZero.phase == TouchPhase.Ended) {
                rigidbody.mass = 1;
                touchStartedOnUI = true;

            } else if (touchZero.phase == TouchPhase.Began) {
                // get a raycast from the touch Position to the world
                ray = camComponent.ScreenPointToRay(touchZero.position);

                // Get the initial point where the raycast hits
                if (plane.Raycast(ray, out planeEnter)) {
                    initialPanHit = ray.GetPoint(planeEnter);
                }

                touchStartedOnUI = false;
                rigidbody.mass = 0;

                // Close overlayMenu
                Globals.UICanvas.uiElements.OverlayMenu.closeOverlayMenu();

            } else if (!touchStartedOnUI && touchZero.phase == TouchPhase.Moved) {
                // get a raycast from the touch Position to the world
                ray = camComponent.ScreenPointToRay(touchZero.position);

                // Get the new point where the raycast hits
                if (plane.Raycast(ray, out planeEnter)) {

                    //float difference = (initialPanHit.point - actualPanHit.point).magnitude;
                    directionVector = (initialPanHit - ray.GetPoint(planeEnter));
                    directionVector.y = 0;

                    //transform.Translate(directionVector, Space.World);
                    rigidbody.velocity = directionVector * moveSpeed;     
                }
            } 
        }
    }

    void ZoomCamera() {
        if(!zoomedToBuilding && !moving) {
            // -- Camera Zoom with 2 fingers

            // Close overlayMenu
            Globals.UICanvas.uiElements.OverlayMenu.closeOverlayMenu();

            // Store both touches.
            touchZero = Input.GetTouch(0);
            touchOne = Input.GetTouch(1);

            // Find the position in the previous frame of each touch.
            touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            // Find the magnitude of the vector (the distance) between the touches in each frame.
            prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            // Find the difference in the distances between each frame.
            deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

            // If we didnt reach the Boundary yet we translate
            if ((transform.position.y < (zoomMax + portraitBonusMaxZoom) && deltaMagnitudeDiff > 0) || (transform.position.y > zoomMin && deltaMagnitudeDiff < 0)) {
                // Transform in Camera Space (So we go towards the Direction we are looking)
                transform.Translate(
                    new Vector3(
                        0,
                        deltaMagnitudeDiff * zoomSpeed * rotationCorrectionY, // Correction of camera height when rotating
                        -(deltaMagnitudeDiff * zoomSpeed)), // Translation Vector -(Distance since last frame * Speed) no x and y movement
                    Space.Self);
            }

            // #1.1 X Rotation of the Camera is dependend by the height of it -> On to the ground the Angle is more flat
            // Take the Camera Height -> Offset and rotation is for Balancing the Height Value
            heightBasedRotationAdditionX = (transform.position.y - rotationTransformOffset) * rotationAmount;

            // Rotate the Camera - add the height-based rotation to the default Camera-Angle
            transform.eulerAngles = new Vector3(defaultCameraXRotation + heightBasedRotationAdditionX, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);

            if (transform.position.y > minHeightSpeed) {
                clampedTransformPositionY = transform.position.y;
            } else {
                clampedTransformPositionY = minHeightSpeed;
            }
        }    
    }

    /// <summary>
    /// Adjust Envi Needle after Orientation switch
    /// </summary>
    /// <returns></returns>
    public IEnumerator adjustEnviNeedle() {

        yield return new WaitForSeconds(0.3f);

        Globals.Game.currentWorld.enviGlass.transformNeedle();

        yield return null;
    }


    /// <summary>
    /// Animation Core wich is called each frame while the animation is running
    /// </summary>
    /// <param name="vec3">Target Position</param>
    /// <param name="magspeed">Movement magnitude</param>
    /// <param name="rotationX">Target X Rotation</param>
    /// <param name="rotationmagspeed">Rotation Angle difference</param>
    private void moveTo(Vector3 vec3, float magspeed, float rotationX, float rotationmagspeed) {

        // The Movetowards does exaclty what we want
        // We call it with the current position, the target position, and the maximum magnitude it moves per call
        transform.position = Vector3.MoveTowards(transform.position, vec3, animationSpeed * Time.deltaTime * magspeed);

        // If our rotation is below 0 we raise it up by 360 this makes the call a little bit easier
        // instead of calling move to 350 we can now say -10
        if(rotationX < 0) {
            rotationX += 360;
        }

        // Check to see wich way is faster (+x or -x)
        // if this is true it is faster to go to -x 
        // from 30 to 10 this basically reads if(340>20)
        // (360+10-30 = 340 and 360-10+30 = 380%360 = 20) 
        if ((360 + rotationX - transform.rotation.eulerAngles.x) % 360 > (360 - rotationX + transform.rotation.eulerAngles.x) % 360) {

            // Animate to -x
            transform.Rotate(new Vector3(-animationSpeed * Time.deltaTime * rotationmagspeed, 0, 0));
            
            // Until it is faster to go to +x (we passed our destination)
            if ((360 + rotationX - transform.rotation.eulerAngles.x) % 360 <= (360 - rotationX + transform.rotation.eulerAngles.x) % 360) {
                // Then we set it exactly to the destination and make the animation stop later
                transform.eulerAngles = new Vector3(rotationX, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
            }

        } else {
            // The same but the other way around
            transform.Rotate(new Vector3(animationSpeed * Time.deltaTime * rotationmagspeed, 0, 0));

            if ((360 + rotationX - transform.rotation.eulerAngles.x) % 360 > (360 - rotationX + transform.rotation.eulerAngles.x) % 360) {
                transform.eulerAngles = new Vector3(rotationX, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
            }
        }
        
        // Small threshhold to set the current pos to target pos if within a small magnitude
        if ((transform.position - vec3).magnitude < 0.1f) {
            transform.position = vec3;
        }

        // Animation done when the position is the target position and the rotation is the target rotation
        if (transform.position == vec3 && System.Math.Round(transform.eulerAngles.x, 2) == System.Math.Round(rotationX, 2)) {

            // Set moving to false and dont call this function again
            moving = false;
        } else {
            // Still animating so call this on the next GUI update with these variables
            moving = true;
            movingTo = vec3;
            rotatexTo = rotationX;
            magspeedTo = magspeed;
            rotationmagspeedTo = rotationmagspeed;
        }
    }

    /// <summary>
    /// Moves the camera to the building
    /// </summary>
    /// <param name="building">The building to move to</param>
    public void moveToBuilding(Building hittedBuilding, bool saveORpos = false) {

        zoomedToBuilding = false;

        // Save Original Camera spot
        if (saveORpos) {
            moveToBuildingOriginalCameraPosition = transform.position;
        }

        // Calculate Vector
        moveToBuildingVec3 = hittedBuilding.transform.position + hittedBuilding.buildingMenuOffsetLandscape + moveToBuildingVec3Translation;

        // The rotate X we want to set
        rotateX = -10.0f;

        // Get the Angle difference between the current and target angle (needed to calculate the animation to be as long as the move animation)
        rotateto = Quaternion.Euler(-10, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
        angle = Quaternion.Angle(transform.rotation, rotateto);

        // Get the magnitude between the current positon and target position (again needed to make the animations be synchron)
        magnitude = (transform.position - moveToBuildingVec3).magnitude;

        moveTo(moveToBuildingVec3, magnitude, rotateX, angle);

        buildingMovedTo = hittedBuilding;

        // Block the Camera movement as long as we are in zoom
        zoomedToBuilding = true;
    }

    /// <summary>
    /// Move the Camera back from the building information panel
    /// </summary>
    public void moveOutOfBuilding() {
        // Clear the Camera move blockage
        zoomedToBuilding = false;

        // Set the positon where we want to move 
        directionVector = moveToBuildingOriginalCameraPosition;

        // The rotate X we want to set
        rotateX = defaultCameraXRotation;

        // Get the Angle difference between the current and target angle (needed to calculate the animation to be as long as the move animation)
        Quaternion rotateTo = Quaternion.Euler(32, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
        angle = Quaternion.Angle(transform.rotation, rotateTo);

        // Get the magnitude between the current positon and target position (again needed to make the animations be synchron)
        magnitude = (transform.position - directionVector).magnitude;

        moveTo(directionVector, magnitude, rotateX, angle);
    }
 
}
