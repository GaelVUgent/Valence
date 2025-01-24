using Newtonsoft.Json;
using System;
using System.ComponentModel;

namespace MICT.eDNA.Models
{
    [Serializable]
    public class ActionConfiguration : BaseDataObject
    {
        public bool IsRequired;
        //TODO: Tutor specific, should be removed later
        public bool IsPhysical { get; set; }

        //TODO: Possibility to link nudges to an action
        //TODO: Possibility to link certain UI elements to be larger on screen of wizard
        //TODO: Possibility to link certain actions to this actiom. e.g. When this action is activated... person starts coughing?

        public ActionConfiguration()
        {
            ConfigId = -1;
            DatabaseId = -1;
        }
        #pragma warning disable 0612
        public ActionConfiguration(Action oldDataStructure)
        {
            ConfigId = oldDataStructure.ConfigId == -1 ? oldDataStructure.DatabaseId : oldDataStructure.ConfigId;
            DatabaseId = oldDataStructure.DatabaseId;
            Name = oldDataStructure.Name;
            Description = oldDataStructure.Description;
            IsRequired = oldDataStructure.IsRequired;
            IsPhysical = oldDataStructure.IsPhysical;
            Parameter = oldDataStructure.Parameter;
        }
        #pragma warning restore 0612
    }
}
