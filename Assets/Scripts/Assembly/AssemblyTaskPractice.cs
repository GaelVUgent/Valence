using System.Collections;
using UnityEngine;

/// <summary>
/// Implementation of the assembly task manager class, 
/// specific to the practice assembly.
/// This is a typical video game tutorial, with hard coded 
/// responses to user actions.
/// </summary>
public class AssemblyTaskPractice : AssemblyTask
{

    private int step;

    public GameObject confirmButton;
    public GameObject resetButton;
    public Animator negativeFeedback, positiveFeedback;

    public bool askUserFeedback;

    public Transform instructionHolder;

    public override void ResetAssembly()
    {
        base.ResetAssembly();
        SetStep(0);
    }

    private void SetStep(int step)
    {
        this.step = step;

        SetInstruction(step + 1);
        switch (step)
        {

            case 0:
                //continue by confirm button
                confirmButton.SetActive(true);
                resetButton.SetActive(false);
                foreach (AssemblyPiece p in assemblyPieces)
                    p.gameObject.SetActive(false);
                helper.gameObject.SetActive(false);
                break;

            case 1:
                //continue by Update()
                assemblyPieces[0].gameObject.SetActive(true);
                resetButton.SetActive(true);
                break;

            case 2:
                //continue by Continue()

                resetButton.SetActive(true);
                break;

            case 3:
                //continue by OnPlace()
                assemblyPieces[1].gameObject.SetActive(true);
                resetButton.SetActive(true);
                confirmButton.SetActive(false);
                break;

            case 4:
                //revert by UnlockPiece()
                resetButton.SetActive(false);
                break;

            case 5:
                //revert by UnlockPiece() or continue by confirmButton
                resetButton.SetActive(false);
                confirmButton.SetActive(true);
                break;

            case 6:
                StartCoroutine(Feedback(true));
                //continue by feedback (identical to confirm)
                resetButton.SetActive(false);
                break;

            case 7:
                //continue by Update()
                helper.gameObject.SetActive(true);
                foreach (AssemblyPiece p in assemblyPieces)
                    p.gameObject.SetActive(true);
                assemblyPieces[2].SetAvailable();
                helpLevel = AssemblyHelper.HelpLevel.PieceOnly;
                UpdateHelp();
                break;

            case 8:
                //continue by Reset()
                resetButton.SetActive(true);
                break;

            case 9:
                //continue by standard sequence (confirm button after snap)
                break;

            case 10:
                //continue by standard sequence (confirm button after snap)
                assemblyPieces[3].SetAvailable();
                helpLevel = AssemblyHelper.HelpLevel.Positional;
                UpdateHelp();
                break;

            case 11:
                //continue by standard sequence (confirm button after snap)
                assemblyPieces[4].SetAvailable();
                helpLevel = AssemblyHelper.HelpLevel.PosRot;
                UpdateHelp();
                break;

            case 12:
                //continue by standard sequence (confirm button after snap)
                assemblyPieces[5].SetAvailable();
                UpdateHelp();
                break;

            case 13:
                //continue by confirm button
                confirmButton.SetActive(true);
                resetButton.SetActive(false);
                break;

            case 14:
                //done
                Global.ReturnToMenu();
                break;

        }
    }

    private IEnumerator Feedback(bool positive)
    {
        instructionHolder.gameObject.SetActive(false);
        if (positive)
            positiveFeedback.SetTrigger("Feedback");
        else
            negativeFeedback.SetTrigger("Feedback");
        yield return new WaitForSeconds(2.5f);
        instructionHolder.gameObject.SetActive(true);
    }

    protected override void Update()
    {
        base.Update();

        if (step == 1)
        {
            if (assemblyPieces[0].currentGrabber != null)
            {
                SetStep(++step);
            }
        }

        if (step == 7)
        {
            if (assemblyPieces[2].currentGrabber != null && assemblyPieces[2].currentGrabber.canPlace)
            {
                SetStep(++step);
                assemblyPieces[2].Release();
                Rigidbody rb = assemblyPieces[2].GetComponent<Rigidbody>();
                StartCoroutine(EjectBlock(rb));
            }
        }
    }

    private IEnumerator EjectBlock(Rigidbody rb)
    {
        rb.velocity = new Vector3(-3f, 3f, 0f);
        rb.detectCollisions = false;
        yield return new WaitForSeconds(.3f);
        rb.detectCollisions = true;
    }

    protected override void OnReset()
    {
        if (step == 8)
            SetStep(9);
    }

    protected override void OnPlace(NodeAttach attach)
    {
        if (step == 3)
        {
            if (attach.onCP)
                SetStep(5);
            else
                SetStep(4);
        }
        if (step >= 9 & step <= 12)
        {
            confirmButton.SetActive(true);
        }
    }

    public override void UnlockPiece(Grabbable piece)
    {
        if (assemblyAttach != null && piece == assemblyAttach.piece)
        {
            if (step == 4 | step == 5)
                SetStep(3);
        }
    }

    protected override void OnConfirm()
    {
        //skip feedback instruction if not required
        if (step == 5 & !askUserFeedback)
            step = 7;
        else
            step++;
        SetStep(step);
        confirmButton.SetActive(step == 13);
    }

    protected override void OnContinue(AssemblyCheckpoint nextCP)
    {
        nextCP.target.SetAvailable();
        SetStep(++step);
    }

    public void SetInstruction(int index)
    {
        instructionHolder.gameObject.SetActive(true);
        for (int i = 0; i < instructionHolder.childCount; i++)
        {
            instructionHolder.GetChild(i).gameObject.SetActive((i + 1) == index);
        }
    }
}
