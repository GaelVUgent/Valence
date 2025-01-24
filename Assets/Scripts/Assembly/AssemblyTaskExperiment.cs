
using MICT.eDNA.Controllers;
using MICT.eDNA.Managers;
/// <summary>
/// Implementation of the assembly task manager class, 
/// specific to the real experiment.
/// </summary>
public class AssemblyTaskExperiment : AssemblyTask
{

    public AssemblyMenu menu;

    private int instructionStep;

    protected override void Awake()
    {
        base.Awake();
    }

    private void OnDestroy()
    {
        OutputController.ClearCurrentData();
        if (ServiceLocator.UserService.CurrentUser != null)
        {
            ServiceLocator.UserService.OverrideParticipantNumber(ServiceLocator.UserService.CurrentUser.ParticipantNumber + 1);
        }
    }

    public override void ResetAssembly()
    {
        base.ResetAssembly();
        OutputController.StartWriting();
        instructionStep = 0;
    }

    protected override void OnReset()
    {
        DataLogger.LogEventStart(ExperimentData.Event.ResetPieces);
    }

    protected override void OnPlace(NodeAttach attach)
    {
        string attDetails = attach.ToString();
        DataLogger.LogEventStart(ExperimentData.Event.SnapPiece, attDetails);
        if(candidateAssemblyAttach != null)
            menu.RequestConfirmAssembly();
    }

    public override void UnlockPiece(Grabbable piece)
    {
        if (assemblyAttach != null && piece == assemblyAttach.piece)
        {
            DataLogger.LogEventEnd(ExperimentData.Event.SnapPiece);
        }
        if(candidateAssemblyAttach != null && piece == candidateAssemblyAttach.piece)
        {
            candidateAssemblyAttach = null;
            menu.CancelConfirmAssembly();
        }
    }

    protected override void OnConfirm()
    {
        DataLogger.LogEventEnd(ExperimentData.Event.SnapPiece);
        menu.Feedback(candidateAssemblyAttach.Confirm());
    }

    protected override void OnContinue(AssemblyCheckpoint nextCP)
    {
        if (nextCP == null)
        {
            //no more checkpoints; task is done
            DataLogger.FinishSequence();
            currentTaskIndex++;
            int nTasks = Global.global.flow.tasks.Length;
            menu.EndTaskMenu(currentTaskIndex, nTasks);
        }
        else
        {
            //move on to next piece
            helpLevel = Global.global.flow.GetHelpLevel(currentTaskIndex, nextPieceIndex);
            DataLogger.StartStep(nextPieceIndex, nextCP.instructionStep, (int)helpLevel + 1);
            nextCP.target.SetAvailable();
            UpdateHelp();

            instructionStep = nextCP.instructionStep;
            menu.SetInstruction(instructionStep);
        }
    }
}
