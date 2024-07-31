using UnityEngine;
using Cysharp.Threading.Tasks;
using TMPro;

public class UIObject : MonoBehaviour
{
    TMP_Text text;

    void Start()
    {
        text = GetComponent<TMP_Text>();
    }

    public async void toggleVisible(bool visible)
    {
        easeing ease = new easeing(2f, visible);

        if (visible)
        {
            while (text.color != new Color(1, 1, 1, 1))
            {
                text.color = Color.Lerp(text.color, new Color(1, 1, 1, 1), ease.value());
                await UniTask.Yield();
            }
            visible = true;
        }
        else
        {
            while (text.color != new Color(1, 1, 1, 0))
            {
                text.color = Color.Lerp(text.color, new Color(1, 1, 1, 0), ease.value());
                await UniTask.Yield();
            }
            visible = false;
        }
    }
}
