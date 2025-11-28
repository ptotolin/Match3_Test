using System;
using UnityEngine;

public class GemInputHandler : MonoBehaviour
{
    public event Action<Vector2Int, Vector2Int> EventSwipeDetected; 
    
    // dependencies
    //private SC_GameLogic scGameLogic;
    private GameBoard gameboard;
    
    // locals
    private Vector2 firstTouchPosition;
    private Vector2 finalTouchPosition;
    private bool mousePressed;
    private float swipeAngle = 0;
    private bool initialized;
    private Camera mainCamera;
    private SC_Gem firstTouchGem;
    

    public void Initialize(GameBoard gameboard)
    {
        this.gameboard = gameboard;
        
        mainCamera = Camera.main;
        initialized = true;

    }

    private void Update()
    {
        if (!initialized) {
            return;
        }

        if (!mousePressed && Input.GetMouseButtonDown(0)) {
            mousePressed = true;
            firstTouchPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            firstTouchGem = GetGemAtPosition(firstTouchPosition);
        }
        else
        if (mousePressed && Input.GetMouseButtonUp(0)) {
            mousePressed = false;
            if (firstTouchGem != null) {
                finalTouchPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                swipeAngle = CalculateAngle();
                
                if (Vector3.Distance(firstTouchPosition, finalTouchPosition) > 0.5f)
                    FireSwipeEventIfNeeded();
            }
        }
    }

    private SC_Gem GetGemAtPosition(Vector2 position)
    {
        var hit = Physics2D.Raycast(position, Vector2.zero);
            
        if (hit.collider != null)
        {
            var gem = hit.collider.GetComponent<SC_Gem>();
            if (gem != null)
            {
                return gem;
            }
        }

        return null;
    }
    
    private float CalculateAngle()
    {
        var angle = Mathf.Atan2(finalTouchPosition.y - firstTouchPosition.y, finalTouchPosition.x - firstTouchPosition.x);
        angle *= 180.0f / Mathf.PI;

        return angle;
    }
    
    private void FireSwipeEventIfNeeded()
    {
        if (!gameboard.TryGetGemPos(firstTouchGem, out var firstGemPos)) {
            return;
        }

        if (swipeAngle is < 45 and > -45 && firstGemPos.x < SC_GameVariables.Instance.rowsSize - 1)
        {
            EventSwipeDetected?.Invoke(firstGemPos, new Vector2Int(firstGemPos.x + 1, firstGemPos.y));

        }
        else if (swipeAngle is > 45 and <= 135 && firstGemPos.y < SC_GameVariables.Instance.colsSize - 1)
        {
            EventSwipeDetected?.Invoke(firstGemPos, new Vector2Int(firstGemPos.x, firstGemPos.y + 1));
        }
        else if (swipeAngle is < -45 and >= -135 && firstGemPos.y > 0)
        {
            EventSwipeDetected?.Invoke(firstGemPos, new Vector2Int(firstGemPos.x, firstGemPos.y - 1));
        }
        else if (swipeAngle > 135 || swipeAngle < -135 && firstGemPos.x > 0)
        {
            EventSwipeDetected?.Invoke(firstGemPos, new Vector2Int(firstGemPos.x - 1, firstGemPos.y));
        }

        // scGameLogic.SetGem(posIndex.x,posIndex.y, this);
        // scGameLogic.SetGem(otherGem.posIndex.x, otherGem.posIndex.y, otherGem);
        //
        // StartCoroutine(CheckMoveCo());
    }
}