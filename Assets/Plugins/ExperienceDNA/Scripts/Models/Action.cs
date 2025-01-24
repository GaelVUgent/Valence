using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine.Serialization;

namespace MICT.eDNA.Models
{
    [Serializable]
    public class Action: BaseDataObject
    {
        [JsonIgnore, JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public ActionConfiguration Configuration;
        [Obsolete]
        public bool IsRequired { internal get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public DateTime TimeStamp;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public float Duration;

        //Tutor specific
        [Obsolete]
        public bool IsPhysical { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue(-1)]
        public int CompletedInTrialId { get; private set; }
       
        //TODO: Possibility to link nudges to an action
        //TODO: Possibility to link certain UI elements to be larger on screen of wizard
        //TODO: Possibility to link certain actions to this actiom. e.g. When this action is activated... person starts coughing?

        public void SetComplete(Trial currentTrial)
        {
            CompletedInTrialId = currentTrial.ConfigId;
        }

        public Action()
        {
            ConfigId = -1;
            DatabaseId = -1;
        }
        #pragma warning disable 0612
        public Action(Action other) {
            ConfigId = other.ConfigId;
            DatabaseId = other.DatabaseId;
            Name = other.Name;
            Description = other.Description;
            
            IsRequired = other.IsRequired;
            TimeStamp = DateTime.Now;
            Duration = other.Duration;
            IsPhysical = other.IsPhysical;
            CompletedInTrialId = other.CompletedInTrialId;

        }

        public Action(ActionConfiguration other)
        {
            ConfigId = other.ConfigId;
            Name = other.Name;
            Description = other.Description;
            IsRequired = other.IsRequired;
            IsPhysical = other.IsPhysical;
            Configuration = other;
            Parameter = other.Parameter;
        }
        #pragma warning restore 0612

        public static bool operator ==(Action id1, Action id2)
        {
            try
            {
                return id1.GetHashCode().Equals(id2?.GetHashCode());
            }
            catch
            {
                return EqualityComparer<Action>.Default.Equals(id1, id2);
            }
        }

        public virtual bool Equals(Action other)
        {
            try
            {
                return this.GetHashCode().Equals(other?.GetHashCode());
            }
            catch
            {
                return EqualityComparer<Action>.Default.Equals(this, other);
            }
        }

        public override bool Equals(object other)
        {
            var otherObj = other as Action;
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

        public static bool operator !=(Action id1, Action id2)
        {
            return !(id1?.GetHashCode() == id2?.GetHashCode());
        }

    }
    [Serializable]
    public class ActionSelector
    {
        public ActionSelector()
        {
            _value = -1;
        }
        [UnityEngine.SerializeField]
        public int _value;
        public int ConfigId { get { return _value; } set { _value = value; } }
    }
}
