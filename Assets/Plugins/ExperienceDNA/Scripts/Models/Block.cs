using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MICT.eDNA.Models
{
    public class Block: BaseDataObject
    {
        [JsonIgnore, JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public BlockConfiguration Configuration;
        [Obsolete]
        public List<Trial> Trials;
        [Obsolete]
        public HashSet<Condition> Conditions { internal get { return Configuration?.Conditions ?? _conditions; } set { _conditions = value; } }
        [Obsolete]
        protected HashSet<Condition> _conditions;
        [Obsolete]
        public HashSet<int> LinkedConditions { internal get; set; }
        [Obsolete]
        public bool IsBreak { internal get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public DateTime StartTime;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public float Duration;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [Range(0,1)]
        public float Accuracy;
        #pragma warning disable 0612
        public void SetConditions(HashSet<Condition> set)
        {
            _conditions = PopulateFromIds(LinkedConditions, set);
        }

        public Block() {
            ConfigId = -1;
            DatabaseId = -1;
        }
        
        public Block(BlockConfiguration other)
        {
            ConfigId = other.ConfigId;
            DatabaseId = other.DatabaseId;
            Name = other.Name;
            Description = other.Description;
            Configuration = other;
            Parameter = other.Parameter;
            LinkedConditions = other.LinkedConditions;
            IsBreak = other.IsBreak;

            if (other.Trials != null && other.Trials.Count > 0) {
                Trials = new List<Trial>();
                foreach (var trialConfig in other.Trials)
                {
                    Trials.Add(new Trial(trialConfig));

                }
            }
        }
        #pragma warning restore 0612

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(Block id1, Block id2)
        {
            try
            {
                return id1.GetHashCode().Equals(id2?.GetHashCode());
            }
            catch
            {
                return EqualityComparer<Block>.Default.Equals(id1, id2);
            }
        }

        public virtual bool Equals(Block other)
        {
            try
            {
                return this.GetHashCode().Equals(other?.GetHashCode());
            }
            catch
            {
                return EqualityComparer<Block>.Default.Equals(this, other);
            }
        }

        public override bool Equals(object other)
        {
            var otherObj = other as Block;
            if (otherObj == null)
            {
                return false;
            }
            return GetHashCode() == otherObj?.GetHashCode();
        }

        public static bool operator !=(Block id1, Block id2)
        {
            return !(id1?.GetHashCode() == id2?.GetHashCode());
        }
    }

    [Serializable]
    public class BlockSelector {
        public BlockSelector()
        {
            _value = -1;
        }
        [SerializeField]
        private int _value;
        public int ConfigId { get { return _value; } set { _value = value; } }
    }
}