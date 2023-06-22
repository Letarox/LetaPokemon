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
        _originalPosition = _spriteRenderer.transform.localPosition;
        _originalColor = GetComponent<SpriteRenderer>().color;
    }

    public void Setup(Pokemon pokemon)
    {
        Pokemon = pokemon;
        if (_isPlayerTeam)
            _spriteRenderer.sprite = Pokemon.Base.BackSprite;
        else
            _spriteRenderer.sprite = Pokemon.Base.FrontSprite;

        _hud.SetData(pokemon);

        _spriteRenderer.color = _originalColor;
        PlayEnterAnimation();
    }

    public void PlayEnterAnimation()
    {
        if (_isPlayerTeam)
            _spriteRenderer.transform.localPosition = new Vector3(-510f, _originalPosition.y);
        else
            _spriteRenderer.transform.localPosition = new Vector3(500f, _originalPosition.y);

        _spriteRenderer.transform.DOLocalMoveX(_originalPosition.x, 1f);
    }

    public void PlayAttackAnimation()
    {
        Sequence sequence = DOTween.Sequence();

        if (_isPlayerTeam)
            sequence.Append(_spriteRenderer.transform.DOLocalMoveX(_originalPosition.x + 50f, 0.25f));
        else
            sequence.Append(_spriteRenderer.transform.DOLocalMoveX(_originalPosition.x - 50f, 0.25f));

        sequence.Append(_spriteRenderer.transform.DOLocalMoveX(_originalPosition.x, 0.25f));
    }

    public void PlayHitAnimation()
    {
        Sequence sequence = DOTween.Sequence();

        for(int i = 0; i < 3; i++)
        {
            sequence.Append(_spriteRenderer.DOFade(0, 0.1f));
            sequence.Append(_spriteRenderer.DOColor(_originalColor, 0.1f));
        }        
    }

    public void PlayFaintAnimation()
    {
        Sequence sequence = DOTween.Sequence();

        if(_isPlayerTeam)
            sequence.Append(_spriteRenderer.transform.DOLocalMoveY(_originalPosition.y - 220f, 0.75f));
        else
            sequence.Append(_spriteRenderer.transform.DOLocalMoveY(_originalPosition.y - 150f, 0.5f));
        sequence.Join(_spriteRenderer.DOFade(0, 0.5f));
    }

    public void HideUnit()
    {
        _spriteRenderer.sortingOrder = 0;
    }

    public void ReAppearUnit()
    {
        _spriteRenderer.sortingOrder = 1;
    }
}
