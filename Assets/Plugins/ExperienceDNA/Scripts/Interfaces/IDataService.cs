using MICT.eDNA.Models;
using System;
using UnityEngine;

namespace MICT.eDNA.Interfaces
{
    public interface IDataService
    {
        event EventHandler OnDataLoaded;
        Experiment Data { get; }
        Experience DataStructure { get; }
        void Init(MonoBehaviour behaviour, bool useStreamingAssetsFolder = false, bool automaticallyStartWritingOutput = true);
        void SaveDataToLocal(Experiment data, bool writeToPersistent = true, string folderPath = "", string filename = "");
        Models.ActionConfiguration GetActionConfigurationFromSelector(ActionSelector selector);
        TrialConfiguration GetTrialConfigurationFromSelector(TrialSelector selector);
        BlockConfiguration GetBlockConfigurationFromSelector(BlockSelector selector);
        Models.Action GetActionFromSelector(ActionSelector selector);
        Trial GetTrialFromSelector(TrialSelector selector);
        Block GetBlockFromSelector(BlockSelector selector);
        Condition GetConditionFromSelector(ConditionSelector selector);

        //OUTPUT
        void CloseStream(bool finishFile = true);
        void SetCurrentOutputFrame(SingleOutputFrame frame);
        void SetEndOutput(Output data);
        void StartOutput(bool startNewFile = false);
        
        //TODO: Later add Post methods to send data as well
    }
}
