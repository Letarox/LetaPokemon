using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    [SerializeField] private float _moveSpeed;
    CharacterAnimator _animator;

    public CharacterAnimator Animator { get => _animator; }
    public bool IsMoving { get; private set; }

    private void Awake()
    {
        _animator = GetComponent<CharacterAnimator>();
        if (_animator == null)
            Debug.LogError("Animator is NULL on " + transform.name);
    }
    public void HandleUpdate()
    {
        _animator.IsMoving = IsMoving;
    }
    public IEnumerator ApplyMovement(Vector2 moveVec, Action OnMoveOver = null)
    {
        //apply proper animation based on received input
        _animator.MoveX = Math.Clamp(moveVec.x, -1f, 1f);
        _animator.MoveY = Math.Clamp(moveVec.y, -1f, 1f);

        //set the position of our movement based on received input
        Vector3 targetPos = transform.position;
        targetPos.x += moveVec.x;
        targetPos.y += moveVec.y;

        if (!IsWalkable(targetPos))
            yield break;
        //turn our bool to true to make sure we can't receive further player input
        IsMoving = true;

        //apply movement to the player until its on the next tile
        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, _moveSpeed * Time.deltaTime);
            yield return null;
        }

        //when the position is close enough, make sure to set the player pos to the next tile
        transform.position = targetPos;

        //turn our bool to false so we can receive input from the player
        IsMoving = false;

        OnMoveOver?.Invoke();
    }
    bool IsWalkable(Vector3 targetPos)
    {
        //check if the next tile we are going to move is from the SolidObjectsLayer
        if (Physics2D.OverlapCircle(targetPos, 0.2f, GameLayers.Instance.SolidObjectsLayer | GameLayers.Instance.InteractableLayer) != null)
            return false;

        return true;
    }
}