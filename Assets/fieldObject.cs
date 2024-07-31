using UnityEngine;
using Cysharp.Threading.Tasks;

public class fieldObject : MonoBehaviour
{
    public async UniTask translateObject(Vector3 nextPos)
    {
        while (this.transform.position != nextPos)
        {
            this.transform.position = Vector3.MoveTowards(
                this.transform.position,
                nextPos,
                Time.deltaTime * 10f
            );
            await UniTask.Yield();
        }
        return;
    }

    public async UniTask translateObjectEase(Vector3 nextPos, bool easeOut)
    {
        easeing ease = new easeing(5f, easeOut);
        while (this.transform.position != nextPos)
        {
            this.transform.position = Vector3.Lerp(this.transform.position, nextPos, ease.value());
            if (Vector3.Distance(this.transform.position, nextPos) < 0.01f)
            {
                this.transform.position = nextPos;
            }
            await UniTask.Yield();
        }
        return;
    }
}
