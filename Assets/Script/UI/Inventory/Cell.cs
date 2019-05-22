using UnityEngine;

public class Cell : MonoBehaviour
{
    #region Field
    private bool m_isEmpty = true;
    private int  m_itemIndex;
    #endregion
    
    #region Property
    public int CellIndex { get; }
    public bool IsEmpty => m_isEmpty;
    #endregion

    public void Initialize()
    {
        m_isEmpty = true;
    }

    public void SetItemIndex(int itemIndex)
    {
        m_isEmpty   = false;
        m_itemIndex = itemIndex;
    }

    public void ClearItemIndex()
    {
        m_isEmpty = true;
        m_itemIndex = -1;
    }

    public int GetItemIndex()
    {
        return m_itemIndex;
    }
}