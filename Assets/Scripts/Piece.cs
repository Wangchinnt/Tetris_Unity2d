// This class is responsible for managing the tetromino pieces, 
// including process the logic of their movement, rotation, and locking and
// handling touch input for the game.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Piece : MonoBehaviour
{
    public Board board { get; private set; }
    public TetrominoData data { get; private set; }
    public Vector3Int[] cells { get; private set; }
    public Vector3Int position { get; private set; }
    public int rotationIndex { get; private set; }

    [Header("-----------Movement Controls---------")]
    [SerializeField] float stepDelay = 1f;
    [SerializeField] float lockDelay = 0.5f;

    [Header("--------------Touch Controls------------")]  
    [SerializeField] float swipeThreshold = 40f;
    [SerializeField] float tapThresholdDistance = 10f;
    [SerializeField] float tapThresholdTime = 0.2f;
    [SerializeField] float hardDropThreshold = 500f;
    
    private float stepTime;
    private float lockTime;
    private Vector2 startTouchPosition;
    private Vector2 endTouchPosition;
    private float startTouchTime;
    private float endTouchTime;
    private bool isTouching = false;
    private bool isSoftDropping = false;

    const float yOffset = 20f;


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

        if (Time.timeScale > 0f)
        {
            HandleTouchInput();
            if (isSoftDropping)
            {
                SoftDrop();
            }
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
            // Check if the touch is over a UI element
            if (IsPointerOverUIObject(touch.position))
            {
                return; // Ignore this touch if it's over a UI element
            }
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

     private bool IsPointerOverUIObject(Vector2 touchPosition)
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = touchPosition;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        return results.Count > 0;
    }

    private void DetectSwipeGesture()
    {
        // Distance swiped 
        Vector2 swipeDelta = endTouchPosition - startTouchPosition;
        // if swipe distance is greater than threshold 
        if (Mathf.Abs(swipeDelta.x) > swipeThreshold || Mathf.Abs(swipeDelta.y) > swipeThreshold)
        {
            // if horizontal swipe
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
            }
            startTouchPosition = endTouchPosition;
        }
    }

    private void DetectTapOrQuickSwipe()
    {
        Vector2 tapDelta = endTouchPosition - startTouchPosition;
        float tapDuration = endTouchTime - startTouchTime;
        Vector2 swipeVector = endTouchPosition - startTouchPosition;
        float swipeSpeed = swipeVector.magnitude / tapDuration;

        if (tapDuration < tapThresholdTime && tapDelta.magnitude < tapThresholdDistance)
        {
            if (endTouchPosition.y < Screen.height / 2 + yOffset)
            {
                if (endTouchPosition.x >= Screen.width / 2)
                {
                    Rotate(1);
                }
                else
                {
                    Rotate(-1);
                }
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
        if (Move(Vector2Int.down))
        {
            FindAnyObjectByType<ScoreManager>().AddScore(1);
        }
    }

    private void HardDrop()
    {
        int dropDistance = 0;
        isSoftDropping = false;
        FindAnyObjectByType<AudioManager>().PlayHardDropSound();
        while (Move(Vector2Int.down))
        {
            dropDistance++;
        }
        FindAnyObjectByType<ScoreManager>().AddScore(dropDistance * 2);
        Lock();
    }

    private void Lock()
    {
        board.Set(this);
        FindAnyObjectByType<AudioManager>().PlayLockSound();
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
        FindAnyObjectByType<AudioManager>().PlayRotateSound();

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
