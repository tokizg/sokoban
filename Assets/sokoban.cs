using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using System.Linq;

public class sokoban : MonoBehaviour
{
    public static sokoban instance;

    enum TileType
    {
        NONE,
        GROUND,

        // 何も無い

        // 地面
        TARGET, // 目的地
        PLAYER, // プレイヤー
        BLOCK, // ブロック

        PLAYER_ON_TARGET, // プレイヤー（目的地の上）
        BLOCK_ON_TARGET, // ブロック（目的地の上）
    }

    [SerializeField]
    TextAsset stageFile; // ステージ構造が記述されたテキストファイル

    int rows = 10; // 行数
    int columns = 10; // 列数
    TileType[,] tileList; // タイル情報を管理する二次元配列

    [SerializeField]
    float tileSize; // タイルのサイズ

    [SerializeField]
    GameObject groundObject;

    [SerializeField]
    GameObject targetObject;

    [SerializeField]
    GameObject playerObject;

    [SerializeField]
    GameObject blockObject;

    [SerializeField]
    GameObject clearText;

    GameObject player; // プレイヤーのゲームオブジェクト
    Vector2 middleOffset; // 中心位置

    [SerializeField]
    int blockCount; // ブロックの数

    public bool cancelControll; // ゲームをクリアした場合 true

    easeing ease;

    // 方向の種類
    public enum DirectionType
    {
        UP, // 上
        RIGHT, // 右
        DOWN, // 下
        LEFT, // 左
    }

    // 各位置に存在するゲームオブジェクトを管理する連想配列
    Dictionary<GameObject, Vector2Int> gameObjectPosTable =
        new Dictionary<GameObject, Vector2Int>();
    List<GameObject> grounds = new List<GameObject>();

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        initializeGame();
    }

    // タイルの情報を読み込む
    void LoadTileData()
    {
        // タイルの情報を一行ごとに分割
        var lines = stageFile.text.Split(
            new[] { '\r', '\n' },
            System.StringSplitOptions.RemoveEmptyEntries
        );

        // タイルの列数を計算
        var nums = lines[0].Split(new[] { ',' });

        // タイルの列数と行数を保持
        rows = lines.Length; // 行数
        columns = nums.Length; // 列数

        // タイル情報を int 型の２次元配列で保持
        tileList = new TileType[columns, rows];
        for (int y = 0; y < rows; y++)
        {
            // 一文字ずつ取得
            var st = lines[y];
            nums = st.Split(new[] { ',' });
            for (int x = 0; x < columns; x++)
            {
                // 読み込んだ文字を数値に変換して保持
                tileList[x, y] = (TileType)int.Parse(nums[x]);
            }
        }
    }

    // 指定された行番号と列番号からスプライトの表示位置を計算して返す
    Vector3 GetDisplayPosition(int x, int y)
    {
        return new Vector3(x * tileSize - middleOffset.x, 0, y * -tileSize + middleOffset.y);
    }

    // ステージを作成
    async UniTask CreateStage()
    {
        List<UniTask> uniTasks = new List<UniTask>();

        // ステージの中心位置を計算
        middleOffset.x = columns * tileSize * 0.5f - tileSize * 0.5f;
        middleOffset.y = rows * tileSize * 0.5f - tileSize * 0.5f;
        ;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                var val = tileList[x, y];

                if (val == TileType.NONE)
                    continue;

                var name = "tile" + y + "_" + x;
                GameObject tile = Instantiate(groundObject);
                grounds.Add(tile);
                tile.transform.position = GetDisplayPosition(x, y) + Vector3.up * 20f;
                tile.name = name;

                // 何も無い場所は無視
                if (val == TileType.NONE)
                    continue;

                switch (val)
                {
                    case TileType.TARGET:
                        tile = Instantiate(targetObject);
                        break;
                    case TileType.PLAYER:
                        tile = Instantiate(playerObject);
                        player = tile;
                        break;
                    case TileType.BLOCK:
                        blockCount++;
                        tile = Instantiate(blockObject);
                        break;
                    case TileType.GROUND:
                        continue;
                    default:
                        break;
                }
                tile.transform.position = GetDisplayPosition(x, y) + Vector3.up * 20f;

                // ブロックを連想配列に追加
                gameObjectPosTable.Add(tile, new Vector2Int(x, y));
            }
        }

        foreach (var ground in grounds)
        {
            uniTasks.Add(
                ground
                    .GetComponent<fieldObject>()
                    .translateObjectEase(ground.transform.position - Vector3.up * 20f, false)
            );
            await UniTask.Delay(10);
        }
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                var position = new Vector2Int(x, y);
                var obj = GetGameObjectAtPosition(position);
                if (obj != null)
                {
                    uniTasks.Add(
                        obj.GetComponent<fieldObject>()
                            .translateObjectEase(obj.transform.position - Vector3.up * 20f, false)
                    );
                }
                await UniTask.Delay(10);
            }
        }

        await UniTask.WhenAll(uniTasks);
    }

    // 指定された位置に存在するゲームオブジェクトを返します
    GameObject GetGameObjectAtPosition(Vector2Int pos)
    {
        foreach (var pair in gameObjectPosTable)
        {
            // 指定された位置が見つかった場合
            if (pair.Value == pos)
            {
                // その位置に存在するゲームオブジェクトを返す
                return pair.Key;
            }
        }
        return null;
    }

    // 指定された位置がステージ内なら true を返す
    bool IsValidPosition(Vector2Int pos)
    {
        if (0 <= pos.x && pos.x < columns && 0 <= pos.y && pos.y < rows)
        {
            return tileList[pos.x, pos.y] != TileType.NONE;
        }
        return false;
    }

    // 指定された位置のタイルがブロックなら true を返す
    bool IsBlock(Vector2Int pos)
    {
        var cell = tileList[pos.x, pos.y];
        return cell == TileType.BLOCK || cell == TileType.BLOCK_ON_TARGET;
    }

    // 毎フレーム呼び出される


    // 指定された方向にプレイヤーが移動できるか検証
    // 移動できる場合は移動する
    async public void TryMovePlayer(DirectionType direction)
    {
        // プレイヤーの現在地を取得
        var currentPlayerPos = gameObjectPosTable[player];

        // プレイヤーの移動先の位置を計算
        var nextPlayerPos = GetNextPositionAlong(currentPlayerPos, direction);

        // プレイヤーの移動先がステージ内ではない場合は無視
        if (!IsValidPosition(nextPlayerPos))
            return;

        cancelControll = true;

        List<UniTask> taskList = new List<UniTask>();
        // プレイヤーの移動先にブロックが存在する場合
        if (IsBlock(nextPlayerPos))
        {
            // ブロックの移動先の位置を計算
            var nextBlockPos = GetNextPositionAlong(nextPlayerPos, direction);

            // ブロックの移動先がステージ内の場合かつ
            // ブロックの移動先にブロックが存在しない場合
            if (IsValidPosition(nextBlockPos) && !IsBlock(nextBlockPos))
            {
                // 移動するブロックを取得
                var block = GetGameObjectAtPosition(nextPlayerPos);

                // プレイヤーの移動先のタイルの情報を更新
                UpdateGameObjectPosition(nextPlayerPos);

                // ブロックを移動
                taskList.Add(
                    block
                        .GetComponent<fieldObject>()
                        .translateObject(GetDisplayPosition(nextBlockPos.x, nextBlockPos.y))
                );

                // ブロックの位置を更新
                gameObjectPosTable[block] = nextBlockPos;

                // ブロックの移動先の番号を更新
                if (tileList[nextBlockPos.x, nextBlockPos.y] == TileType.GROUND)
                {
                    // 移動先が地面ならブロックの番号に更新
                    tileList[nextBlockPos.x, nextBlockPos.y] = TileType.BLOCK;
                }
                else if (tileList[nextBlockPos.x, nextBlockPos.y] == TileType.TARGET)
                {
                    // 移動先が目的地ならブロック（目的地の上）の番号に更新
                    tileList[nextBlockPos.x, nextBlockPos.y] = TileType.BLOCK_ON_TARGET;
                }

                // プレイヤーの現在地のタイルの情報を更新
                UpdateGameObjectPosition(currentPlayerPos);

                // プレイヤーを移動
                taskList.Add(
                    player
                        .GetComponent<fieldObject>()
                        .translateObject(GetDisplayPosition(nextPlayerPos.x, nextPlayerPos.y))
                );

                // プレイヤーの位置を更新
                gameObjectPosTable[player] = nextPlayerPos;

                // プレイヤーの移動先の番号を更新
                if (tileList[nextPlayerPos.x, nextPlayerPos.y] == TileType.GROUND)
                {
                    // 移動先が地面ならプレイヤーの番号に更新
                    tileList[nextPlayerPos.x, nextPlayerPos.y] = TileType.PLAYER;
                }
                else if (tileList[nextPlayerPos.x, nextPlayerPos.y] == TileType.TARGET)
                {
                    // 移動先が目的地ならプレイヤー（目的地の上）の番号に更新
                    tileList[nextPlayerPos.x, nextPlayerPos.y] = TileType.PLAYER_ON_TARGET;
                }
            }
        }
        // プレイヤーの移動先にブロックが存在しない場合
        else
        {
            // プレイヤーの現在地のタイルの情報を更新
            UpdateGameObjectPosition(currentPlayerPos);

            // プレイヤーを移動
            taskList.Add(
                player
                    .GetComponent<fieldObject>()
                    .translateObject(GetDisplayPosition(nextPlayerPos.x, nextPlayerPos.y))
            );
            // プレイヤーの位置を更新
            gameObjectPosTable[player] = nextPlayerPos;

            // プレイヤーの移動先の番号を更新
            if (tileList[nextPlayerPos.x, nextPlayerPos.y] == TileType.GROUND)
            {
                // 移動先が地面ならプレイヤーの番号に更新
                tileList[nextPlayerPos.x, nextPlayerPos.y] = TileType.PLAYER;
            }
            else if (tileList[nextPlayerPos.x, nextPlayerPos.y] == TileType.TARGET)
            {
                // 移動先が目的地ならプレイヤー（目的地の上）の番号に更新
                tileList[nextPlayerPos.x, nextPlayerPos.y] = TileType.PLAYER_ON_TARGET;
            }
        }
        await UniTask.WhenAll(taskList);
        cancelControll = false;
    }

    // 指定された方向の位置を返す
    Vector2Int GetNextPositionAlong(Vector2Int pos, DirectionType direction)
    {
        switch (direction)
        {
            // 上
            case DirectionType.UP:
                pos.y -= 1;
                break;

            // 右
            case DirectionType.RIGHT:
                pos.x += 1;
                break;

            // 下
            case DirectionType.DOWN:
                pos.y += 1;
                break;

            // 左
            case DirectionType.LEFT:
                pos.x -= 1;
                break;
        }
        return pos;
    }

    // 指定された位置のタイルを更新
    void UpdateGameObjectPosition(Vector2Int pos)
    {
        // 指定された位置のタイルの番号を取得
        var cell = tileList[pos.x, pos.y];

        // プレイヤーもしくはブロックの場合
        if (cell == TileType.PLAYER || cell == TileType.BLOCK)
        {
            // 地面に変更
            tileList[pos.x, pos.y] = TileType.GROUND;
        }
        // 目的地に乗っているプレイヤーもしくはブロックの場合
        else if (cell == TileType.PLAYER_ON_TARGET || cell == TileType.BLOCK_ON_TARGET)
        {
            // 目的地に変更
            tileList[pos.x, pos.y] = TileType.TARGET;
        }

        // ゲームをクリアしたかどうか確認
        CheckCompletion();
    }

    // ゲームをクリアしたかどうか確認
    void CheckCompletion()
    {
        // 目的地に乗っているブロックの数を計算
        int blockOnTargetCount = 0;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                if (tileList[x, y] == TileType.BLOCK_ON_TARGET)
                {
                    blockOnTargetCount++;
                }
            }
        }

        // すべてのブロックが目的地の上に乗っている場合
        if (blockOnTargetCount == blockCount)
        {
            cancelControll = true;

            // ゲームクリア
            clearText.SendMessage("toggleVisible", true);
        }
    }

    async void initializeGame()
    {
        clearText.SendMessage("toggleVisible", false);

        cancelControll = true; // ゲームクリアフラグを true に設定
        blockCount = 0; // ブロックの数をリセット
        grounds.Clear(); // リストをクリア
        gameObjectPosTable.Clear(); // 連想配列をクリア
        LoadTileData(); // タイルの情報を読み込む
        await CreateStage(); // ステージを作成
        cancelControll = false; // ゲームクリアフラグを false に設定
    }

    // ゲーム開始時に呼び出される
    public async void Reset()
    {
        this.cancelControll = true;
        List<UniTask> uniTasks = new List<UniTask>();

        foreach (var ground in grounds)
        {
            uniTasks.Add(
                ground
                    .GetComponent<fieldObject>()
                    .translateObjectEase(ground.transform.position - Vector3.up * 20f, false)
            );
            await UniTask.Delay(10);
        }
        foreach (var obj in gameObjectPosTable)
        {
            uniTasks.Add(
                obj.Key
                    .GetComponent<fieldObject>()
                    .translateObjectEase(obj.Key.transform.position - Vector3.up * 20f, false)
            );
            await UniTask.Delay(10);
        }

        await UniTask.WhenAll(uniTasks);

        foreach (var pair in gameObjectPosTable)
        {
            Destroy(pair.Key); // ゲームオブジェクトを削除
        }
        foreach (var ground in grounds)
        {
            Destroy(ground);
        }
        initializeGame();
        return;
    }
}
