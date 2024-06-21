
// This script is responsible for managing the game board, 
// including spawning and moving pieces, checking for valid positions, clearing lines,
// and holding tetrominoes.
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class Board : MonoBehaviour
{
    public Tilemap tilemap { get; private set; }
    public Piece activePiece { get; private set; }
    public Vector2Int boardSize { get; private set; } = new Vector2Int(10, 20);
    public TetrominoData[] tetrominoes;

    [SerializeField] Tilemap nextTetrominoTilemap;
    [SerializeField] Tilemap holdTetrominoTilemap;

    [SerializeField] GameObject gameOverPanel;

    [SerializeField] Text lastScoreText;

    private int yOffSet = 4;
    private Vector3Int spawnPosition = new Vector3Int(-1, 8, 0);
    private Queue<TetrominoData> nextTetrominoQueue = new Queue<TetrominoData>();
    private int queueSize = 3;


    private TetrominoData holdTetromino;
    private bool hasHoldTetromino = false;
    private bool canHold = true;

    private RectInt Bounds
    {
        get
        {
            Vector2Int position = new Vector2Int(-boardSize.x / 2, -boardSize.y / 2);
            return new RectInt(position, boardSize);
        }
    }

    private void Awake()
    {
        tilemap = GetComponentInChildren<Tilemap>();
        activePiece = GetComponentInChildren<Piece>();
        for (int i = 0; i < tetrominoes.Length; i++)
        {
            tetrominoes[i].Initialize();
        }
    }

    private void Start()
    {
        InitializeQueue();
        // Wait 2 seconds before spawning the first piece
        SpawnPiece();
    }

    private void InitializeQueue()
    {
        for (int i = 0; i < queueSize; i++)
        {
            TetrominoData newTetromino = GetRandomTetromino();
            newTetromino.Initialize();
            nextTetrominoQueue.Enqueue(newTetromino);
        }
        UpdateNextTetrominoUI();
    }
    private TetrominoData GetRandomTetromino()
    {
        int random = Random.Range(0, tetrominoes.Length);
        return tetrominoes[random];
    }

    public void SpawnPiece()
    {
        TetrominoData data = nextTetrominoQueue.Dequeue();
        activePiece.Initialize(this, spawnPosition, data);

        nextTetrominoQueue.Enqueue(GetRandomTetromino());
        UpdateNextTetrominoUI();

        if (!IsValidPosition(activePiece, activePiece.position))
        {
            GameOver();
        }
        else
        {
            Set(activePiece);
        }
    }

    private void GameOver()
    {
        this.tilemap.ClearAllTiles();
        Time.timeScale = 0f;
        gameOverPanel.SetActive(true);
        lastScoreText.text = "Your score: " + FindObjectOfType<ScoreManager>().score.ToString();
    }

    public void Set(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            tilemap.SetTile(tilePosition, piece.data.tile);
        }

    }

    public void Clear(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            tilemap.SetTile(tilePosition, null);
        }
    }

    public bool IsValidPosition(Piece piece, Vector3Int position)
    {
        RectInt bounds = this.Bounds;

        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + position;
            // Check if the piece is within the bounds of the board
            if (!bounds.Contains((Vector2Int)tilePosition))
            {
                return false;
            }
            // Check if the piece is not colliding with other pieces
            if (tilemap.HasTile(tilePosition))
            {
                return false;
            }
        }
        return true;
    }

    public void ClearLines()
    {
        RectInt bounds = this.Bounds;
        int row = bounds.yMin;
        int lineCleared = 0;

        // We should use while loop for check when the current row is cleared and the above dropped
        // Clear from bottom to top
        while (row < bounds.yMax)
        {
            // Only advance to the next row if the current is not cleared
            // because the tiles above will fall down when a row is cleared
            if (IsLineFull(row))
            {
                LineClear(row);
                lineCleared++;
            }
            else
            {
                row++;
            }
        }
        if (lineCleared > 0)
        {
            // Add score based on the number of lines cleared
            FindObjectOfType<ScoreManager>().AddScoreForLine(lineCleared);
            FindObjectOfType<AudioManager>().PlayLineClearSound();
        }
    }

    private bool IsLineFull(int row)
    {
        RectInt bounds = this.Bounds;
        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int tilePosition = new Vector3Int(col, row, 0);
            if (!tilemap.HasTile(tilePosition))
            {
                return false;
            }
        }
        return true;
    }

    private void LineClear(int row)
    {
        RectInt bounds = this.Bounds;
        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int tilePosition = new Vector3Int(col, row, 0);
            tilemap.SetTile(tilePosition, null);
        }

        while (row < bounds.yMax)
        {
            for (int col = bounds.xMin; col < bounds.xMax; col++)
            {
                Vector3Int position = new Vector3Int(col, row + 1, 0);
                TileBase above = tilemap.GetTile(position);

                position = new Vector3Int(col, row, 0);
                tilemap.SetTile(position, above);
            }
            row++;
        }
    }

    private void UpdateNextTetrominoUI()
    {
        nextTetrominoTilemap.ClearAllTiles();
        TetrominoData[] nextTetrominoArray = nextTetrominoQueue.ToArray();

        for (int i = 0; i < nextTetrominoArray.Length; i++)
        {
            Vector3Int positionOffset = new Vector3Int(0, -i * yOffSet, 0);  // Adjust the multiplier to control spacing
            DisplayTetromino(nextTetrominoTilemap, nextTetrominoArray[i], positionOffset);
        }
    }

    private void DisplayTetromino(Tilemap tilemap, TetrominoData tetrominoData, Vector3Int positionOffset)
    {
        for (int i = 0; i < tetrominoData.cells.Length; i++)
        {
            Vector3Int position = (Vector3Int)tetrominoData.cells[i] + positionOffset;
            tilemap.SetTile(position, tetrominoData.tile);
        }
    }

    // This method is called when the player clicks the Hold button.
    // It will hold the current tetromino and spawn the next one.
    public void HoldTetromino()
    {
        if (!canHold)
        {
            return;
        }
        Clear(activePiece);

        if (!hasHoldTetromino)
        {
            holdTetromino = activePiece.data;
            hasHoldTetromino = true;
            SpawnPiece();
        }
        else
        {
            TetrominoData temp = holdTetromino;
            holdTetromino = activePiece.data;
            activePiece.Initialize(this, spawnPosition, temp);
            Set(activePiece);
        }

        UpdateHoldUI();
        canHold = false;
    }

    private void UpdateHoldUI()
    {
        holdTetrominoTilemap.ClearAllTiles();
        if (hasHoldTetromino)
        {
            DisplayTetromino(holdTetrominoTilemap, holdTetromino, Vector3Int.zero);
        }
    }

    public void ResetHold()
    {
        canHold = true;
    }

}
