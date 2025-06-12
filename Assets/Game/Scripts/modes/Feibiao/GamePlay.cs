using Assets.Game.Scripts;
using Assets.Game.Scripts.modes.Feibiao;
using Assets.Scripts.common;
using Assets.Scripts.Events;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using static Assets.Game.Scripts.modes.Feibiao.PaintBoard;


public class GamePlay : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private GameUILayer gameUILayer;
    [SerializeField] private TargetMgr targetMgr;
    [SerializeField] private EffectLayer effectLayer;
    [SerializeField] private GameObject objPrefab;
    [SerializeField] public Transform levelRoot;
    [SerializeField] private GameObject blockLayer;
    [SerializeField] private Transform boardBaffle;
    [SerializeField] private PaintBoard paintBoard;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private GameObject bossFlag;
    [HideInInspector]
    public Transform boardLayer;//脚本 Feibiao中赋值

    [HideInInspector]
    public int deafultHoleNum;
    [HideInInspector]
    public int needHoleNum;

    private List<Board> boards = new List<Board>();

    private static readonly string[] COLORS = new string[]
    {
        "#6ccae2", "#95ed85", "#206abc", "#ef8bfb",
        "#d7c55e", "#7043bd", "#ff6e8d"
    };

    // Start is called before the first frame update
    void Awake()
    {
        Physics2D.gravity = new Vector2(0, -500f);

        EventMng.addEventListener(EventTypes.BTN_REMOVE_BOARD, ShowBlockLayer);

        gameUILayer.Init(this);
        //blockLayer.GetComponent<Button>().onClick.AddListener(OnBlockLayerTouchStart);
        //GetComponent<Button>().onClick.AddListener();

        boardBaffle.gameObject.AddComponent<Collider2DListener>().addListener(OnBeginContact);
    }

    private void OnDestroy()
    {
        EventMng.removeEventListener(EventTypes.BTN_REMOVE_BOARD, ShowBlockLayer);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (GameManager.Instance.isGuide) return;
        OnTouchStart();
    }

    public void OnAutoPlay()
    {
        if (IsInvoking(nameof(StartAutoPlay)))
        {
            CancelInvoke(nameof(StartAutoPlay));
            UIManager.Instance.ShowMsg("Auto Mode disabled");
        }
        else
        {
            UIManager.Instance.ShowMsg("Auto Mode enabled");
            InvokeRepeating(nameof(StartAutoPlay), 0f, 0.3f);
        }
    }

    private void StartAutoPlay()
    {
        Debug.Log("startAutoPlay");
        for (int index = boards.Count - 1; index >= 0; index--)
        {
            var board = boards[index];
            if (board.isLocked) continue;

            var screws = board.GetScrewComps();
            for (int k = 0; k < screws.Count; k++)
            {
                var comp = screws[k];
                if (!comp.isLocked && IsTouchenEnabled(board, comp))
                {
                    TouchOperateObj(comp);
                    return;
                }
            }
        }
    }

    public async Task InitGame(GridDataWrapper paintData, int level)
    {
        Debug.Log("gamePlay");
        InitPaintBoard(paintData, level);
        CheckBoards();
        await Task.WhenAny(InitBoards());
        List<Board> data = boards;
        RefreshBoardLockState();
        InitLevelConfig(level);
    }

    private void ShowBlockLayer(EventStruct evt)
    {
        blockLayer.SetActive(true);
        UIManager.Instance.ShowMsg("Please select a panel to smash");
    }
    private List<List<int>> GetColorConfig(int level)
    {
        var curLevelConfig = LevelMgr.GetLevelDifficultyConfig(level);
        Debug.Log($"curLevelConfig: {curLevelConfig}");

        var steps = paintBoard.GetOperateSteps();
        var colors = steps.Select(step => step.colorIndex).ToList();
        colors = ShuffleList(colors);

        int total = colors.Count;
        var colorArr = new List<List<int>>();
        int tmpCount = 0;

        for (int i = 1; i < curLevelConfig.Count; i++)
        {
            var e1 = curLevelConfig[i - 1];
            var e2 = curLevelConfig[i];
            float offset = (float)(e2.Min - e1.Min);
            int count = Mathf.FloorToInt(offset * total);

            if (i == curLevelConfig.Count - 1)
            {
                count = total - tmpCount;
            }

            var tmp = new List<int>();
            for (int j = tmpCount; j < tmpCount + count; j++)
            {
                tmp.Add(colors[j]);
            }
            tmpCount += count;
            colorArr.Add(tmp);
        }

        Debug.Log($"colors: {string.Join(",", colors)}");
        Debug.Log($"colorArr: {string.Join(",", colorArr.Select(x => string.Join("|", x)))}");

        return colorArr;
    }

    private void InitLevelConfig(int level)
    {
        var colorArr = GetColorConfig(level);
        var colorsConfig = new List<int>();
        var targetConfig = new List<int>();

        foreach (var stageArr in colorArr)
        {
            var tmpColors = new List<int>();
            foreach (var color in stageArr)
            {
                for (int k = 0; k < 3; k++)
                {
                    tmpColors.Add(color);
                }
            }

            colorsConfig.AddRange(ShuffleList(tmpColors));
            targetConfig.AddRange(stageArr);
        }

        targetMgr.SetData(targetConfig, this);

        for (int bi = boards.Count - 1; bi >= 0; bi--)
        {
            var board = boards[bi];
            var screws = board.GetScrewComps();
            for (int l = 0; l < screws.Count; l++)
            {
                var comp = screws[l];
                if (colorsConfig.Count > 0)
                {
                    comp.SetType(colorsConfig[0]);
                    colorsConfig.RemoveAt(0);
                }
            }
        }
    }

    private void InitPaintBoard(GridDataWrapper paintData, int level)
    {
        paintBoard.LoadGridData(paintData);
        if (GameManager.Instance.currMode.id != GameMode.Feibiao)
        {
            paintBoard.FixHistory(120);
            return;
        }
        ///if (!LevelMgr.IsABTest()) return;

        if (GameManager.Instance.currMode.id == GameMode.Feibiao)
        {
            if (level >= 15 && level % 5 == 0)
            {
                levelText.color = Color.red;
                bossFlag.SetActive(true);
                Debug.Log("特殊处理关卡");

                if (level <= 15)
                {
                    paintBoard.FixHistory(80);
                }
                else if (level <= 30)
                {
                    paintBoard.FixHistory(90);
                }
                else if (level <= 60)
                {
                    paintBoard.FixHistory(100);
                }
                else
                {
                    paintBoard.FixHistory(118);
                }
            }
            else
            {
                Debug.Log("无需特殊处理关卡");
            }
        }
    }

    private int GetAllBoardHolesCount()
    {
        int holesCount = 0;
        foreach (Transform boardNodeLayer in boardLayer)
        {
            foreach (Transform child in boardNodeLayer)
            {
                var boardComp = child.GetComponent<Board>();
                holesCount += boardComp.holesNode.transform.childCount;
            }
        }
        return holesCount;
    }

    private void CheckBoards()
    {
        int holesCount = GetAllBoardHolesCount();
        int requiredHoles = paintBoard.GetOperateSteps().Count * 3;
        deafultHoleNum = requiredHoles;
        needHoleNum = requiredHoles - holesCount;
        if (holesCount < requiredHoles)
        {
            foreach (Transform boardNodeLayer in boardLayer)
            {
                boardNodeLayer.SetSiblingIndex(1000 + boardNodeLayer.GetSiblingIndex());
            }
            int needToAdd = requiredHoles - holesCount;
            var availableBoards = new List<Transform>();
            int startLayer = 4;// 第5层（索引从0开始）
            int endLayer = boardLayer.childCount - 3;

            for (int i = startLayer; i < endLayer; i++)
            {
                availableBoards.Add(boardLayer.GetChild(i));
            }

            int remainingToAdd = needToAdd;
            Debug.Log($"remainingToAdd: {remainingToAdd}");
            var boardPool = new List<Transform>(availableBoards);

            while (remainingToAdd > 0)
            {
                if (boardPool.Count == 0)
                {
                    boardPool.AddRange(availableBoards);
                }

                int randomIndex = Random.Range(0, boardPool.Count);
                var cloneLayer = Instantiate(boardPool[randomIndex], boardLayer);
                int selectedLayerHoles = 0;

                foreach (Transform child in cloneLayer)
                {
                    var boardComp = child.GetComponent<Board>();
                    selectedLayerHoles += boardComp.holesNode.transform.childCount;
                }

                if (selectedLayerHoles <= remainingToAdd)
                {
                    remainingToAdd -= selectedLayerHoles;
                }
                else
                {
                    int left = selectedLayerHoles - remainingToAdd;
                    for (int j = cloneLayer.childCount - 1; j >= 0; j--)
                    {
                        var boardComp = cloneLayer.GetChild(j).GetComponent<Board>();
                        int holeNum = boardComp.holesNode.transform.childCount;
                        if (holeNum <= left)
                        {
                            Destroy(boardComp.gameObject);
                            left -= holeNum;
                        }
                        else
                        {
                            for (int index = 0; index < left; index++)
                            {
                                Destroy(boardComp.holesNode.transform.GetChild(holeNum - index - 1).gameObject);
                            }
                            if (boardComp.holesNode.transform.childCount == 0)
                            {
                                Destroy(boardComp.gameObject);
                            }
                            break;
                        }
                    }
                    remainingToAdd = 0;
                }

                cloneLayer.SetSiblingIndex(1000 - boardLayer.childCount);
                boardPool.RemoveAt(randomIndex);
            }

            for (int i = 0; i < boardLayer.childCount; i++)
            {
                boardLayer.GetChild(i).SetSiblingIndex(i);
            }
        }
        else
        {
            Debug.Log("当前关卡不需要补充洞");
        }
    }

    private async Task InitBoards()
    {
        Debug.Log("InitBoards===============");
        boards = new List<Board>();

        List<string> color1 = new List<string>(COLORS);
        ShuffleList(color1);
        List<string> tmpColors = new List<string>();
        tmpColors.AddRange(color1);
        tmpColors.AddRange(color1);
        tmpColors.AddRange(color1);

        for (int index = 0; index < boardLayer.transform.childCount; index++)
        {
            Transform boardNodeLayer = boardLayer.transform.GetChild(index);
            // 按y坐标排序子节点
            List<Transform> children = new List<Transform>();
            for (int i = 0; i < boardNodeLayer.childCount; i++)
            {
                children.Add(boardNodeLayer.GetChild(i));
            }
            children.Sort((a, b) => b.position.y.CompareTo(a.position.y));
            for (int m = 0; m < children.Count; m++)
            {
                children[m].SetSiblingIndex(m);
            }

            //await Task.Delay(index * 10);
            var t = Time.frameCount;
            while (Time.frameCount - t < 2)
            {
                await Task.Yield();
            }

            for (int j = 0; j < boardNodeLayer.childCount; j++)
            {
                Transform boardNode = boardNodeLayer.GetChild(j);
                Rigidbody2D rigidBody = boardNode.GetComponent<Rigidbody2D>();
                rigidBody.simulated = true;
                rigidBody.angularDrag = 2;
                rigidBody.gravityScale = 2;

                Board boardComp = boardNode.GetComponent<Board>();
                //PolygonCollider2D physicsPolygonCollider = boardNode.GetComponent<PolygonCollider2D>();
                //PolygonCollider2D polygonCollider = boardNode.GetComponent<PolygonCollider2D>();
                boardComp.layerIndex = index + 1;
                boardNode.gameObject.layer = LayerMask.NameToLayer("UI");

                // 设置碰撞器
                //physicsPolygonCollider.points = polygonCollider.points;
                boardComp.Init(objPrefab);

                // 设置颜色
                Color color;
                ColorUtility.TryParseHtmlString(tmpColors[index % tmpColors.Count], out color);
                boardComp.SetBoardColor(color);

                boards.Add(boardComp);
            }
        }
    }

    private void RefreshBoardLockState()
    {
        var groupArr = new List<string>();
        for (int i = 1; i <= 20; i++)
        {
            groupArr.Add($"board_{i}");
        }

        for (int index = boardLayer.childCount - 1; index >= 0; index--)
        {
            var boardNodeLayer = boardLayer.GetChild(index);
            if (boardNodeLayer.childCount > 0)
            {
                var boardNode = boardNodeLayer.GetChild(0);
                int groupIndex = groupArr.IndexOf(LayerMask.LayerToName(boardNode.gameObject.layer));
                if (groupIndex >= 0)
                {
                    groupArr.RemoveAt(groupIndex);
                }
            }
        }
        int count = 0;
        for (int index = boardLayer.childCount - 1; index >= 0; index--)
        {
            var boardNodeLayer = boardLayer.GetChild(index);
            if (boardNodeLayer.childCount > 0)
            {
                count++;
            }

            string group = "";
            foreach (Transform boardNode in boardNodeLayer)
            {
                var boardComp = boardNode.GetComponent<Board>();
                if (count > 5)
                {
                    boardComp.SetLock(true, 0);
                }
                else if (count > 4)
                {
                    boardComp.SetLock(true);
                }
                else
                {
                    if (string.IsNullOrEmpty(group))
                    {
                        group = groupArr[0];
                        groupArr.RemoveAt(0);
                    }

                    if (boardNode.gameObject.layer == LayerMask.NameToLayer("UI") && groupArr.Count > 0)
                    {
                        PolygonCollider2D collider = boardNode.GetComponent<PolygonCollider2D>();
                        collider.enabled = false;
                        boardNode.gameObject.layer = LayerMask.NameToLayer(group);
                        collider.enabled = true;
                    }
                    boardComp.SetLock(false);
                }
            }
        }
    }

    public List<int> GetTopObjs(int layerCount = 1)
    {
        var sortedBoards = boards.Where(board => !board.isLocked)
                                .OrderByDescending(board => board.layerIndex)
                                .ToList();

        var types = new List<int>();
        int currentLayer = -1;
        int layerFound = 0;

        foreach (var board in sortedBoards)
        {
            if (currentLayer != board.layerIndex)
            {
                currentLayer = board.layerIndex;
                layerFound++;

                if (layerFound > layerCount)
                {
                    break;
                }
            }

            types.AddRange(board.GetScrewComps().Select(screw => screw.type));
        }

        return types;
    }

    public List<int> GetTouchedObjs()
    {
        var types = new List<int>();
        for (int index = boards.Count - 1; index >= 0; index--)
        {
            var board = boards[index];
            if (board.isLocked) continue;

            var screws = board.GetScrewComps();
            for (int k = 0; k < screws.Count; k++)
            {
                var comp = screws[k];
                if (!comp.isLocked && IsTouchenEnabled(board, comp))
                {
                    types.Add(comp.type);
                }
            }
        }
        return types;
    }

    private bool IsTouchenEnabled(Board senderBoard, Rope obj)
    {
        for (int i = 0; i < boards.Count; i++)
        {
            Board curBoard = boards[i];
            if (curBoard.isLocked) continue;
            if (curBoard == senderBoard) continue;
            if (curBoard.layerIndex <= senderBoard.layerIndex) continue;
            if (IsCollided(obj.sprite.transform, curBoard)) return false;
        }
        return true;
    }

    /** 判断多边形相交*/
    private bool IsCollided(Transform a, Board b)
    {
        PolygonCollider2D ap = a.GetComponent<PolygonCollider2D>();
        PolygonCollider2D bp = b.transform.GetComponent<PolygonCollider2D>();
        float distance = Physics2D.Distance(ap, bp).distance;
        bool isClose = distance < 0.01f;
        return isClose;
    }

    public void OnBlockLayerTouchStart()
    {
        Debug.Log("touch block layer");
        Vector2 touchLoc = Input.mousePosition;
        Debug.Log($"touchLoc: {touchLoc}");

        for (int index = boards.Count - 1; index >= 0; index--)
        {
            var board = boards[index];
            if (board.isLocked) continue;

            var wp = board.transform.GetComponent<PolygonCollider2D>().points;
            // 将多边形顶点转换到世界坐标系
            Vector2[] worldPoints = new Vector2[wp.Length];
            for (int i = 0; i < wp.Length; i++)
            {
                worldPoints[i] = board.transform.TransformPoint(wp[i]);
            }
            if (IsPointInPolygon(touchLoc, worldPoints))
            {
                effectLayer.PlayHammerEffect(board.transform);
                //blockLayer.GetComponent<Button>().onClick.RemoveListener(OnBlockLayerTouchStart);

                DOVirtual.DelayedCall(0.5f, () =>
                {
                    blockLayer.SetActive(false);
                    //blockLayer.GetComponent<Button>().onClick.AddListener(OnBlockLayerTouchStart);
                    EventMng.dispatchEvent((EventTypes.REMOVE_BOARD), board);
                    SoundManager.Ins.PlaySfx("banzisui");
                });
                return;
            }
        }
        UIManager.Instance.ShowMsg("Please select a panel to smash");
    }

    /** 点击毛线圈*/
    private void OnTouchStart()
    {
        Vector2 touchLoc = Input.mousePosition;
        for (int bi = boards.Count - 1; bi >= 0; bi--)
        {
            Board curBoard = boards[bi];
            var onBoardObjs = curBoard.GetScrewComps();
            for (int j = 0; j < onBoardObjs.Count; j++)
            {
                Rope curObj = onBoardObjs[j];
                if (curObj.isLocked) continue;

                var wp = curObj.sprite.GetComponent<PolygonCollider2D>().points;
                // 将多边形顶点转换到世界坐标系
                Vector2[] worldPoints = new Vector2[wp.Length];
                for (int i = 0; i < wp.Length; i++)
                {
                    worldPoints[i] = curObj.sprite.transform.TransformPoint(wp[i]);
                }

                // 检查点击位置是否在物体范围内
                if (IsPointInPolygon(touchLoc, worldPoints))
                {
                    Transform trans = curBoard.transform.parent;
                    Debug.Log("板子层数====" + trans);
                    if (!IsTouchenEnabled(curBoard, curObj))
                    {
                        curObj.Shake();
                        return;
                    }
                    TouchOperateObj(curObj);
                    return;
                }
            }
        }
    }

    private void TouchOperateObj(Rope comp)
    {
        targetMgr.OnTouchOperateObj(comp);
        EventMng.dispatchEvent((EventTypes.SCREW_REMOVE), comp.transform);
        SoundManager.Ins.PlaySfx("dianjifeibiao");
        //PlatformService.Instance.VibrateShort(false);
    }

    void OnBeginContact(Collision2D collision)
    {
        var otherCollider = collision.collider;
        otherCollider.enabled = false;
        //collision.collider.enabled = false;  // 正确的注释方式

        var board = otherCollider.GetComponent<Board>();
        int index = boards.IndexOf(board);
        if (index > -1)
        {
            Debug.Log("移出板子");
            Destroy(otherCollider.gameObject);
            boards.RemoveAt(index);
            RefreshBoardLockState();
        }
    }

    private static List<T> ShuffleList<T>(List<T> list)
    {
        List<T> result = new List<T>(list);
        for (int i = result.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            T temp = result[i];
            result[i] = result[j];
            result[j] = temp;
        }
        return result;
    }

    private bool IsPointInPolygon(Vector2 point, Vector2[] polygon)
    {
        bool inside = false;
        int j = polygon.Length - 1;

        for (int i = 0; i < polygon.Length; i++)
        {
            if (((polygon[i].y > point.y) != (polygon[j].y > point.y)) &&
                (point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) / (polygon[j].y - polygon[i].y) + polygon[i].x))
            {
                inside = !inside;
            }
            j = i;
        }

        return inside;
    }

    private bool IsPolygonColliding(Vector2[] poly1, Vector2[] poly2)
    {
        for (int i = 0; i < poly1.Length; i++)
        {
            if (IsPointInPolygon(poly1[i], poly2)) return true;
        }

        for (int i = 0; i < poly2.Length; i++)
        {
            if (IsPointInPolygon(poly2[i], poly1)) return true;
        }

        return false;
    }
}
