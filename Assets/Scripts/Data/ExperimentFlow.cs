using System;

/// <summary>
/// Contains custom input data for the experiment, managing how many tasks 
/// occur and their helping level.
/// This class has been implemented entirely so it can be more easily 
/// customized in the future.
/// </summary>
[Serializable]
public struct ExperimentFlow
{

    public AssemblyTask[] tasks;

    public AssemblyHelper.HelpLevel GetHelpLevel(int sequenceIndex, int pieceIndex)
    {
        //default
        AssemblyHelper.HelpLevel hl = AssemblyHelper.HelpLevel.PieceOnly;
        if (!IsValid(sequenceIndex, pieceIndex))
            return hl;
        int stepIndex = pieceIndex - 1;
        hl = (AssemblyHelper.HelpLevel)(tasks[sequenceIndex].steps[stepIndex].helpLevel - 1);
        return hl;

    }

    public float GetAssemblyAngle(int sequenceIndex)
    {
        if (!IsValid(sequenceIndex))
            return 0f;
        return tasks[sequenceIndex].assemblyAngle;
    }

    private bool IsValid(int sequenceIndex)
    {
        return sequenceIndex >= 0
            & sequenceIndex < tasks.Length;
    }

    private bool IsValid(int sequenceIndex, int pieceIndex)
    {
        int stepIndex = pieceIndex - 1;
        return IsValid(sequenceIndex) &&
            (stepIndex >= 0 & stepIndex < tasks[sequenceIndex].steps.Length);
    }

    [Serializable]
    public struct AssemblyTask
    {
        public AssemblyStep[] steps;
        public float assemblyAngle;
    }

    [Serializable]
    public struct AssemblyStep
    {
        public int helpLevel;
    }
}
