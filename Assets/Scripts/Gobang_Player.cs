using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System.Linq;

public enum PlayerState
{
    NotReady = 0,
    Ready = 1
}

public enum DirectionPiece
{
    Up = 0,
    Down = 1,
    Left = 2,
    Right = 3,
    LeftTop = 4,
    RightDown = 5,
    LeftDown = 6,
    RightTop = 7,
}

public class Gobang_Player : MonoBehaviour
{
    [Header("Game Setting")]
    public Vector3 zeroPosition;
    public float cellWidth;
    public PieceColor pieceColor = PieceColor.black;

    public GameObject blackPiece;
    public GameObject whitePiece;

    public List<Gobang_Piece> currentPieceList = new List<Gobang_Piece>();

    public PlayerState playerState = PlayerState.NotReady;

    PhotonView pv;

    int row;
    int column;

    void Start()
    {
        pv = GetComponent<PhotonView>();

        Gobang_NetworkManager.instance.SetName();

        if (pv.IsMine)
        {
            Gobang_NetworkManager.instance.SetSelfText(pieceColor);
        }
        else
        {
            Gobang_NetworkManager.instance.SetEnemyText(pieceColor);
        }

        
    }

    void Update()
    {
        if (Gobang_NetworkManager.instance.playerTurn != pieceColor) return;
        if (Gobang_NetworkManager.instance.gameState != GameState.Ready) return;

        var players = FindObjectsOfType<Gobang_Player>();
        foreach (var item in players)
        {
            if (item.playerState != PlayerState.Ready) return;
        }

        Gobang_NetworkManager.instance.waiting_Obj.SetActive(false);
        Gobang_NetworkManager.instance.isGaming = true;

        if (!pv.IsMine) return;

        // Show the Turn on UI
        pv.RPC("ToogleTurn", RpcTarget.All);

        // Mouse Botton Click
        if (Input.GetMouseButtonDown(0))
        {
            // Get the position And Cal the row and column (Int)
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 offsetPos = mousePos - zeroPosition;
            row = (int)Mathf.Round(offsetPos.y / cellWidth); 
            column = (int)Mathf.Round(offsetPos.x / cellWidth);

            int[] rowColumnValues = { row, column };

            // If piece is Out of the BG ?
            if (row < 0 || row > 14 || column < 0 || column > 14) return;

            // If the piece position is have the other piece
            currentPieceList = GameObject.FindObjectsOfType<Gobang_Piece>().ToList();

            foreach (var item in currentPieceList)
            {
                if (item.row == row && item.column == column)
                {
                    return;
                }
            }

            // Set the piece position
            Vector3 piecePos = new Vector3(column * cellWidth, row * cellWidth, 0) + zeroPosition;

            // Inst a network piece
            GameObject newPiece;
            Gobang_Piece currentPiece = new Gobang_Piece();

            if(pieceColor == PieceColor.black)
            {
                newPiece = PhotonNetwork.Instantiate(blackPiece.name, piecePos, Quaternion.identity);

                // update the data
                newPiece.GetComponent<PhotonView>().RPC("SetRowColumnValue", RpcTarget.All, rowColumnValues);
                currentPiece = newPiece.GetComponent<Gobang_Piece>();
            }     
            else
            {
                newPiece = PhotonNetwork.Instantiate(whitePiece.name, piecePos, Quaternion.identity);

                // update the data
                newPiece.GetComponent<PhotonView>().RPC("SetRowColumnValue", RpcTarget.All, rowColumnValues);
                currentPiece = newPiece.GetComponent<Gobang_Piece>();
            }

            // Play the marking sound
            Gobang_NetworkManager.instance.PlayMarkingSFX();

            //if have a line of five pieces
            currentPieceList = FindObjectsOfType<Gobang_Piece>().ToList();
            bool isFive = JudgeFivePiece(currentPieceList, currentPiece);

            if (isFive)
            {
                // Game Over
                Gobang_NetworkManager.instance.gameObject.GetComponent<PhotonView>().RPC("GameOver", RpcTarget.All, pieceColor);

            }

            // Change turn
            Gobang_NetworkManager.instance.gameObject.GetComponent<PhotonView>().RPC("ChangeTurn", RpcTarget.All);
            
        }
    }

    bool JudgeFivePiece(List<Gobang_Piece> currentList, Gobang_Piece currentPiece)
    {
        bool result = false;

        List<Gobang_Piece> currentColorList = currentList.Where(en => en.pieceColor == pieceColor).ToList();

        var upList = GetSamePieceByDirection(currentColorList, currentPiece, DirectionPiece.Up);
        var downList = GetSamePieceByDirection(currentColorList, currentPiece, DirectionPiece.Down);
        var leftList = GetSamePieceByDirection(currentColorList, currentPiece, DirectionPiece.Left);
        var rightList = GetSamePieceByDirection(currentColorList, currentPiece, DirectionPiece.Right);
        var leftTopList = GetSamePieceByDirection(currentColorList, currentPiece, DirectionPiece.LeftTop);
        var rightDownList = GetSamePieceByDirection(currentColorList, currentPiece, DirectionPiece.RightDown);
        var leftDownList = GetSamePieceByDirection(currentColorList, currentPiece, DirectionPiece.LeftDown);
        var RightTopList = GetSamePieceByDirection(currentColorList, currentPiece, DirectionPiece.RightTop);

        if (upList.Count + downList.Count + 1 >= 5 || 
            leftList.Count + rightList.Count + 1 >= 5 || 
            leftTopList.Count + rightDownList.Count + 1 >= 5 || 
            leftDownList.Count + RightTopList.Count + 1 >= 5)
        {
            // Game Over
            result = true;
        }

        return result;
    }

    List<Gobang_Piece> GetSamePieceByDirection(List<Gobang_Piece> currentColorList, Gobang_Piece currentPiece, DirectionPiece direction)
    {
        List<Gobang_Piece> result = new List<Gobang_Piece>();

        switch (direction)
        {
            case DirectionPiece.Up:
                foreach (var item in currentColorList)
                {
                    if (item.row == currentPiece.row + 1 && item.column == currentPiece.column)
                    {
                        result.Add(item);
                        var resultList = GetSamePieceByDirection(currentColorList, item, DirectionPiece.Up);
                        result.AddRange(resultList);
                    }
                }
                break;
            case DirectionPiece.Down:
                foreach (var item in currentColorList)
                {
                    if (item.row == currentPiece.row - 1 && item.column == currentPiece.column)
                    {
                        result.Add(item);
                        var resultList = GetSamePieceByDirection(currentColorList, item, DirectionPiece.Down);
                        result.AddRange(resultList);
                    }
                }
                break;
            case DirectionPiece.Left:
                foreach (var item in currentColorList)
                {
                    if (item.row == currentPiece.row && item.column == currentPiece.column - 1)
                    {
                        result.Add(item);
                        var resultList = GetSamePieceByDirection(currentColorList, item, DirectionPiece.Left);
                        result.AddRange(resultList);
                    }
                }
                break;
            case DirectionPiece.Right:
                foreach (var item in currentColorList)
                {
                    if (item.row == currentPiece.row && item.column == currentPiece.column + 1)
                    {
                        result.Add(item);
                        var resultList = GetSamePieceByDirection(currentColorList, item, DirectionPiece.Right);
                        result.AddRange(resultList);
                    }
                }
                break;
            case DirectionPiece.LeftTop:
                foreach (var item in currentColorList)
                {
                    if (item.row == currentPiece.row + 1 && item.column == currentPiece.column - 1)
                    {
                        result.Add(item);
                        var resultList = GetSamePieceByDirection(currentColorList, item, DirectionPiece.LeftTop);
                        result.AddRange(resultList);
                    }
                }
                break;
            case DirectionPiece.RightDown:
                foreach (var item in currentColorList)
                {
                    if (item.row == currentPiece.row - 1 && item.column == currentPiece.column + 1)
                    {
                        result.Add(item);
                        var resultList = GetSamePieceByDirection(currentColorList, item, DirectionPiece.RightDown);
                        result.AddRange(resultList);
                    }
                }
                break;
            case DirectionPiece.LeftDown:
                foreach (var item in currentColorList)
                {
                    if (item.row == currentPiece.row - 1 && item.column == currentPiece.column - 1)
                    {
                        result.Add(item);
                        var resultList = GetSamePieceByDirection(currentColorList, item, DirectionPiece.LeftDown);
                        result.AddRange(resultList);
                    }
                }
                break;
            case DirectionPiece.RightTop:
                foreach (var item in currentColorList)
                {
                    if (item.row == currentPiece.row + 1 && item.column == currentPiece.column + 1)
                    {
                        result.Add(item);
                        var resultList = GetSamePieceByDirection(currentColorList, item, DirectionPiece.RightTop);
                        result.AddRange(resultList);
                    }
                }
                break;
        }

        return result;
    }

    [PunRPC]
    public void SetPieceColor(PieceColor pieceCo)
    {
        pieceColor = pieceCo;
    }

    [PunRPC]
    public void SetPlayerReady()
    {
        playerState = PlayerState.Ready;

        if (pv.IsMine)
        {
            Gobang_NetworkManager.instance.selfReady_T.text = "已準備";
        }
        else
        {
            Gobang_NetworkManager.instance.enemyReady_T.text = "已準備";

        }
    }

    [PunRPC]
    public void ToogleTurn()
    {
        if (pv.IsMine)
        {
            if (Gobang_NetworkManager.instance.playerTurn == pieceColor)
            {
                Gobang_NetworkManager.instance.turn_TG_List[0].isOn = true;
            }
            else
            {
                Gobang_NetworkManager.instance.turn_TG_List[1].isOn = true;
            }
        }
        else
        {
            if (Gobang_NetworkManager.instance.playerTurn == pieceColor)
            {
                Gobang_NetworkManager.instance.turn_TG_List[1].isOn = true;
            }
            else
            {
                Gobang_NetworkManager.instance.turn_TG_List[0].isOn = true;
            }
        }
    }
}