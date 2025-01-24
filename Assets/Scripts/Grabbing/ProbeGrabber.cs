using System.Collections.Generic;

/// <summary>
/// Special version of a grabber that uses a trigger 
/// collider in stead of a custom distance calculation
/// to find a target piece.
/// </summary>
public class ProbeGrabber : Grabber {

    public int grabThreshold = 2;
    public FingerProbe[] probes;

    protected override Grabbable FindPiece() {
        List<Grabbable> pieces = new List<Grabbable>();
        List<int> pieceCount = new List<int>();
        int maxIndex = -1;
        int maxCount = 0;
        foreach(FingerProbe p in probes) {
            if(p.piece != null) {
                int i = pieces.IndexOf(p.piece);
                if(i < 0) {
                    pieces.Add(p.piece);
                    pieceCount.Add(1);
                    if(maxCount == 0) {
                        maxIndex = pieceCount.Count - 1;
                        maxCount = 1;
                    }
                }
                else {
                    if(++pieceCount[i] > maxCount) {
                        maxIndex = i;
                        maxCount = pieceCount[i];
                    }
                }
            }
        }

        if(maxCount >= grabThreshold)
            return pieces[maxIndex];
        return null;
    }

}
