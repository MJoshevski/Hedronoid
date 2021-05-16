using Hedronoid;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyEmojis : HNDGameObject
{
    [SerializeField]
    private SpriteRenderer m_emojiRenderer;
    [SerializeField]
    private Sprite m_warriorSprite;
    [SerializeField]
    private Sprite m_mageSprite;
    [SerializeField]
    private Animator emojiAnimator;

    protected override void Awake()
    {
        base.Awake();
    }

    public void ChangeTarget(GameObject target)
    {
        // var aggro = target.GetComponent<Aggro>();
        // if (aggro)
        // {
        //     m_emojiRenderer.sprite = (aggro.Character == CharacterType.WARRIOR) ? m_warriorSprite : m_mageSprite;
        //     if (emojiAnimator)
        //     {
        //         emojiAnimator.SetTrigger("showEmoji");
        //     }
        // }
    }
}
