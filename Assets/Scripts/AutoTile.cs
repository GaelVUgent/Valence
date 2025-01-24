using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Grid))]
public class AutoTile : MonoBehaviour
{

    private Grid grid;
    public int nCols = 10;

    private void UpdateLayout()
    {
        if (nCols <= 0)
            return;
        if (grid == null)
            grid = GetComponent<Grid>();
        for (int i = 0; i < transform.childCount; i++)
            transform.GetChild(i).localPosition = grid.GetCellCenterLocal(new Vector3Int(i % nCols, i / nCols, 0));
    }

    private void OnEnable()
    {
        UpdateLayout();
    }

    private void OnValidate()
    {
        UpdateLayout();
    }
}
