using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Serialization;

namespace MICT.eDNA.Models
{
    public class BaseDataObject: IEquatable<BaseDataObject>
    {
        [SerializeField, JsonProperty("Id", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate), DefaultValue(-1)]
        private int _value;
        [JsonIgnore]
        public int DatabaseId { get => _value; set => _value = value; }
        //to the people upgrading their projects: ConfigId is the new Id. (the Id(_value) found in the jsons is purely left as this name to support the Django backend)
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate), DefaultValue(-1)] 
        public int ConfigId;
        public string Name;
        public string Description;
        [JsonIgnore]
        public object Parameter;

        public BaseDataObject()
        {
            ConfigId = -1;
            DatabaseId = -1;
        }

        public BaseDataObject(int id)
        {
            ConfigId = id;
        }

        protected HashSet<T> PopulateFromIds<T>(ICollection<int> ids, IEnumerable<T> allData) where T : BaseDataObject, new()
        {
            var value = new HashSet<T>();
            if (allData != null && ids != null)
            {
                foreach (var item in allData)
                {
                    if (item.ConfigId > -1)
                    {
                        if (ids.Contains(item.ConfigId))
                        {
                            value.Add(item);
                            continue;
                        }
                    }
                    else if (item.DatabaseId > -1) {
                        if (ids.Contains(item.ConfigId))
                        {
                            value.Add(item);
                        }
                    }
                }
            }
            return value;
        }

        protected List<T> PopulateFromIdsAsList<T>(ICollection<int> ids, IEnumerable<T> allData) where T : BaseDataObject, new()
        {
            var value = new List<T>();
            if (allData != null && ids != null)
            {
                foreach (var item in allData)
                {
                    if (item.ConfigId > -1)
                    {
                        if (ids.Contains(item.ConfigId))
                        {
                            value.Add(item);
                            continue;
                        }
                    }
                    else if (item.DatabaseId > -1)
                    {
                        if (ids.Contains(item.ConfigId))
                        {
                            value.Add(item);
                        }
                    }
                }
            }
            return value;
        }

        public override int GetHashCode()
        {
            //making sure we can still get the old experiment structure - see HashSets. Should be able to delete later when old structures are gone
            if (ConfigId == -1 && DatabaseId != -1) {
                ConfigId = DatabaseId;
            }
            return ConfigId;
        }

        public static bool operator ==(BaseDataObject id1, BaseDataObject id2)
        {
            try {
                return id1.GetHashCode().Equals(id2?.GetHashCode());
            }
            catch
            {
                return EqualityComparer<BaseDataObject>.Default.Equals(id1, id2);
            }
        }

        public virtual bool Equals(BaseDataObject other)
        {
            try
            {
                return this.GetHashCode().Equals(other?.GetHashCode());
            }
            catch
            {
                return EqualityComparer<BaseDataObject>.Default.Equals(this, other);
            }
        }

        public override bool Equals(object other)
        {
            var otherObj = other as BaseDataObject;
            if (otherObj == null)
            {
                return false;
            }
            return GetHashCode() == otherObj?.GetHashCode();
        }

        public static bool operator !=(BaseDataObject id1, BaseDataObject id2)
        {
            return !(id1?.GetHashCode() == id2?.GetHashCode());
        }

        //Django databases start with ids of 1, not 0. Obselete since we split Id up into DatabaseId and ConfigId
        [Obsolete]
        public bool UpdateZeroIdIfNeeded(int replacementId = 10000) {
            if (ConfigId == 0) {
                ConfigId = replacementId;
                return true;
            }
            return false;
        }
    }
}