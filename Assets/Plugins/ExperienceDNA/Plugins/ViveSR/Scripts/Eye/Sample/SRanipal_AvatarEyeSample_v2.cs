﻿//========= Copyright 2018, HTC Corporation. All rights reserved. ===========
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Assertions;

namespace ViveSR
{
    namespace anipal
    {
        namespace Eye
        {
            public class SRanipal_AvatarEyeSample_v2 : MonoBehaviour
            {
                [SerializeField] private Transform[] EyesModels = new Transform[0];
                [SerializeField] private Transform[] EyesOrigins = new Transform[0];
                [SerializeField] private List<EyeShapeTable> EyeShapeTables;
                /// <summary>
                /// Customize this curve to fit the blend shapes of your avatar.
                /// </summary>
                [SerializeField] private AnimationCurve EyebrowAnimationCurveUpper;
                /// <summary>
                /// Customize this curve to fit the blend shapes of your avatar.
                /// </summary>
                [SerializeField] private AnimationCurve EyebrowAnimationCurveLower;
                /// <summary>
                /// Customize this curve to fit the blend shapes of your avatar.
                /// </summary>
                [SerializeField] private AnimationCurve EyebrowAnimationCurveHorizontal;

                public bool NeededToGetData = true;
                private Dictionary<EyeShape, float> EyeWeightings = new Dictionary<EyeShape, float>();
                private AnimationCurve[] EyebrowAnimationCurves = new AnimationCurve[(int)EyeShape.Max];
                private GameObject[] EyeAnchors;
                private const int NUM_OF_EYES = 2;
                private static EyeData_v2 eyeData = new EyeData_v2();
                private bool eye_callback_registered = false;

                private void Start()
                {
                    if (!SRanipal_Eye_Framework.Instance.EnableEye)
                    {
                        enabled = false;
                        return;
                    }

                    if (EyesModels?.Length >= 2)
                    {
                        SetEyesModels(EyesModels[0], EyesModels[1]);
                    }
                    SetEyeShapeTables(EyeShapeTables);

                    AnimationCurve[] curves = new AnimationCurve[(int)EyeShape.Max];
                    for (int i = 0; i < EyebrowAnimationCurves.Length; ++i)
                    {
                        if (i == (int)EyeShape.Eye_Left_Up || i == (int)EyeShape.Eye_Right_Up) curves[i] = EyebrowAnimationCurveUpper;
                        else if (i == (int)EyeShape.Eye_Left_Down || i == (int)EyeShape.Eye_Right_Down) curves[i] = EyebrowAnimationCurveLower;
                        else curves[i] = EyebrowAnimationCurveHorizontal;
                    }
                    SetEyeShapeAnimationCurves(curves);
                }

                public GameObject Box1, Box2;
                private void Update()
                {
                    if (SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.WORKING &&
                        SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.NOT_SUPPORT) return;

                    if (NeededToGetData)
                    {
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
                        else if (SRanipal_Eye_Framework.Instance.EnableEyeDataCallback == false)
                            SRanipal_Eye_API.GetEyeData_v2(ref eyeData);

                        //Debug.Log("[Eye v2] Openness = " + eyeData.verbose_data.left.eye_openness + ", " + eyeData.verbose_data.right.eye_openness);

                        bool isLeftEyeActive = false;
                        bool isRightEyeAcitve = false;
                        if (SRanipal_Eye_Framework.Status == SRanipal_Eye_Framework.FrameworkStatus.WORKING)
                        {
                            isLeftEyeActive = eyeData.verbose_data.left.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_GAZE_ORIGIN_VALIDITY);
                            isRightEyeAcitve = eyeData.verbose_data.right.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_GAZE_ORIGIN_VALIDITY);
                        }
                        else if (SRanipal_Eye_Framework.Status == SRanipal_Eye_Framework.FrameworkStatus.NOT_SUPPORT)
                        {
                            isLeftEyeActive = true;
                            isRightEyeAcitve = true;
                        }

                        if (isLeftEyeActive || isRightEyeAcitve)
                        {
                            if (eye_callback_registered == true)
                                SRanipal_Eye_v2.GetEyeWeightings(out EyeWeightings, eyeData);
                            else
                                SRanipal_Eye_v2.GetEyeWeightings(out EyeWeightings);
                            UpdateEyeShapes(EyeWeightings);
                        }
                        else
                        {
                            for (int i = 0; i < (int)EyeShape.Max; ++i)
                            {
                                bool isBlink = ((EyeShape)i == EyeShape.Eye_Left_Blink || (EyeShape)i == EyeShape.Eye_Right_Blink);
                                EyeWeightings[(EyeShape)i] = isBlink ? 1 : 0;
                            }

                            UpdateEyeShapes(EyeWeightings);

                            return;
                        }

                        Vector3 GazeOriginCombinedLocal, GazeDirectionCombinedLocal = Vector3.zero;
                        if (eye_callback_registered == true)
                        {
                            if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.COMBINE, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal, eyeData)) { }
                            else if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.LEFT, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal, eyeData)) { }
                            else if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.RIGHT, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal, eyeData)) { }
                        }
                        else
                        {
                            if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.COMBINE, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal)) { }
                            else if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.LEFT, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal)) { }
                            else if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.RIGHT, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal)) { }

                        }
                        //print(GazeDirectionCombinedLocal);
                        UpdateGazeRay(GazeDirectionCombinedLocal);
                    }
                }
                private void Release()
                {
                    if (eye_callback_registered == true)
                    {
                        SRanipal_Eye_v2.WrapperUnRegisterEyeDataCallback(Marshal.GetFunctionPointerForDelegate((SRanipal_Eye_v2.CallbackBasic)EyeCallback));
                        eye_callback_registered = false;
                    }
                }

                private void OnDisable()
                {
                    if (EyesModels != null)
                    {
                        //resetting eyeballs
                        foreach (var item in EyesModels)
                        {
                            item.localEulerAngles = new Vector3(0, 0, -180f);
                        }
                    }
                    for (int i = 0; i < (int)EyeShape.Max; ++i)
                    {
                        EyeWeightings[(EyeShape)i] = 0;
                    }
                    UpdateEyeShapes(EyeWeightings);
                    ResetBlendshapes();
                }

                private void OnDestroy()
                {
                    DestroyEyeAnchors();
                }

                public void SetEyesModels(Transform leftEye, Transform rightEye)
                {
                    if (leftEye != null && rightEye != null)
                    {
                        EyesModels = new Transform[NUM_OF_EYES] { leftEye, rightEye };
                        DestroyEyeAnchors();
                        CreateEyeAnchors();
                    }
                }

                public void SetEyeShapeTables(List<EyeShapeTable> eyeShapeTables)
                {
                    bool valid = true;
                    if (eyeShapeTables == null)
                    {
                        valid = false;
                    }
                    else
                    {
                        for (int table = 0; table < eyeShapeTables.Count; ++table)
                        {
                            if (eyeShapeTables[table].skinnedMeshRenderer == null)
                            {
                                valid = false;
                                break;
                            }
                            for (int shape = 0; shape < eyeShapeTables[table].eyeShapes.Length; ++shape)
                            {
                                EyeShape eyeShape = eyeShapeTables[table].eyeShapes[shape];
                                if (eyeShape > EyeShape.Max || eyeShape < 0)
                                {
                                    valid = false;
                                    break;
                                }
                            }
                        }
                    }
                    if (valid)
                        EyeShapeTables = eyeShapeTables;
                }

                public void SetEyeShapeAnimationCurves(AnimationCurve[] eyebrowAnimationCurves)
                {
                    if (eyebrowAnimationCurves.Length == (int)EyeShape.Max)
                        EyebrowAnimationCurves = eyebrowAnimationCurves;
                }

                public void UpdateGazeRay(Vector3 gazeDirectionCombinedLocal)
                {
                    if (EyesModels?.Length >= 2)
                    {
                        for (int i = 0; i < EyesModels.Length; ++i)
                        {

                            Vector3 target = EyesOrigins[i].transform.TransformPoint(gazeDirectionCombinedLocal);
                            EyesModels[i].LookAt(target);

                            continue;
                            if (i == 0)
                                Box1.transform.position = target;
                            else
                                Box2.transform.position = target;
                        }
                    }
                }

                public void UpdateEyeShapes(Dictionary<EyeShape, float> eyeWeightings)
                {
                    foreach (var table in EyeShapeTables)
                        RenderModelEyeShape(table, eyeWeightings);
                }

                private void RenderModelEyeShape(EyeShapeTable eyeShapeTable, Dictionary<EyeShape, float> weighting)
                {
                    for (int i = 0; i < eyeShapeTable.eyeShapes.Length; ++i)
                    {
                        EyeShape eyeShape = eyeShapeTable.eyeShapes[i];
                        if (eyeShape > EyeShape.Max || eyeShape < 0) continue;

                        if (eyeShape == EyeShape.Eye_Left_Blink || eyeShape == EyeShape.Eye_Right_Blink)
                            eyeShapeTable.skinnedMeshRenderer.SetBlendShapeWeight(i, weighting[eyeShape] * 100f);
                        else
                        {
                            AnimationCurve curve = EyebrowAnimationCurves[(int)eyeShape];
                            eyeShapeTable.skinnedMeshRenderer.SetBlendShapeWeight(i, curve.Evaluate(weighting[eyeShape]) * 100f);
                        }
                    }
                }

                private void ResetBlendshapes()
                {
                    foreach (var table in EyeShapeTables)
                    {
                        for (int i = 0; i < table.eyeShapes.Length; ++i)
                        {
                            EyeShape eyeShape = table.eyeShapes[i];
                            if (eyeShape > EyeShape.Max || eyeShape < 0) continue;
                            table.skinnedMeshRenderer.SetBlendShapeWeight(i, 0);
                        }
                    }
                }

                private void CreateEyeAnchors()
                {
                    EyeAnchors = new GameObject[NUM_OF_EYES];

                    for (int i = 0; i < NUM_OF_EYES; ++i)
                    {
                        EyeAnchors[i] = new GameObject();
                        EyeAnchors[i].name = "EyeAnchor_" + i;
                        EyeAnchors[i].transform.SetParent(gameObject.transform);
                        if (EyesModels?.Length > i)
                        {
                            EyeAnchors[i].transform.localPosition = EyesModels[i].localPosition;
                            EyeAnchors[i].transform.localRotation = EyesModels[i].localRotation;
                            EyeAnchors[i].transform.localScale = EyesModels[i].localScale;
                        }
                    }
                }

                private void DestroyEyeAnchors()
                {
                    if (EyeAnchors != null)
                    {
                        foreach (var obj in EyeAnchors)
                            if (obj != null) Destroy(obj);
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