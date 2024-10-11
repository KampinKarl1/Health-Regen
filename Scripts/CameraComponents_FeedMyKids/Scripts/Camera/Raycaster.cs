using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomCameraComponents{

public class Raycaster : MonoBehaviour
{
    private static Raycaster instance = null;
    public static Raycaster Instance => instance;

    [SerializeField] Camera mainCam = null;
    enum RaycastTo { CamFWD, MousePos };
    [Header("Raycast Specs")]
    [SerializeField] private RaycastTo raycastTo = RaycastTo.CamFWD;
    [SerializeField] float rayDistance = 100.0f;
    [SerializeField] float sphereRad = 1.0f;
    [SerializeField] bool sphereCast = false;
    bool gotHitLastFrame = false;

    [SerializeField] QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore;
    [SerializeField] LayerMask castMask = 0;

    [Header("Performance")]
    [Tooltip("Do update every other frame?")]
    public bool isUpdateEveryOtherFrame = true;
    bool everyOtherFrame = false;

    Vector3 lastHitPos = Vector3.zero;
    internal Vector3 GetLastHitPos() => hit.collider != null ? hit.point: lastHitPos;

    internal bool TryGetLastHit(out RaycastHit _hit) 
    {
        _hit = hit;
        return hit.collider != null;
    }

    //Keep as members to avoid GC
    Ray ray = new Ray();
    static readonly RaycastHit EMPTY_HIT = new RaycastHit();
    RaycastHit hit = EMPTY_HIT;
    GameObject lastHit = null;

    public delegate void OnHitSomething(in RaycastHit hit);
    public delegate void OnStopHitting();

    /// <summary>
    /// Called when when hitting a gameobject.
    /// </summary>
    public OnHitSomething onHitSomething;
    public OnStopHitting onEndHit;

    #region Builtin
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            DestroyImmediate(this);
    }

    void Start()
    {
        if (!mainCam)
        mainCam = Camera.main;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            CastRay();
            return;
        }

        everyOtherFrame = !everyOtherFrame;

        if (everyOtherFrame)        
            CastRay();        
    }
    #endregion
    void CastRay()
    {
        ray = raycastTo == RaycastTo.CamFWD ? new Ray(mainCam.transform.position, mainCam.transform.forward) : mainCam.ScreenPointToRay(Input.mousePosition);

        if (sphereCast ? Physics.SphereCast(ray, sphereRad, out hit, rayDistance, castMask, triggerInteraction) : Physics.Raycast(ray, out hit, rayDistance, castMask, triggerInteraction))
        {
            if (!gotHitLastFrame)
            {
                gotHitLastFrame = true;
                lastHit = hit.collider.gameObject;
                onHitSomething?.Invoke(in hit);
                return;
            }

            if (lastHit != hit.collider.gameObject)
            {
                lastHit = hit.collider.gameObject;
                onEndHit?.Invoke();
                onHitSomething?.Invoke(in hit);
            }
        }
        else if (gotHitLastFrame)
        {
            gotHitLastFrame = false;

            hit = EMPTY_HIT;

            onEndHit?.Invoke();
        }
    }

    /// <summary>
    /// Facade for Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, castDistance)
    /// </summary>
    /// <param name="hit"></param>
    /// <param name="castDistance"></param>
    /// <returns></returns>
    public static bool CastToMouse(out RaycastHit hit, float castDistance = 100f) 
    {
        return Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, castDistance);
    }
        public static bool CastFromCamera(out RaycastHit hit, float distance = 100f) 
        {
            return Physics.Raycast(new Ray(Camera.main.transform.position, Camera.main.transform.forward), out hit, distance);
        }
}
}
