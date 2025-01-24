using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MICT.eDNA.Models
{
    public class Trial: BaseDataObject
    {
        [JsonIgnore, JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public TrialConfiguration Configuration;
        //possible actions
        //[Obsolete, JsonIgnore]
        [JsonIgnore]
        public List<Action> Actions { get { return _actions; } }
        [JsonIgnore]
        protected List<Action> _actions;
        [Obsolete, JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public HashSet<int> LinkedActions;
        [Obsolete, JsonIgnore]
        public HashSet<Condition> Conditions { get { return Configuration?.Conditions ?? _conditions; } }
        [JsonIgnore]
        protected HashSet<Condition> _conditions;
        [Obsolete, JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public HashSet<int> LinkedConditions;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public DateTime StartTime;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public float Duration;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [Range(0, 1)]
        public float Accuracy;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public Queue<Action> History;
        [Obsolete, JsonIgnore]
        public bool IsRequired;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public CompletionStatus Status;
        #pragma warning disable 0612
        public void SetActions(HashSet<Action> set) {
            _actions = PopulateFromIds(LinkedActions,set).ToList();
        }

        public void SetConditions(HashSet<Condition> set)
        {
            _conditions = PopulateFromIds(LinkedConditions, set);
        }

        public Trial() {
            ConfigId = -1;
            DatabaseId = -1;
        }
        
        public Trial(TrialConfiguration other) {
            ConfigId = other.ConfigId;
            DatabaseId = other.DatabaseId;
            Name = other.Name;
            Description = other.Description;
            IsRequired = other.IsRequired;
            Configuration = other;
            Parameter = other.Parameter;
            LinkedActions = other.LinkedActions;
            LinkedConditions = other.LinkedConditions;
            _conditions = other.Conditions;
            
            if (other.Actions != null && other.Actions.Count > 0)
            {
                _actions = new List<Action>();
                foreach (var actionConfig in other.Actions)
                {
                    _actions.Add(new Action(actionConfig));
                }
            }
        }
        #pragma warning restore 0612

        public static bool operator ==(Trial id1, Trial id2)
        {
            try
            {
                return id1.GetHashCode().Equals(id2?.GetHashCode());
            }
            catch
            {
                return EqualityComparer<Trial>.Default.Equals(id1, id2);
            }
        }

        public virtual bool Equals(Trial other)
        {
            try
            {
                return this.GetHashCode().Equals(other?.GetHashCode());
            }
            catch
            {
                return EqualityComparer<Trial>.Default.Equals(this, other);
            }
        }

        public override bool Equals(object other)
        {
            var otherObj = other as Trial;
            if (otherObj == null)
            {
                return false;
            }
            return GetHashCode() == otherObj?.GetHashCode();
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator !=(Trial id1, Trial id2)
        {
            return !(id1?.GetHashCode() == id2?.GetHashCode());
        }
    }

    [Serializable]
    public class TrialSelector
    {
        public TrialSelector() {
            _value = -1;
        }
        [SerializeField]
        private int _value;
        public int ConfigId { get { return _value; } set { _value = value; } }
    }
}
