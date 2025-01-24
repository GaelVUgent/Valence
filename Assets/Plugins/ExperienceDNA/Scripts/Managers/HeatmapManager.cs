using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeatmapManager : MonoBehaviour
{
    [HideInInspector] public bool _IsOn = false;

    [SerializeField] private Camera _mainCamera;
    [SerializeField] private Shader _heatmap_Merge;
    [SerializeField] private Shader _heatmap_Color;
    [SerializeField] private RenderTexture _effectRT;

    private Camera _effectCam;
    //private RenderTexture _effectRT;
    private Material _heatmapMergeMat;
    private Dictionary<Material, Shader> _materialShaderDic = new Dictionary<Material, Shader>();

    private void Start()
    {
        _effectCam = new GameObject().AddComponent<Camera>();
        _effectCam.enabled = false;
        _effectCam.name = "Effect Camera";
        _effectCam.targetTexture = _effectRT;

        //Disable HDR when using Single Pass
        _mainCamera.allowHDR = false;

        //Create Materials
        _heatmapMergeMat = new Material(_heatmap_Merge);
    }

    public void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (!_IsOn)
        {
            Graphics.Blit(source, destination);
            return;
        }
        //set up a temporary camera
        _effectCam.CopyFrom(_mainCamera);
        //_effectCam.clearFlags = CameraClearFlags.Color;
        //_effectCam.backgroundColor = new Color(0, 0, 0, 1);

        //_effectRT = new RenderTexture(source.width, source.height, 0, RenderTextureFormat.ARGB32);
        //_effectRT.Create();
        _effectCam.targetTexture = _effectRT;

        //Set Objects To effect layer
        //Dictionary<GameObject, int> layerPerObject = new Dictionary<GameObject, int>();
        //for (int i = 0; i < _objectsToRender.Count; i++)
        //{
        //    layerPerObject.Add(_objectsToRender[i], _objectsToRender[i].layer);
        //    SetLayerRecursively(_objectsToRender[i], 31);
        //}
        //cull any layer that isn't the effect layer
        //_effectCam.cullingMask = 1;

        //Set materials to heatmap shader
        _materialShaderDic.Clear();
        for (int i = 0; i < HCCIInteractionObject.MATERIALS.Count; i++)
        {
            _materialShaderDic[HCCIInteractionObject.MATERIALS[i]] = HCCIInteractionObject.MATERIALS[i].shader;
            HCCIInteractionObject.MATERIALS[i].shader = _heatmap_Color;
        }
        //render scene
        _effectCam.Render();
        //restore material shaders
        for (int i = 0; i < HCCIInteractionObject.MATERIALS.Count; i++)
        {
            HCCIInteractionObject.MATERIALS[i].shader = _materialShaderDic[HCCIInteractionObject.MATERIALS[i]];
        }

        _heatmapMergeMat.SetTexture("_SceneTex", source);
        Graphics.Blit(_effectRT, destination, _heatmapMergeMat);
        ////Put all Gameobjects back to their respective layers
        //for (int i = 0; i < _objectsToRender.Count; i++)
        //{
        //    SetLayerRecursively(_objectsToRender[i], layerPerObject[_objectsToRender[i]]);
        //}

        //_effectRT.Release();
    }

    private void SetLayerRecursively(GameObject go, int layer)
    {
        go.layer = layer;
        for (int i = 0; i < go.transform.childCount; i++)
        {
            SetLayerRecursively(go.transform.GetChild(i).gameObject, layer);
        }
    }
}
