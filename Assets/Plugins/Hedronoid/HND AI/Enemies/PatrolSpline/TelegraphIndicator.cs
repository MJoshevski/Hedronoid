using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Hedronoid;

/// <summary>
/// 
/// </summary>
public class TelegraphIndicator : HNDGameObject
{
    [SerializeField]
    SpriteRenderer m_Sprite;
    protected override void Start()
    {
        base.Start();
        if (!m_Sprite)
            m_Sprite.GetComponent<SpriteRenderer>();
    }

    public void FadeAndDestroy(float delay)
    {
        StartCoroutine(FadeDestroy(delay));
    }

    IEnumerator FadeDestroy(float delay)
    {
        yield return new WaitForSeconds(delay);
        while(m_Sprite.color.a >= 0.001f)
        {
            Color aux = m_Sprite.color;
            aux.a -= 0.05f;
            m_Sprite.color = aux;
            yield return null;
        }
        yield return null;
        Destroy(gameObject);
    }
}
