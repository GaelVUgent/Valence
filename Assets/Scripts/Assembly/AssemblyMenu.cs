using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Provides instructions and UI during an assembly task.
/// Currently only used for the real experiment assembly.
/// </summary>
public class AssemblyMenu : MonoBehaviour
{

    public AssemblyTask assembly;
    public GameObject confirmAssemblyMenu, userFeedbackMenu, assemblyOptionMenu, endTaskMenu;
    public Transform instructionHolder;
    public Animator positiveFeedback, negativeFeedback;
    public Text endTaskText;
    public GameObject endTaskButton;
    public GameObject endAnimation;

    public bool askUserFeedback;

    private Texture[] instructions;
    private int currentInstruction;

    private void Start()
    {
        confirmAssemblyMenu.SetActive(false);
        userFeedbackMenu.SetActive(false);
        assemblyOptionMenu.SetActive(true);
        SetInstruction(1);
    }

    public void PreviousInstruction()
    {
        SetInstruction(currentInstruction - 1);
    }

    public void NextIntsruction()
    {
        SetInstruction(currentInstruction + 1);
    }

    /// <summary>
    /// Set the instruction screen to show the instruction with the given index
    /// (1 - based index for convenience with the data).
    /// This method will disable the instruction screen if the given index was not found.
    /// </summary>
    public void SetInstruction(int index)
    {
        instructionHolder.gameObject.SetActive(true);
        for (int i = 0; i < instructionHolder.childCount; i++)
        {
            instructionHolder.GetChild(i).gameObject.SetActive((i + 1) == index);
        }
    }

    /// <summary>
    /// Enable the menu for confirming the assembly
    /// </summary>
    public void RequestConfirmAssembly()
    {
        confirmAssemblyMenu.SetActive(true);
        assemblyOptionMenu.SetActive(false);
    }

    /// <summary>
    /// Disable the menu for confirming the assembly
    /// and return to the default assembly options (reset button)
    /// </summary>
    public void CancelConfirmAssembly()
    {
        confirmAssemblyMenu.SetActive(false);
        assemblyOptionMenu.SetActive(true);
    }

    /// <summary>
    /// Show positive or negative feedback. To be called 
    /// after a piece placement has been confirmed.
    /// </summary>
    public void Feedback(bool positive)
    {
        confirmAssemblyMenu.SetActive(false);
        instructionHolder.gameObject.SetActive(false);
        assemblyOptionMenu.SetActive(false);
        StartCoroutine(FeedbackSequence(positive));
    }

    private IEnumerator FeedbackSequence(bool positive)
    {
        userFeedbackMenu.SetActive(false);
        if (positive)
            positiveFeedback.SetTrigger("Feedback");
        else
            negativeFeedback.SetTrigger("Feedback");

        yield return new WaitForSeconds(2.5f);

        if (askUserFeedback)
            userFeedbackMenu.SetActive(true);
        else
            ReceiveFeedback(0);
    }

    /// <summary>
    /// Remove currently attached piece to try again
    /// (unused)
    /// </summary>
    public void RetryAssembly()
    {
        confirmAssemblyMenu.SetActive(false);
        assemblyOptionMenu.SetActive(true);
        assembly.RedoNextPiece();
    }

    /// <summary>
    /// Pass on confirm button input. Confirm the button 
    /// currently attached to the assembly.
    /// </summary>
    public void ConfirmAssembly()
    {
        assembly.ConfirmNextPiece();
    }

    /// <summary>
    /// Reset any free pieces remaining for the current assembly.
    /// </summary>
    public void ResetPieces()
    {
        assembly.ResetPieces(false);
    }

    /// <summary>
    /// Pass on user feedback back to the assembly task (and data logger).
    /// Continue the assembly
    /// </summary>
    public void ReceiveFeedback(int feedback)
    {
        userFeedbackMenu.SetActive(false);
        assemblyOptionMenu.SetActive(true);
        DataLogger.FeedbackStep(feedback);
        assembly.Continue();
    }

    /// <summary>
    /// Show the end menu and display how many tasks remain.
    /// This menu gives the user the option to start the next 
    /// sequence, if possible.
    /// </summary>
    /// <param name="currentTask">Index of the currently finished assembly sequence</param>
    /// <param name="totalTasks">Total number of sequences to be performed during the experiment</param>
    public void EndTaskMenu(int currentTask, int totalTasks)
    {
        endAnimation.SetActive(false);
        endAnimation.SetActive(true);
        instructionHolder.gameObject.SetActive(false);
        confirmAssemblyMenu.SetActive(false);
        userFeedbackMenu.SetActive(false);
        assemblyOptionMenu.SetActive(false);
        endTaskMenu.SetActive(true);

        bool finishedExperiment = currentTask >= totalTasks;
        if (finishedExperiment)
        {
            endTaskButton.SetActive(false);
            endTaskText.text = BabelTranslator.Tr("ASSEMBLY_FINISHED");
        }
        else
        {
            endTaskButton.SetActive(true);
            endTaskText.text = string.Format(BabelTranslator.Tr("ASSEMBLY_SUB_FINISHED"), currentTask, totalTasks);
        }
    }

    /// <summary>
    /// Pass on button press for starting the next assembly sequence.
    /// </summary>
    public void FinishTask()
    {
        confirmAssemblyMenu.SetActive(false);
        userFeedbackMenu.SetActive(false);
        assemblyOptionMenu.SetActive(true);
        endTaskMenu.SetActive(false);
        assembly.ResetAssembly();
    }
}
