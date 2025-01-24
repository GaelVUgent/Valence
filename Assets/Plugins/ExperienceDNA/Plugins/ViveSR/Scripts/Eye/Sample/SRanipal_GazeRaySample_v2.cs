//========= Copyright 2018, HTC Corporation. All rights reserved. ===========
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Assertions;

namespace ViveSR
{
    namespace anipal
    {
        namespace Eye
        {
            public class SRanipal_GazeRaySample_v2 : MonoBehaviour
            {
                public int LengthOfRay = 25;
                public GameObject boxUserToObject;
                public GameObject boxUserToPlatform;
                public GameObject boxUserToContext;
                public GameObject textUserToObject;
                public GameObject textUserToPlatform;
                public GameObject textUserToContext;
                public LayerMask AciveLayersForGaze;
                Color colorboxUserToObject;
                Color colorboxUserToPlatform;
                Color colorboxUserToContext;
                [SerializeField] private LineRenderer GazeRayRenderer;
                private static EyeData_v2 eyeData = new EyeData_v2();
                private bool eye_callback_registered = false;
                private Ray _gazeRay;
                private RaycastHit _raycastHitWithGaze;
                private void Start()
                {
                    if (!SRanipal_Eye_Framework.Instance.EnableEye)
                    {
                        enabled = false;
                        return;
                    }
                    Assert.IsNotNull(GazeRayRenderer);
                }

                private void Update()
                {
                    if (SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.WORKING &&
                        SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.NOT_SUPPORT) return;

                    if (SRanipal_Eye_Framework.Instance.EnableEyeDataCallback == true && eye_callback_registered == false)
                    {
                        SRanipal_Eye_v2.WrapperRegisterEyeDataCallback(Marshal.GetFunctionPointerForDelegate((SRanipal_Eye_v2.CallbackBasic)EyeCallback));
                        eye_callback_registered = true;
                    }
                    else if (SRanipal_Eye_Framework.Instance.EnableEyeDataCallback == false && eye_callback_registered == true)
                    {
                        SRanipal_Eye_v2.WrapperUnRegisterEyeDataCallback(Marshal.GetFunctionPointerForDelegate((SRanipal_Eye_v2.CallbackBasic)EyeCallback));
                        eye_callback_registered = false;
                    }

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

                    Vector3 GazeDirectionCombined = Camera.main.transform.TransformDirection(GazeDirectionCombinedLocal);
                    GazeRayRenderer.SetPosition(0, Camera.main.transform.position - Camera.main.transform.up * 0.05f);
                    GazeRayRenderer.SetPosition(1, Camera.main.transform.position + GazeDirectionCombined * LengthOfRay);
                    //--------------------------------------------------------Dit stuk code is nieuw
                    //_gazeRay = new Ray(GazeRayRenderer.GetPosition(0), GazeRayRenderer.GetPosition(1));
                    Physics.Raycast(GazeRayRenderer.GetPosition(0), GazeRayRenderer.GetPosition(1), out _raycastHitWithGaze, LengthOfRay, AciveLayersForGaze);
                    textUserToObject.SetActive(false);//set text canvas User To Object to false
                    textUserToPlatform.SetActive(false);//set text canvas User To Platform to false
                    textUserToContext.SetActive(false);//set text canvas User To Context to false
                    if (_raycastHitWithGaze.collider != null)
                    {
                        Debug.Log("You hit something!!");
                        /*if (_raycastHitWithGaze.collider.GetComponent<HCCIInteractionObject>() != null) {
                            var interObject = _raycastHitWithGaze.collider.GetComponent<HCCIInteractionObject>();

                        }*/

                        if (_raycastHitWithGaze.collider.CompareTag("UserToObject"))
                        {

                            colorboxUserToObject[1] = colorboxUserToObject[1] + 0.01f;
                            textUserToObject.SetActive(true);//set text canvas to true
                        }
                        if (_raycastHitWithGaze.collider.CompareTag("UserToPlatform"))
                        {

                            colorboxUserToPlatform[1] = colorboxUserToPlatform[1] + 0.01f;
                            textUserToPlatform.SetActive(true);//set text canvas to true
                        }
                        if (_raycastHitWithGaze.collider.CompareTag("UserToContext"))
                        {
                            colorboxUserToContext[1] = colorboxUserToContext[1] + 0.01f;
                            textUserToContext.SetActive(true);//set text canvas to true
                        }
                    }
                    if (Input.GetKeyDown(KeyCode.Space)) // deze if statement dient voor het tonen van hoeveel er gekeken is
                    {
                        boxUserToObject.GetComponent<Renderer>().material.color = colorboxUserToObject;
                        boxUserToPlatform.GetComponent<Renderer>().material.color = colorboxUserToPlatform;
                        boxUserToContext.GetComponent<Renderer>().material.color = colorboxUserToContext;
                    }
                    if (Input.GetKeyUp(KeyCode.Space))
                    {
                        boxUserToObject.GetComponent<Renderer>().material.color = Color.white;
                        boxUserToPlatform.GetComponent<Renderer>().material.color = Color.white;
                        boxUserToContext.GetComponent<Renderer>().material.color = Color.white;
                    }

                    //---------------------------------------------------------
                }

                private void OnDestroy()
                {
                    Release();
                }

                private void Release()
                {
                    if (eye_callback_registered == true)
                    {
                        SRanipal_Eye_v2.WrapperUnRegisterEyeDataCallback(Marshal.GetFunctionPointerForDelegate((SRanipal_Eye_v2.CallbackBasic)EyeCallback));
                        eye_callback_registered = false;
                    }
                }
                private static void EyeCallback(ref EyeData_v2 eye_data)
                {
                    eyeData = eye_data;
                }
            }
        }
    }
}
