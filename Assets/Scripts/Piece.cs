using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR;

public class Piece : MonoBehaviour
{
    public Board board { get; private set; }
    public TetrominoData data { get; private set; }
    public Vector3Int[] cells { get; private set; }
    public Vector3Int position { get; private set; }
    public int rotationIndex { get; private set; }
    public float stepDelay = 1f;
    public float lockDelay = 0.5f;
    public float hardDropThreshold = 500f;
    private float swipeThreshold = 50f;  
    private float stepTime;
    private float lockTime;
    private Vector2 startTouchPosition;
    private Vector2 endTouchPosition;
    private float startTouchTime;
    private float endTouchTime;
    private bool isTouching = false;
    private bool isSoftDropping = false;

    public void Initialize(Board board, Vector3Int position, TetrominoData data)
    {
        this.board = board;
        this.position = position;
        this.data = data;
        this.rotationIndex = 0;
        this.stepTime = Time.time + stepDelay;
        this.lockTime = 0f;

        if (this.cells == null)
        {
            this.cells = new Vector3Int[this.data.cells.Length];
        }

        for (int i = 0; i < data.cells.Length; i++)
        {
            this.cells[i] = (Vector3Int)data.cells[i];
        }
    }

    private void Update()
    {
        board.Clear(this);
        lockTime += Time.deltaTime;

        HandleTouchInput();
        if (isSoftDropping)
        {
            SoftDrop();
        }

        if (Time.time > stepTime)
        {
            Step();
        }

        board.Set(this);
    }

    private void HandleTouchInput()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    startTouchPosition = touch.position;
                    startTouchTime = Time.time;
                    isTouching = true;
                    isSoftDropping = false;
                    break;
                case TouchPhase.Moved:
                    endTouchPosition = touch.position;
                    if (isTouching)
                    {
                        DetectSwipeGesture();
                    }
                    break;
                case TouchPhase.Ended:
                    if (isTouching)
                    {
                        endTouchPosition = touch.position;
                        endTouchTime = Time.time;
                        DetectTapOrQuickSwipe();
                        isTouching = false;
                        isSoftDropping = false;
                    }
                    break;
            }
        }
    }

    private void DetectSwipeGesture()
    {
        Vector2 swipeDelta = endTouchPosition - startTouchPosition;

        if (Mathf.Abs(swipeDelta.x) > swipeThreshold || Mathf.Abs(swipeDelta.y) > swipeThreshold)
        {
            if (Mathf.Abs(swipeDelta.x) > Mathf.Abs(swipeDelta.y))
            {
                if (swipeDelta.x > 0)
                {
                    Move(Vector2Int.right);
                }
                else
                {
                    Move(Vector2Int.left);
                }
            }
            else
            {
                if (swipeDelta.y < 0)
                {
                    isSoftDropping = true; 
                }
                // Optional: Handle up swipe if needed
            }
            startTouchPosition = endTouchPosition;
        }
    }

    private void DetectTapOrQuickSwipe()
    {
        Vector2 tapDelta = endTouchPosition - startTouchPosition;
        float tapDuration = endTouchTime - startTouchTime;
        float tapThresholdDistance = 10f; 
        float tapThresholdTime = 0.2f; 
        Vector2 swipeVector = endTouchPosition - startTouchPosition;
        float swipeSpeed = swipeVector.magnitude / tapDuration;

        if (tapDuration < tapThresholdTime && tapDelta.magnitude < tapThresholdDistance)
        {
            if (endTouchPosition.x > Screen.width / 2)
            {
                Rotate(1); 
            }
            else
            {
                Rotate(-1); 
            }
        }
        else if (swipeVector.y < 0 && swipeSpeed > hardDropThreshold)
        {
            HardDrop(); 
        }
    }

    private void Step()
    {
        stepTime = Time.time + stepDelay;
        Move(Vector2Int.down);

        if (lockTime >= lockDelay)
        {
            Lock();
        }
    }

    private void SoftDrop()
    {
        Move(Vector2Int.down);
        FindAnyObjectByType<ScoreManager>().AddScore(1);
    }

    private void HardDrop()
    {
        int dropDistance = 0;
        isSoftDropping = false;
        while (Move(Vector2Int.down))
        {
            dropDistance++;
        }
        FindAnyObjectByType<ScoreManager>().AddScore(dropDistance*2);
        Lock();
    }

    private void Lock()
    {   
        board.Set(this);
        board.ClearLines();
        board.SpawnPiece();
        board.ResetHold();
    }

    private bool Move(Vector2Int translation)
    {
        Vector3Int newPosition = position + (Vector3Int)translation;

        if (board.IsValidPosition(this, newPosition))
        {
            position = newPosition;
            lockTime = 0f;
            return true;
        }

        return false;
    }

    private void Rotate(int direction)
    {
        int originalRotationIndex = rotationIndex;
        rotationIndex = Wrap(rotationIndex + direction, 0, 4);
        ApplyRotationMatrix(direction);

        if (!TestWallKicks(rotationIndex, direction))
        {
            rotationIndex = originalRotationIndex;
            ApplyRotationMatrix(-direction);
        }
    }

    private void ApplyRotationMatrix(int direction)
    {
        float[] matrix = Data.RotationMatrix;

        for (int i = 0; i < cells.Length; i++)
        {
            Vector3 cell = cells[i];
            int x, y;

            switch (data.tetromino)
            {
                case Tetromino.I:
                case Tetromino.O:
                    cell.x -= 0.5f;
                    cell.y -= 0.5f;
                    x = Mathf.CeilToInt((cell.x * matrix[0] * direction) + (cell.y * matrix[1] * direction));
                    y = Mathf.CeilToInt((cell.x * matrix[2] * direction) + (cell.y * matrix[3] * direction));
                    break;

                default:
                    x = Mathf.RoundToInt((cell.x * matrix[0] * direction) + (cell.y * matrix[1] * direction));
                    y = Mathf.RoundToInt((cell.x * matrix[2] * direction) + (cell.y * matrix[3] * direction));
                    break;
            }

            cells[i] = new Vector3Int(x, y, 0);
        }
    }

    private bool TestWallKicks(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = GetWallKickIndex(rotationIndex, rotationDirection);

        for (int i = 0; i < data.wallKicks.GetLength(1); i++)
        {
            Vector2Int translation = data.wallKicks[wallKickIndex, i];
            if (Move(translation))
            {
                return true;
            }
        }

        return false;
    }

    private int GetWallKickIndex(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = rotationIndex * 2;
        if (rotationDirection < 0)
        {
            wallKickIndex--;
        }

        return Wrap(wallKickIndex, 0, data.wallKicks.GetLength(0));
    }

    private int Wrap(int input, int min, int max)
    {
        if (input < min)
        {
            return max - (min - input) % (max - min);
        }
        else
        {
            return min + (input - min) % (max - min);
        }
        
    }

    public void UpdateSpeed()
    {
        stepDelay = 1f - (FindAnyObjectByType<ScoreManager>().level - 1) * 0.1f;
        lockDelay = 0.5f - (FindAnyObjectByType<ScoreManager>().level - 1) * 0.05f;
    }
}
