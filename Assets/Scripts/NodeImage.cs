using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NodeImage : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler
{
    #region public field

    public Node data;

    public Text text_F;
    public Text text_G;
    public Text text_H;
    public Transform arrow;
    [NonSerialized] public RectTransform rect;

	#endregion


	#region private field
    
    private Image img;

	#endregion


	#region monobehavior callbacks

    void Awake ()
    {
        rect = GetComponent<RectTransform>();
        img = GetComponent<Image>();
    }

	#endregion

	#region custom methods

    public void SetDataUI()
    {
        text_F.text = data.F.ToString();
        text_G.text = data.G.ToString();
        text_H.text = data.H.ToString();
        text_F.gameObject.SetActive(true);
        text_G.gameObject.SetActive(true);
        text_H.gameObject.SetActive(true);

        if (data.parent != null)
        {
            float scale = AStarManager.instance.nodeWidth / 100f;
            arrow.localScale = new Vector3(scale, scale, scale);
            arrow.rotation = Quaternion.Euler(0, 0, Node.GetArrowAngle(data));
            arrow.gameObject.SetActive(true);
        }
    }

    public void ResetDataUI()
    {
        text_F.gameObject.SetActive(false);
        text_G.gameObject.SetActive(false);
        text_H.gameObject.SetActive(false);
        arrow.gameObject.SetActive(false);
    }

    public void SetRect(Vector2 pos, float width)
    {
        rect.anchoredPosition = pos;
        rect.sizeDelta = new Vector2(width, width);
    }

    public void SetAsStartNode()
    {
        if (AStarManager.instance.startNode != null)
        {
            AStarManager.instance.startNode.SetAsNormalNode();
        }

        if (AStarManager.instance.endNode == this)
        {
            AStarManager.instance.endNode = null;
        }

        data.state = BlockState.End;
        data.state = BlockState.Start;
        AStarManager.instance.startNode = this;
        img.color = AStarManager.instance.startColor;
    }

    public void SetAsEndNode()
    {
        if (AStarManager.instance.endNode != null)
        {
            AStarManager.instance.endNode.SetAsNormalNode();
        }

        if (AStarManager.instance.startNode == this)
        {
            AStarManager.instance.startNode = null;
        }

        data.state = BlockState.End;
        AStarManager.instance.endNode = this;
        img.color = AStarManager.instance.endColor;
    }

    public void SetAsObstruct()
    {
        if (AStarManager.instance.startNode == this)
        {
            AStarManager.instance.startNode = null;
        }

        if (AStarManager.instance.endNode == this)
        {
            AStarManager.instance.endNode = null;
        }

        data.state = BlockState.Obstruct;
        AStarManager.instance.AddObstruct(this);
        img.color = AStarManager.instance.obstructColor;
    }

    public void SetAsNormalNode()
    {
        data.state = BlockState.None;

        if (AStarManager.instance.startNode == this)
        {
            AStarManager.instance.startNode = null;
        }

        if (AStarManager.instance.endNode == this)
        {
            AStarManager.instance.endNode = null;
        }

        if (AStarManager.instance.obstruct.Contains(this))
        {
            AStarManager.instance.obstruct.Remove(this);
        }

        img.color = Color.white;
    }

    public void SetAsPathNode(Color pathColor)
    {
        img.color = pathColor;
    }

    public void CheckIsFindPath()
    {
        if (AStarManager.instance.isFind)
        {
            AStarManager.instance.ResetDataTotally();
        }
    }

    void Operate()
    {
        CheckIsFindPath();

        switch (AStarManager.instance.operateType)
        {
            case OperateType.Normal:
                SetAsNormalNode();
                break;
            case OperateType.Start:
                SetAsStartNode();
                break;
            case OperateType.End:
                SetAsEndNode();
                break;
            case OperateType.Obstruct:
                SetAsObstruct();
                break;
            default:
                SetAsNormalNode();
                break;
        }
    }

    #endregion


    #region custom coroutines



    #endregion

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (AStarManager.instance.isMouseDown)
        {
            Operate();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Operate();
    }
}