using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MICT.eDNA.Models
{
    public class TrialConfiguration: BaseDataObject
    {
        //possible actions
        [JsonIgnore, JsonProperty(ReferenceLoopHandling = ReferenceLoopHandling.Ignore)]
        public List<ActionConfiguration> Actions { get { return _actions; } }
        [JsonIgnore, JsonProperty(ReferenceLoopHandling = ReferenceLoopHandling.Ignore)]
        protected List<ActionConfiguration> _actions;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public HashSet<int> LinkedActions;
        [JsonIgnore, JsonProperty(ReferenceLoopHandling = ReferenceLoopHandling.Ignore)]
        public HashSet<Condition> Conditions { get { return _conditions; } }
        [JsonIgnore, JsonProperty(ReferenceLoopHandling = ReferenceLoopHandling.Ignore)]
        protected HashSet<Condition> _conditions;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public HashSet<int> LinkedConditions;
        public bool IsRequired;

        public void SetActions(HashSet<ActionConfiguration> set) {
            _actions = PopulateFromIds(LinkedActions,set).ToList();
        }

        public void SetConditions(HashSet<Condition> set)
        {
            _conditions = PopulateFromIds(LinkedConditions, set);
        }

        public TrialConfiguration() {
            ConfigId = -1;
            DatabaseId = -1;
        }
        #pragma warning disable 0612
        public TrialConfiguration(Trial oldDataStructure)
        {
            //fallback to import old datastructure that doesnt have configId yet
            ConfigId = oldDataStructure.ConfigId == -1 ? oldDataStructure.DatabaseId : oldDataStructure.ConfigId;
            DatabaseId = oldDataStructure.DatabaseId;
            Name = oldDataStructure.Name;
            Description = oldDataStructure.Description;
            IsRequired = oldDataStructure.IsRequired;
            Parameter = oldDataStructure.Parameter;
            LinkedActions = oldDataStructure.LinkedActions;
            LinkedConditions = oldDataStructure.LinkedConditions;
            _conditions = oldDataStructure.Conditions;

            if (oldDataStructure.Actions != null && oldDataStructure.Actions.Count > 0)
            {
                _actions = new List<ActionConfiguration>();
                foreach (var action in oldDataStructure.Actions)
                {
                    _actions.Add(new ActionConfiguration(action));
                }
            }
        }
        #pragma warning restore 0612
    }
}
