using System.Collections.Generic;
using UnityEngine.UI;

public class HeartContainer
{
    private const float       EachHeartPieceValue = 0.25f;
    private List<Heart> m_hearts;
    private List<Image> m_images;

    public HeartContainer(List<Heart> hearts) => m_hearts = hearts;

    public void Replenish(int numberOfHeartPieces)
    {
        if (m_hearts.Count > 0) 
        {
            var heartPiecesPerHeart = Heart.HeartPiecesPerHeart;
            for (int i = 0; i < m_hearts.Count; i++)
            {
                var heart = m_hearts[i];
                var currentHeartPieces = heart.CurrentNumberOfHeartPieces;
                if (currentHeartPieces < heartPiecesPerHeart)
                {
                    var currentEmptyHeartPieces = heartPiecesPerHeart-m_hearts[i].CurrentNumberOfHeartPieces;
                    var numberOfRemainingHeartPieces = numberOfHeartPieces - currentEmptyHeartPieces;
                    if (numberOfRemainingHeartPieces <= 0)
                    {
                        heart.Replenish(numberOfHeartPieces);
                        break; 
                    }
                    heart.Replenish(currentEmptyHeartPieces);
                    Replenish(numberOfRemainingHeartPieces);
                    break;
                }
            }
        }
    }
}
