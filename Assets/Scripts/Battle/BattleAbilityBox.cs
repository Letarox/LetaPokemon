using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleAbilityBox : MonoBehaviour
{
    [SerializeField] Image _abilityImage;
    [SerializeField] TextMeshProUGUI _abilityText;
    Vector3 _originalPosition;
    Color _defaultColor;

    void Awake()
    {
        _originalPosition = transform.localPosition;
    }
    public void PlayAbilityEnterAnimation(string text)
    {
        //plays an attack animation using DOLocalMoveX, from its current position and it varies from the player and enemy using it. The animation lasts 0.25 seconds
        //plays the return animation using DOLocalMoveX to its original position. The animation lasts 0.25 seconds
        _abilityText.text = text;
        Sequence sequence = DOTween.Sequence();
        sequence.Append(transform.DOLocalMoveX(_originalPosition.x + 220f, 0.5f));        
    }
    public void PlayAbilityExitAnimation()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(_abilityImage.DOFade(0, 0.5f));
        sequence.Join(_abilityText.DOFade(0f, 0.5f));
        sequence.Append(transform.DOLocalMoveX(_originalPosition.x, 0.5f));
        sequence.Append(_abilityImage.DOFade(1, 0.5f));
        sequence.Join(_abilityText.DOFade(1, 0.5f));
    }
}
