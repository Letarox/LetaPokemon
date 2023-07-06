using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    [SerializeField] List<Sprite> _walkDownSprites; 
    [SerializeField] List<Sprite> _walkUpSprites; 
    [SerializeField] List<Sprite> _walkRightSprites; 
    [SerializeField] List<Sprite> _walkLeftSprites; 
    public float MoveX { get; set; }
    public float MoveY { get; set; }
    public bool IsMoving { get; set; }

    SpriteAnimator _walkDownAnim;
    SpriteAnimator _walkUpAnim;
    SpriteAnimator _walkRightAnim;
    SpriteAnimator _walkLeftAnim;
    SpriteAnimator _currentAnim;

    SpriteRenderer _spriteRenderer;
    bool _wasMoving;

    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer == null)
            Debug.LogError("SpriteRenderer is NULL on " + transform.name);
        _walkDownAnim = new SpriteAnimator(_spriteRenderer, _walkDownSprites);
        _walkUpAnim = new SpriteAnimator(_spriteRenderer, _walkUpSprites);
        _walkRightAnim = new SpriteAnimator(_spriteRenderer, _walkRightSprites);
        _walkLeftAnim = new SpriteAnimator(_spriteRenderer, _walkLeftSprites);
        _currentAnim = _walkDownAnim;
    }

    void Update()
    {
        if (GameManager.gameState == GameState.InBattle)
            return;

        SpriteAnimator prevAnim = _currentAnim;

        if (MoveX == 1)
            _currentAnim = _walkRightAnim;
        else if (MoveX == -1)
            _currentAnim = _walkLeftAnim;
        if (MoveY == 1)
            _currentAnim = _walkUpAnim;
        else if (MoveY == -1)
            _currentAnim = _walkDownAnim;

        if (_currentAnim != prevAnim || IsMoving != _wasMoving)
            _currentAnim.Start();

        if (IsMoving)
            _currentAnim.HandleUpdate();
        else
            _spriteRenderer.sprite = _currentAnim.Frames[0];

        _wasMoving = IsMoving;
    }
}
