using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace MICT.eDNA.Models
{
    public class Experience
    {
        [DefaultValue(-1), JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int Id = -1;
        public string Name;
        public string Description;
        public float DurationBetweenBlocks;
        public float DurationBetweenTrials;
        public List<BlockConfiguration> Blocks;
        public HashSet<Condition> AllConditions;
        public HashSet<ActionConfiguration> AllActions;
        public HashSet<TrialConfiguration> AllTrials;

        public void LinkIds()
        {
            if (Blocks != null && AllActions != null)
            {
                foreach (var block in Blocks)
                {
                    block.SetConditions(AllConditions);
                    block.SetTrials(AllTrials);
                    if (block.Trials != null)
                    {
                        foreach (var trial in block.Trials)
                        {
                            trial.SetActions(AllActions);
                            trial.SetConditions(AllConditions);
                        }
                    }
                }
            }
        }

        public Experience() { }

        public Experience(Experiment oldDataStructure)
        {
            Id = -1;
            DurationBetweenBlocks = oldDataStructure.DurationBetweenBlocks;
            DurationBetweenTrials = oldDataStructure.DurationBetweenTrials;
            HashSet<Condition> conditions = new HashSet<Condition>();
            AllConditions = oldDataStructure.AllConditions;
            
            HashSet<ActionConfiguration> actions = new HashSet<ActionConfiguration>();
            List<BlockConfiguration> blocks = new List<BlockConfiguration>();
            HashSet<TrialConfiguration> trials = new HashSet<TrialConfiguration>();
            if (oldDataStructure.AllActions != null && oldDataStructure.AllActions.Count > 0)
            {
                foreach (var action in oldDataStructure.AllActions)
                {
                    actions.Add(new ActionConfiguration(action));
                }
                AllActions = actions;
            }

            if (oldDataStructure.Blocks != null && oldDataStructure.Blocks.Count > 0)
            {
                foreach (var block in oldDataStructure.Blocks)
                {
                    var blockConfig = new BlockConfiguration(block);
                    blocks.Add(blockConfig);
                    if (block.Trials?.Count > 0)
                    {
                        //trials and expected actions are added dynamically via the Block initialiser
                        foreach (var trial in blockConfig.Trials)
                        {
                            trials.Add(trial);
                        }
                    }
                }
                Blocks = blocks;

            }
            AllTrials = trials;
        }
    }
}
