using Newtonsoft.Json;
using System.Collections.Generic;

namespace MICT.eDNA.Models
{
    public class Output
    {
        public Experience Experience { get; set; }
        [JsonIgnore]
        public Experiment Experiment { get; set; }
        public List<SingleOutputFrame> OutputFrames { get; set; }
    }
}
