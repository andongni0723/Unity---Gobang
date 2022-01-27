using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public enum PieceColor
{
    black = 0,
    white = 1,
}

public class Gobang_Piece : MonoBehaviour
{
    public int row;
    public int column;

    public PieceColor pieceColor = PieceColor.black;

    [PunRPC]
    public void SetRowColumnValue(int[] rowColumnValues)
    {
        if (rowColumnValues.Length != 2) return;

        row = rowColumnValues[0];
        column = rowColumnValues[1];
    }
}
