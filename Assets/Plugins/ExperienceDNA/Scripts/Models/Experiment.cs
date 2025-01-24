using MICT.eDNA.Managers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace MICT.eDNA.Models
{
    public class Experiment
    {
        public int Id = -1;
        public string Name;
        public DateTime Date;
        public float DurationBetweenBlocks;
        public float DurationBetweenTrials;
        public List<Block> Blocks;
        public HashSet<Condition> AllConditions;
        public HashSet<Action> AllActions;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore), DefaultValue(-1)]
        public int ExperienceId{ private get; set; }
        private Experience _experience;
        #pragma warning disable 0612
        public void LinkIds()
        {
            if (Blocks != null && AllActions != null)
            {
                foreach (var block in Blocks)
                {
                    block.SetConditions(AllConditions);
                    if (block.Trials != null)
                    {
                        foreach (var trial in block.Trials)
                        {
                            if(AllActions?.Count > 0)
                                trial.SetActions(new HashSet<Action>(AllActions));
                            if(AllConditions?.Count > 0)
                                trial.SetConditions(new HashSet<Condition>(AllConditions));
                        }
                    }
                }
            }
        }
        #pragma warning restore 0612

        public int GetExperienceId() {
            return ExperienceId;
        }

        public Experiment() {
            Date = DateTime.Now;
        }

        public void UpdateName() {
            var userId = ServiceLocator.UserService?.CurrentUser?.ParticipantNumber.ToString("D3") ?? "000";
            Name = $"{_experience?.Name ?? "Untitled"} {userId} {Date:HH-mm}";
        }
        public Experiment(Experience structure)
        {
            _experience = structure;
            var userId = ServiceLocator.UserService?.CurrentUser?.ParticipantNumber.ToString("D3") ?? "000";          
            Date = DateTime.Now;
            Name = $"{structure.Name} {userId} {Date:HH-mm}";
            DurationBetweenBlocks = structure.DurationBetweenBlocks;
            DurationBetweenTrials = structure.DurationBetweenTrials;
            AllConditions = structure.AllConditions;

            HashSet<Action> actions = new HashSet<Action>();
            List<Block> blocks = new List<Block>();
            if (structure.AllActions != null && structure.AllActions.Count > 0)
            {
                foreach (var actionConfig in structure.AllActions)
                {
                    actions.Add(new Action(actionConfig));
                }
                AllActions = actions;
            }
            if (structure.Blocks != null && structure.Blocks.Count > 0)
            {
                foreach (var blockConfig in structure.Blocks)
                {
                    blocks.Add(new Block(blockConfig));
                    //trials and actions are added dynamically via the Block initialiser
                }
                Blocks = blocks;
            }
        }
    }
}
