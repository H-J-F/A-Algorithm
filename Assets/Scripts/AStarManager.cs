using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum OperateType
{
    Normal = 0,
    Start = 1,
    End = 2,
    Obstruct = 3
}

public class AStarManager : MonoBehaviour 
{
	#region public field

    public static AStarManager instance;

    [NonSerialized] public float nodeWidth;

    public bool isFind = false;
    public bool isMouseDown = false;
    public List<NodeImage> path = new List<NodeImage>();
    public List<NodeImage> obstruct = new List<NodeImage>();
    public OperateType operateType = OperateType.Normal;

    public Color startColor;
    public Color endColor;
    public Color obstructColor;
    public Color pathColor;

    public Button calculateBtn;
    public Button restartBtn;
    public Button resetBtn;
    public Button startBtn;
    public Button endBtn;
    public Button obstructBtn;
    public Button submitBtn;
    public Toggle canCrossObstructToggle;

    public InputField rowInput;
    public InputField columInput;

    public Text hintText;

    // 控制UI显示的Transform
    public Transform menu;
    public Transform inputTrans;
    public RectTransform mapGenerator;

    public GameObject nodePrefab;

    [NonSerialized] public NodeImage startNode;
    [NonSerialized] public NodeImage endNode;

    #endregion


    #region private field

    private bool canCrossCorner = false;
    private bool lastCanCrossCorner = false;
    private int col, row;
    
    private List<NodeImage> openList = new List<NodeImage>();
    private List<NodeImage> closeList = new List<NodeImage>();

    private NodeImage[,] map;

    #endregion


    #region monobehavior callbacks

    void Awake()
    {
        instance = this;
    }

	void Start () {
		mapGenerator.gameObject.SetActive(false);
        menu.gameObject.SetActive(false);
        inputTrans.gameObject.SetActive(true);
        hintText.gameObject.SetActive(false);

        calculateBtn.onClick.AddListener(OnCalculateBtnClick);
        restartBtn.onClick.AddListener(OnReStartBtnClick);
        resetBtn.onClick.AddListener(OnResetBtnClick);
        startBtn.onClick.AddListener(OnStartBtnClick);
        endBtn.onClick.AddListener(OnEndBtnClick);
        obstructBtn.onClick.AddListener(OnObstructBtnClick);
        submitBtn.onClick.AddListener(OnSubmitBtnClick);
        canCrossObstructToggle.onValueChanged.AddListener(OnCanCrossObstructToggle);
    }
	
	void Update ()
    {
        isMouseDown = Input.GetMouseButton(0);
    }

	#endregion


	#region custom methods

    void OnCalculateBtnClick()
    {
        if (!isFind)
        {
            if (startNode != null && endNode != null)
            {
                CalculatePath();
            }
            else
            {
                StartCoroutine(ShowHintText("请指定起点和终点!"));
            }
        }
        else
        {
            if (lastCanCrossCorner != canCrossCorner)
            {
                ResetCalculateList();
                CalculatePath();
            }
            else
            {
                StartCoroutine(ShowHintText("请重新指定起点和终点!"));
            }
        }
    }

    void OnReStartBtnClick()
    {
        SceneManager.LoadScene("Main");
    }

    void OnResetBtnClick()
    {
        operateType = OperateType.Normal;
    }

    void OnStartBtnClick()
    {
        operateType = OperateType.Start;
    }

    void OnEndBtnClick()
    {
        operateType = OperateType.End;
    }

    void OnObstructBtnClick()
    {
        operateType = OperateType.Obstruct;
    }

    void OnSubmitBtnClick()
    {
        if (!string.IsNullOrEmpty(rowInput.text) && !string.IsNullOrEmpty(columInput.text))
        {
            try
            {
                row = int.Parse(rowInput.text);
                col = int.Parse(columInput.text);

                if (row <= 2 && col <= 2)
                {
                    StartCoroutine(ShowHintText("输入的行列过小!"));
                    return;
                }

                float mapWidth = mapGenerator.rect.width;
                float mapHeight = mapGenerator.rect.height;

                bool heightOrWidth = mapHeight / row < mapWidth / col;
                nodeWidth = heightOrWidth ? mapHeight / row - 2 : mapWidth / col - 2;

                if (nodeWidth < 60f)
                {
                    StartCoroutine(ShowHintText("方格过小！请重新设置！"));
                    return;
                }

                mapGenerator.gameObject.SetActive(true);
                menu.gameObject.SetActive(true);
                inputTrans.gameObject.SetActive(false);

                InitMap(col, row, nodeWidth);
            }
            catch
            {
                StartCoroutine(ShowHintText("请输入正确的列数和行数!"));
            }
        }
    }

    void OnCanCrossObstructToggle(bool toggle)
    {
        canCrossCorner = toggle;
    }

    public void AddObstruct(NodeImage node)
    {
        if (obstruct.Contains(node))
        {
            return;
        }

        obstruct.Add(node);
    }

    void InitMap(int row, int col, float nodeWidth)
    {
        map = new NodeImage[row, col];
        
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < col; j++)
            {
                GameObject go = Instantiate(nodePrefab, mapGenerator);
                NodeImage ni = go.GetComponent<NodeImage>();
                map[i, j] = ni;
                Node data = new Node() {col = i, row = j, F = 0, G = 0, H = 0, parent = null, state = BlockState.None};
                map[i, j].data = data;
                map[i, j].SetRect(new Vector2((i - row / 2f) * (nodeWidth + 2f) + nodeWidth / 2f, (col / 2f - j) * (nodeWidth + 2f) - nodeWidth / 2f), nodeWidth);
            }
        }

        StartCoroutine(ShowHintText("成功生成地图！"));
    }

    void CalculatePath()
    {
        NodeImage node = startNode;
        openList.Add(node);

        while (openList.Count > 0 && !isFind)
        {
            openList.Remove(node);
            closeList.Add(node);

            for (int i = 0; i < 3 && !isFind; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (node.data.col - 1 + i >= 0 &&
                        node.data.col - 1 + i < map.GetLength(0) &&
                        node.data.row - 1 + j >= 0 &&
                        node.data.row - 1 + j < map.GetLength(1))
                    {

                        NodeImage otherNode = map[node.data.col - 1 + i, node.data.row - 1 + j];
                        
                        if (startNode.data.col == otherNode.data.col && startNode.data.row == otherNode.data.row ||
                            otherNode.data.col == node.data.col && otherNode.data.row == node.data.row ||
                            otherNode.data.state == BlockState.Obstruct || closeList.Contains(otherNode) )
                        {
                            continue;
                        }

                        // 是否可跨越障碍到达对角节点
                        if (!canCrossCorner)
                        {
                            // 与对角节点同时相邻的节点是否为障碍节点
                            if (Node.IsImpededByObstruct(map, node.data, otherNode.data))
                            {
                                continue;
                            }
                        }

                        if (endNode.data.col == otherNode.data.col && endNode.data.row == otherNode.data.row)
                        {
                            otherNode.data.parent = node;
                            openList.Add(otherNode);
                            isFind = true;
                            break;
                        }

                        if (!openList.Contains(otherNode))
                        {
                            otherNode.data.parent = node;
                            openList.Add(otherNode);
                            otherNode.data.G = Node.GetNodeG(node.data, otherNode.data);
                            otherNode.data.H = Node.GetNodeH(otherNode.data, endNode.data);
                            otherNode.data.F = Node.GetNodeF(otherNode.data);
                        }
                        else
                        {
                            if (node.data.G + Node.GetPrice(node.data, otherNode.data) < otherNode.data.G)
                            {
                                otherNode.data.parent = node;
                                otherNode.data.G = otherNode.data.parent.data.G + Node.GetPrice(node.data, otherNode.data);
                                otherNode.data.F = Node.GetNodeF(otherNode.data);
                            }
                        }
                    }
                }
            }

            node = Node.GetMinFNode(openList);
        }

        lastCanCrossCorner = canCrossCorner;

        if (isFind)
        {
            ShowPath();
        }
        else
        {
            ResetCalculateList();
            StartCoroutine(ShowHintText("没有可达到终点的路径!"));
        }
    }

    void ShowPath()
    {
        foreach (NodeImage item in openList)
        {
            if (item != endNode)
            {
                item.SetDataUI();
            }
        }

        foreach (NodeImage item in closeList)
        {
            if (item != startNode)
            {
                item.SetDataUI();
            }
        }

        NodeImage node = endNode;
        path.Add(node);
        
        while (node.data.parent != null)
        {
            path.Add(node.data.parent);
            node = node.data.parent;
        }

        string s = "";

        for (int i = path.Count - 1; i >= 0; i--)
        {
            s += "(" + path[i].data.col + "," + path[i].data.row + ")";

            if (path[i] != startNode && path[i] != endNode)
            {
                path[i].SetAsPathNode(pathColor);
            }
            
            if (i != 0)
            {
                s += "->";
            }
        }

        Debug.Log(s);
    }

    // 重置所有计算用辅助数据，清除所有节点UI高亮显示
    public void ResetDataTotally()
    {
        startNode = null;
        endNode = null;

        ResetCalculateList();

        while (obstruct.Count > 0)
        {
            obstruct[0].SetAsNormalNode();
        }

        foreach (NodeImage node in path)
        {
            node.SetAsNormalNode();
            node.data.Reset();
        }

        path.Clear();

        obstruct.Clear();
    }

    // 重置计算用辅助数据，但不重置起点终点和障碍点，清除路径节点的UI高亮显示
    private void ResetCalculateList()
    {
        // 这里必须重置isFind，否则无法进入计算路径
        isFind = false;

        foreach (NodeImage item in openList)
        {
            item.ResetDataUI();
            item.data.Reset();
        }

        foreach (NodeImage item in closeList)
        {
            item.ResetDataUI();
            item.data.Reset();
        }

        path.Remove(startNode);
        path.Remove(endNode);

        foreach (NodeImage node in path)
        {
            node.ResetDataUI();
            node.SetAsNormalNode();
            node.data.Reset();
        }

        path.Clear();
        openList.Clear();
        closeList.Clear();
    }

	#endregion


	#region custom coroutines

    IEnumerator ShowHintText(string text)
    {
        hintText.text = text;
        hintText.gameObject.SetActive(true);

        yield return new WaitForSeconds(3f);

        hintText.gameObject.SetActive(false);
    }

	#endregion
}