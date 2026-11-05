using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class ARObjectPlace : MonoBehaviour
{
    [SerializeField] private ARRaycastManager raycastManager;

    [Tooltip("Optional. Used so tapping an already-placed object selects it instead of placing a new one.")]
    [SerializeField] private ARObjectInteraction interaction;

    [Tooltip("ON: keep placing a new object on every tap until StopPlacing() is called. OFF: place one, then wait for the next building button.")]
    [SerializeField] private bool keepPlacing = true;

    [Tooltip("If no scanned surface is under the tap, fall back to feature points (can place in mid-air).")]
    [SerializeField] private bool allowFeaturePointFallback = false;

    [Tooltip("Max finger/mouse movement (pixels) for a press to count as a tap.")]
    [SerializeField] private float tapMaxMovement = 20f;

    // true = NOT placing. Building buttons set it to false.
    public bool IsTap = true;

    [SerializeField] Ease ease;

    public GameObject BuildingAlpha, BuildingBeta;
    GameObject Gobj;

    private bool tracking;
    private bool hadMultiTouch;
    private Vector2 startPos;

    void Awake()
    {
        if (interaction == null)
            interaction = FindFirstObjectByType<ARObjectInteraction>();
    }

    void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    void Update()
    {
        var touches = Touch.activeTouches;

        // ---------------- TOUCH ----------------
        if (touches.Count >= 2)
        {
            // pinch/twist: never a placement tap
            hadMultiTouch = true;
            tracking = false;
            return;
        }

        if (touches.Count == 1)
        {
            if (hadMultiTouch)
                return;   // leftover finger after a pinch

            Touch t = touches[0];

            switch (t.phase)
            {
                case UnityEngine.InputSystem.TouchPhase.Began:
                    BeginTap(t.screenPosition, t.touchId);
                    break;

                case UnityEngine.InputSystem.TouchPhase.Ended:
                    EndTap(t.screenPosition);
                    break;

                case UnityEngine.InputSystem.TouchPhase.Canceled:
                    tracking = false;
                    break;
            }

            return;
        }

        hadMultiTouch = false;

        // ---------------- MOUSE ----------------
        Mouse mouse = Mouse.current;

        if (mouse == null)
            return;

        if (mouse.leftButton.wasPressedThisFrame)
            BeginTap(mouse.position.ReadValue(), -1);

        if (mouse.leftButton.wasReleasedThisFrame)
            EndTap(mouse.position.ReadValue());
    }

    void BeginTap(Vector2 pos, int pointerId)
    {
        tracking = false;

        if (IsTap)
            return;

        if (EventSystem.current != null)
        {
            bool overUI = pointerId < 0
                ? EventSystem.current.IsPointerOverGameObject()
                : EventSystem.current.IsPointerOverGameObject(pointerId);

            if (overUI)
                return;
        }

        tracking = true;
        startPos = pos;
    }

    void EndTap(Vector2 pos)
    {
        bool wasTracking = tracking;
        tracking = false;

        if (!wasTracking || IsTap)
            return;

        if (Vector2.Distance(pos, startPos) > tapMaxMovement)
            return;

        // Tapped an existing object -> let ARObjectInteraction select it
        if (interaction != null && interaction.IsObjectAt(pos))
            return;

        if (PlaceObj(pos) && !keepPlacing)
            IsTap = true;
    }

    bool PlaceObj(Vector2 Pos)
    {
        var RayHit = new List<ARRaycastHit>();

        // Scanned surfaces only. AllTypes also matches loose feature points,
        // which float in mid-air and are usually the closest hit.
        raycastManager.Raycast(Pos, RayHit, TrackableType.PlaneWithinPolygon);

        if (RayHit.Count == 0 && allowFeaturePointFallback)
            raycastManager.Raycast(Pos, RayHit, TrackableType.FeaturePoint);

        if (RayHit.Count == 0)
            return false;

        Vector3 Position = RayHit[0].pose.position;
        Quaternion Rot = RayHit[0].pose.rotation;

        Gobj = Instantiate(raycastManager.raycastPrefab, Position, Rot);
        Gobj.transform.localScale = Vector3.zero;

        Gobj.transform.DOScale(1f, .5f).SetEase(ease);

        return true;
    }


    public void BuildingOne()
    {
        IsTap = false;
        raycastManager.raycastPrefab = BuildingAlpha;
    }

    public void BuildingTwo()
    {
        IsTap = false;
        raycastManager.raycastPrefab = BuildingBeta;
    }

    // Hook this to a "Done" button to stop placing.
    public void StopPlacing()
    {
        IsTap = true;
        tracking = false;
    }
}