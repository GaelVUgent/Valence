using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MICT.eDNA.Models
{
    public class BlockConfiguration : BaseDataObject
    {
        [JsonIgnore]
        public List<TrialConfiguration> Trials { protected set; get; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public List<int> LinkedTrials;
        [JsonIgnore]
        public HashSet<Condition> Conditions { get { return _conditions; } }
        protected HashSet<Condition> _conditions;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public HashSet<int> LinkedConditions;
        public bool IsBreak;

        public void SetConditions(HashSet<Condition> set)
        {
            _conditions = PopulateFromIds(LinkedConditions, set);
        }

        public void SetTrials(HashSet<TrialConfiguration> set)
        {
            Trials = PopulateFromIdsAsList(LinkedTrials, set);
        }

        public BlockConfiguration() {
            ConfigId = -1;
            DatabaseId = -1;
        }
        #pragma warning disable 0612
        public BlockConfiguration(Block oldDataStructure)
        {
            ConfigId = oldDataStructure.ConfigId == -1 ? oldDataStructure.DatabaseId : oldDataStructure.ConfigId;
            DatabaseId = oldDataStructure.DatabaseId;
            Name = oldDataStructure.Name;
            Description = oldDataStructure.Description;
            Parameter = oldDataStructure.Parameter;
            LinkedConditions = oldDataStructure.LinkedConditions;
            IsBreak = oldDataStructure.IsBreak;

            if (oldDataStructure.Trials != null && oldDataStructure.Trials.Count > 0)
            {
                LinkedTrials = new List<int>();
                Trials = new List<TrialConfiguration>();
                foreach (var trial in oldDataStructure.Trials)
                {
                    //is removed now that Id is replaced with ConfigId and not used as pk in Django database
                    /*
                    if (trial.ConfigId == 0)
                    {
                        trial.UpdateZeroIdIfNeeded();
                    }*/

                    var trialConfig = new TrialConfiguration(trial);
                    Trials.Add(trialConfig);
                    LinkedTrials.Add(trialConfig.ConfigId);
                }
            }
        }
        #pragma warning restore 0612
    }
}