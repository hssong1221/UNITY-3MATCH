using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Board
{
    [System.Serializable]
    public struct rowData {
        public bool[] row;    
    }

    public Grid grid;
    public rowData[] rows = new rowData[9];
}
