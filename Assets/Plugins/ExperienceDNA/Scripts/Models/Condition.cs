using System;
using System.Collections.Generic;

namespace MICT.eDNA.Models
{
    public class Condition: BaseDataObject
    {
        public Condition()
        {
            ConfigId = -1;
            DatabaseId = -1;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(Condition id1, Condition id2)
        {
            try
            {
                return id1.GetHashCode().Equals(id2?.GetHashCode());
            }
            catch
            {
                return EqualityComparer<Condition>.Default.Equals(id1, id2);
            }
        }

        public virtual bool Equals(Condition other)
        {
            try
            {
                return this.GetHashCode().Equals(other?.GetHashCode());
            }
            catch
            {
                return EqualityComparer<Condition>.Default.Equals(this, other);
            }
        }

        public override bool Equals(object other)
        {
            var otherObj = other as Condition;
            if (otherObj == null)
            {
                return false;
            }
            return GetHashCode() == otherObj?.GetHashCode();
        }

        public static bool operator !=(Condition id1, Condition id2)
        {
            return !(id1?.GetHashCode() == id2?.GetHashCode());
        }
    }

    [Serializable]
    public class ConditionSelector
    {
        public ConditionSelector()
        {
            _value = -1;
        }
        [UnityEngine.SerializeField]
        private int _value;
        public int ConfigId { get { return _value; } set { _value = value; } }
    }
}
