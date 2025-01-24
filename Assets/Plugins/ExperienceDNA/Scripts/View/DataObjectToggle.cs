using MICT.eDNA.Controllers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MICT.eDNA.Models
{
    
    public class DataObjectToggle : MonoBehaviour, IPointerClickHandler
    {
        private Toggle _toggle;
        private TMP_Text _text;
        private ExperienceCreatorController _controller;
        public BlockConfiguration Block;
        public TrialConfiguration Trial;
        public ActionConfiguration Action;

        private Block _dummyBlock;
        private Trial _dummyTrial;
        private Action _dummyAction;
        public Condition Condition;
        private BaseDataObject _object;
        public ObjectType Type;
        private bool _toggleValue = false;

        void Start()
        {
            _toggle = GetComponent<Toggle>();
            _toggleValue = _toggle.isOn;
            _text = GetComponent<TMP_Text>();
            _toggle.group = GetComponentInParent<ToggleGroup>();
            _controller = GetComponentInParent<ExperienceCreatorController>();
            _toggle?.onValueChanged?.AddListener(OnToggled);
        }

        void OnDestroy() {
            _toggle?.onValueChanged?.RemoveListener(OnToggled);
        }

        public void Init<T>(T data) where T : BaseDataObject
        {
            if (data is Block)
            {
                Type = ObjectType.Block;
                _dummyBlock = data as Block;
            }
            else if (data is Trial)
            {
                Type = ObjectType.Trial;
                _dummyTrial = data as Trial;
            }
            else if (data is Action)
            {
                Type = ObjectType.Action;
                _dummyAction = data as Action;
            }
            else if (data is Condition)
            {
                Type = ObjectType.Condition;
                Condition = data as Condition;
            }
            if (data is BlockConfiguration)
            {
                Type = ObjectType.Block;
                Block = data as BlockConfiguration;
                _dummyBlock = new Block(Block);
            }
            else if (data is TrialConfiguration)
            {
                Type = ObjectType.Trial;
                Trial = data as TrialConfiguration;
                _dummyTrial = new Trial(Trial);
            }
            else if (data is ActionConfiguration)
            {
                Type = ObjectType.Action;
                Action = data as ActionConfiguration;
                _dummyAction = new Action(Action);
            }
            _object = data;
            SetName(data?.Name ?? "Unnamed");
            
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            switch (Type)
            {
                case ObjectType.Condition:
                    _controller.SetActive<Condition>(_toggle.isOn ? Condition : Condition);
                    break;
                case ObjectType.Block:
                    _controller.SetActive<BlockConfiguration>(_toggle.isOn ? Block : null);
                    break;
                case ObjectType.Trial:
                    _controller.SetActive<TrialConfiguration>(_toggle.isOn ? Trial : null);
                    break;
                case ObjectType.Action:
                    _controller.SetActive<ActionConfiguration>(_toggle.isOn ? Action : Action);
                    break;
                default:
                    break;
            }
        }



        public void SetName(string name) {
            if (_text == null) {
                _text = GetComponent<TMP_Text>();
            }
            _text?.SetText(name);
        }

        private void OnToggled(bool isOn)
        {
            //if (!isOn)
            //{
                switch (Type)
                {
                    case ObjectType.Condition:
                        SetName(Condition.Name);
                        break;
                    case ObjectType.Block:
                        SetName(Block.Name);
                        break;
                    case ObjectType.Trial:
                        SetName(Trial.Name);
                        break;
                    case ObjectType.Action:
                        SetName(Action.Name);
                        break;
                    default:
                        SetName(_object.Name);
                        break;
                }
            //}
        }

        public enum ObjectType { 
            Condition,
            Block,
            Trial,
            Action,
        }

    }
}
