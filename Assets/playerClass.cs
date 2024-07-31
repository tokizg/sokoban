using UnityEngine;

public class playerClass : MonoBehaviour
{
    sokoban sokoban;

    void Start()
    {
        sokoban = sokoban.instance;
    }

    void Update()
    {
        // ゲームクリアしている場合は操作できないようにする
        if (sokoban.cancelControll)
            return;

        // 上矢印が押された場合
        if (Input.GetKeyDown(KeyCode.W))
        {
            // プレイヤーが上に移動できるか検証
            sokoban.TryMovePlayer(sokoban.DirectionType.UP);
        }
        // 右矢印が押された場合
        else if (Input.GetKeyDown(KeyCode.D))
        {
            // プレイヤーが右に移動できるか検証
            sokoban.TryMovePlayer(sokoban.DirectionType.RIGHT);
        }
        // 下矢印が押された場合
        else if (Input.GetKeyDown(KeyCode.S))
        {
            // プレイヤーが下に移動できるか検証
            sokoban.TryMovePlayer(sokoban.DirectionType.DOWN);
        }
        // 左矢印が押された場合
        else if (Input.GetKeyDown(KeyCode.A))
        {
            // プレイヤーが左に移動できるか検証
            sokoban.TryMovePlayer(sokoban.DirectionType.LEFT);
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            sokoban.Reset();
        }
    }
}
