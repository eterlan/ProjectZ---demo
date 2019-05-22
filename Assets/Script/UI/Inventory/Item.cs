using UnityEngine;

public class Item : MonoBehaviour
{
    private readonly ImageSize m_imageSize;
    private int m_firstCellIndex;

    public Item() : this(new ImageSize()) // @TODO: What's this means? Use base constructor?
    {
        m_imageSize = new ImageSize(1,1);
    }

    public Item(ImageSize imageSize)
    {
        m_imageSize = imageSize;
    }

    public ImageSize ImageSize => m_imageSize;

    public int FirstCellIndex
    {
        get => m_firstCellIndex;
        set => m_firstCellIndex = value;
    }

    public int Index { get; }

    public void SetFirstCellIndex(int cellIndex)
    {
        m_firstCellIndex = cellIndex;
    }
}