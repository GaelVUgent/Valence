//========= Copyright 2018, HTC Corporation. All rights reserved. ===========
using MICT.eDNA.Controllers;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Serialization;
using ViveSR.anipal.Eye;

public class EyeDataCapturer : MonoBehaviour
{
    public int LengthOfRay = 4;
    public LayerMask AciveLayersForGaze;
    //[SerializeField] private LineRenderer GazeRayRenderer;
    //[SerializeField] private Transform _GazeObjectTransform;
    public EyeData_v2 eyeData = new EyeData_v2();
    [FormerlySerializedAs("vrCamera")]
    public Camera OverrideMainCamera;
    private bool eye_callback_registered = false;
    private Vector3 _lastGazeDirection = Vector3.forward;
    private RaycastHit _raycastHitWithGaze;
    private bool _focusPointEnabled = false;
    private static EyeDataCapturer _instance;
    private System.IntPtr _functionPointer;
    private bool _eyeStatusIsNotWorkingAndHasBeenCommunicated = false;
    private void Awake()
    {
        if (_instance == null)
            _instance = this;


    }
    IEnumerator Start()
    {
        if (!SRanipal_Eye_Framework.Instance.EnableEye)
        {
            enabled = false;
            yield break;
        }

        while (SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.WORKING)
        {
            if(!_eyeStatusIsNotWorkingAndHasBeenCommunicated)
                Debug.LogWarning($"Eye status: {SRanipal_Eye_Framework.Status}");
            _eyeStatusIsNotWorkingAndHasBeenCommunicated = true;
            yield return null;
        }

        if (SRanipal_Eye_Framework.Instance.EnableEyeDataCallback == true)
        {
            print("registering eye data callback");
            _functionPointer = Marshal.GetFunctionPointerForDelegate<SRanipal_Eye_v2.CallbackBasic>(EyeCallback);
            SRanipal_Eye_v2.WrapperRegisterEyeDataCallback(_functionPointer);
            eye_callback_registered = true;
        }
        //Assert.IsNotNull(GazeRayRenderer);
    }

    private void OnDestroy()
    {
        Release();
    }

    private void Update()
    {
        if (SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.WORKING &&
            SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.NOT_SUPPORT)
            return;

        /* if (SRanipal_Eye_Framework.Instance.EnableEyeDataCallback == true && eye_callback_registered == false)
         {
             SRanipal_Eye.WrapperRegisterEyeDataCallback(Marshal.GetFunctionPointerForDelegate<SRanipal_Eye.CallbackBasic>(EyeCallback));
             eye_callback_registered = true;
         }
         else if (SRanipal_Eye_Framework.Instance.EnableEyeDataCallback == false && eye_callback_registered == true)
         {
             SRanipal_Eye.WrapperUnRegisterEyeDataCallback(Marshal.GetFunctionPointerForDelegate<SRanipal_Eye.CallbackBasic>(EyeCallback));
             eye_callback_registered = false;
         }*/

        Vector3 GazeOriginCombinedLocal, GazeDirectionCombinedLocal;

        if (eye_callback_registered)
        {
            if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.COMBINE, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal, eyeData)) { }
            else if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.LEFT, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal, eyeData)) { }
            else if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.RIGHT, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal, eyeData)) { }
            else return;
        }
        else
        {
            if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.COMBINE, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal)) { }
            else if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.LEFT, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal)) { }
            else if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.RIGHT, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal)) { }
            else return;
        }

        //Debug.Log(GazeDirectionCombinedLocal);
        if (OverrideMainCamera == null)
            OverrideMainCamera = Camera.main;
        _lastGazeDirection = OverrideMainCamera.transform.TransformDirection(GazeDirectionCombinedLocal);
    }

    private void FixedUpdate()
    {
        //VRTK doesn't enable the camera in the first frame
        if (OverrideMainCamera == null)
        {
            if (Camera.main != null)
            { 
                OverrideMainCamera = Camera.main; 
            }
            else
            {
                return;
            }
        }

        if (Physics.Raycast(OverrideMainCamera.transform.position, _lastGazeDirection, out _raycastHitWithGaze, LengthOfRay, AciveLayersForGaze))
        {
            if (_focusPointEnabled)
            {
                //GazeRayRenderer.SetPosition(0, OverrideMainCamera.transform.position - OverrideMainCamera.transform.up * 0.05f);
                //GazeRayRenderer.SetPosition(1, OverrideMainCamera.transform.position + _lastGazeDirection * LengthOfRay);
                //_GazeObjectTransform.position = _hit.point;
            }
            OutputController.INSTANCE?.CheckLookAtCollider(_raycastHitWithGaze.collider);
            //Debug.Log(_raycastHitWithGaze.collider);
        }
        else
        {
            OutputController.INSTANCE?.CheckLookAtCollider(null);
        }
    }

    private void Release()
    {
        if (eye_callback_registered)
        {
            print("releasing " + _functionPointer);
            SRanipal_Eye_API.UnregisterEyeDataCallback(_functionPointer);
            eye_callback_registered = false;
        }
    }
    private static void EyeCallback(ref EyeData_v2 eye_data)
    {
        if (_instance != null)
            _instance.eyeData = eye_data;
    }

    public void ToggleFocusPoint()
    {
        _focusPointEnabled = !_focusPointEnabled;
        //_GazeObjectTransform.gameObject.SetActive(_focusPointEnabled);
    }
}
