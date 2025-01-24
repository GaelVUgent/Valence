using MICT.eDNA.Controllers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if VRTK_VERSION_3_1_0_OR_NEWER
using VRTK;
#endif

[System.Serializable]
public enum HCCIInteractionType
{
    UserToObject,
    UserToContext,
    UserToContent,
    UserToUser
}

public class HCCIInteractionObject : MonoBehaviour
{
    public UnityEvent OnLookAtStarted, OnLookAtEnded;
    public static List<Material> MATERIALS = new List<Material>();
    public HCCIInteractionGraph Graph { get; set; }
    public string Name { get { return _useGameObjectName ? _name + " : " + gameObject?.name : _name; } }
    public HCCIInteractionType Type { get { return _type; } }
    public List<Collider> Colliders { get { return _colliders; } }
    public List<Material> Materials { get { return _materials; } }
    public float Percentage
    {
        get { return _percentage; }
        set
        {
            _percentage = value;
            //for (int i = 0; i < _materials.Count; i++)
            //{
            //    _materials[i].SetFloat("_HeatMapPercentage", value);
            //}
        }
    }
    public float HeatMapTime
    {
        get { return _heatMapTime; }
        set
        {
            _heatMapTime = value;
            for (int i = 0; i < _materials.Count; i++)
            {
                _materials[i].SetFloat("_HeatMapTime", value);
            }
        }
    }
    [SerializeField] private bool _useGameObjectName = false;
    [SerializeField] private string _name;
    [SerializeField] private HCCIInteractionType _type;
    [SerializeField] private List<Collider> _colliders = new List<Collider>();
    [SerializeField] private bool _influenceMaterials = false;
    private GameObject _go;
    private List<Material> _materials = new List<Material>();
    private float _percentage = 0;
    private float _heatMapTime = 0;
#if VRTK_VERSION_3_1_0_OR_NEWER
    [SerializeField] private VRTK.VRTK_InteractableObject _interactable;
#endif

    protected virtual void Start()
    {
        _go = gameObject;
        if ((Colliders == null || Colliders.Count == 0) && GetComponent<Collider>() != null) {
            _colliders.Add(GetComponent<Collider>());
        }

        if (OutputController.INSTANCE == null)
            Debug.LogWarning($"No output controller available, HCCI object {name} will not be registered!");
        else
            Graph = OutputController.INSTANCE.AddHCCIGraph(this);

#if VRTK_VERSION_3_1_0_OR_NEWER
        if (_interactable == null) {
            _interactable = GetComponent<VRTK_InteractableObject>();
        }
        if (_interactable)
        {
            _interactable.InteractableObjectGrabbed += OnGrab;
            _interactable.InteractableObjectUngrabbed += OnDrop;
        }
#endif

        if (_influenceMaterials)
        {
            Renderer[] renderers = _go.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                for (int j = 0; j < renderers[i].sharedMaterials.Length; j++)
                {
                    if (!_materials.Contains(renderers[i].sharedMaterials[j]) && renderers[i].materials[j] != null)
                    {
                        _materials.Add(renderers[i].materials[j]);
                        renderers[i].materials[j].SetColor("_BottomColor", Color.red);
                        renderers[i].materials[j].SetColor("_MiddleColor", Color.green);
                        renderers[i].materials[j].SetColor("_TopColor", Color.blue);
                    }
                    if (!MATERIALS.Contains(renderers[i].sharedMaterials[j]) && renderers[i].materials[j] != null)
                        MATERIALS.Add(renderers[i].materials[j]);
                }
            }
        }
    }

    private void OnDestroy()
    {
#if VRTK_VERSION_3_1_0_OR_NEWER
        if (_interactable)
        {
            _interactable.InteractableObjectGrabbed -= OnGrab;
            _interactable.InteractableObjectUngrabbed -= OnDrop;
        }
#endif
    }

    public void LookAtStarted() {
        OnLookAtStarted?.Invoke();
    }

    public void LookAtEnded()
    {
        OnLookAtEnded?.Invoke();
    }

    //used with new VR Interaction framework
    protected void OnDroppedObject()
    {
        OutputController.INSTANCE.CheckGraspCollider(_colliders[0], false);
    }

    //used with new VR Interaction framework
    protected void OnGrabbedObject()
    {
        OutputController.INSTANCE.CheckGraspCollider(_colliders[0], true);
    }

    //leave this for projects with VRTK
    #if VRTK_VERSION_3_1_0_OR_NEWER
    protected void OnDrop(object sender, InteractableObjectEventArgs e)
    {
        OutputController.INSTANCE.CheckGraspCollider(_colliders[0], false);
    }

    protected void OnGrab(object sender, InteractableObjectEventArgs e)
    {
        OutputController.INSTANCE.CheckGraspCollider(_colliders[0], true);
    }
#endif
}
