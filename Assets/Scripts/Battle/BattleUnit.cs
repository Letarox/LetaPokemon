using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BattleUnit : MonoBehaviour
{
    [SerializeField] bool _isPlayerTeam;
    [SerializeField] BattleHud _hud;
    public bool IsPlayerTeam { get { return _isPlayerTeam; } }
    public BattleHud Hud { get { return _hud; } }

    public Pokemon Pokemon { get; set; }
    SpriteRenderer _spriteRenderer;
    Vector3 _originalPosition;
    Color _originalColor;

    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer == null)
            Debug.LogError("SpriteRenderer is NULL on " + transform.name);        
        _originalColor = GetComponent<SpriteRenderer>().color;
        if (_originalColor == null)
            Debug.LogError("SpriteRenderer.Color is NULL on " + transform.name);
        _originalPosition = _spriteRenderer.transform.localPosition;
    }

    public void Setup(Pokemon pokemon)
    {
        Pokemon = pokemon;
        if (_isPlayerTeam)
            _spriteRenderer.sprite = Pokemon.Base.BackSprite;
        else
            _spriteRenderer.sprite = Pokemon.Base.FrontSprite;

        _hud.SetData(pokemon);
        pokemon.ResetStatBoost();
        _spriteRenderer.color = _originalColor;
        PlayEnterAnimation();
    }

    public void PlayEnterAnimation()
    {
        //when the pokemon enter the battle, checks if its from the player or not and change its position accordingly
        //plays a movement animation using DOLocalMoveX, from the positioon off screen back to its original position. The animation lasts 1 second
        if (_isPlayerTeam)
            _spriteRenderer.transform.localPosition = new Vector3(-510f, _originalPosition.y);
        else
            _spriteRenderer.transform.localPosition = new Vector3(500f, _originalPosition.y);

        _spriteRenderer.transform.DOLocalMoveX(_originalPosition.x, 1f);
    }

    public void PlayAttackAnimation()
    {
        //plays an attack animation using DOLocalMoveX, from its current position and it varies from the player and enemy using it. The animation lasts 0.25 seconds
        //plays the return animation using DOLocalMoveX to its original position. The animation lasts 0.25 seconds
        Sequence sequence = DOTween.Sequence();
        if (_isPlayerTeam)
            sequence.Append(_spriteRenderer.transform.DOLocalMoveX(_originalPosition.x + 50f, 0.25f));
        else
            sequence.Append(_spriteRenderer.transform.DOLocalMoveX(_originalPosition.x - 50f, 0.25f));

        sequence.Append(_spriteRenderer.transform.DOLocalMoveX(_originalPosition.x, 0.25f));
    }

    public void PlayHitAnimation()
    {
        //plays a hit animation that fades its color 3 times. The animation lasts 0.1 seconds
        Sequence sequence = DOTween.Sequence();
        for(int i = 0; i < 3; i++)
        {
            sequence.Append(_spriteRenderer.DOFade(0, 0.1f));
            sequence.Append(_spriteRenderer.DOColor(_originalColor, 0.1f));
        }        
    }

    public void PlayFaintAnimation()
    {
        //plays an attack animation using DOLocalMoveY, from its current position and it varies from the player and enemy using it. The animation lasts 0.5 seconds
        //plays a fade animation combined using Join, so the target fades over the duration. The animation lasts 0.5 seconds
        Sequence sequence = DOTween.Sequence();
        if(_isPlayerTeam)
            sequence.Append(_spriteRenderer.transform.DOLocalMoveY(_originalPosition.y - 250f, 0.8f));
        else
            sequence.Append(_spriteRenderer.transform.DOLocalMoveY(_originalPosition.y - 150f, 0.5f));
        sequence.Join(_spriteRenderer.DOFade(0, 0.5f));
    }

    public void PlayTwoTurnAnimation(bool cast)
    {
        //plays an attack animation using DOLocalMoveY, from its current position and it varies from the player and enemy using it. The animation lasts 0.5 seconds
        //plays a fade animation combined using Join, so the target fades over the duration. The animation lasts 0.5 seconds
        Sequence sequence = DOTween.Sequence();
        if(cast)
            sequence.Append(_spriteRenderer.DOFade(0, 1f));
        else
        {
            sequence.Append(_spriteRenderer.DOFade(1, 0.5f));
            sequence.Join(_spriteRenderer.transform.DOLocalMoveX(_originalPosition.x + 50f, 0.25f));
            sequence.Append(_spriteRenderer.transform.DOLocalMoveX(_originalPosition.x, 0.25f));
        }
    }

    public void HideUnit()
    {
        _spriteRenderer.sortingOrder = -1;
    }

    public void ReAppearUnit()
    {
        _spriteRenderer.sortingOrder = 1;
    }
}
