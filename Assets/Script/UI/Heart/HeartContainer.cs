using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class HeartContainer
{
    private const float       EachHeartPieceValue = 0.25f;
    public List<Heart> m_hearts;

    public HeartContainer(List<Heart> hearts) => m_hearts = hearts;

    public void Replenish(int heartPieces)
    {
        foreach (var heart in m_hearts)
        {
            var emptyHeartPieces = heart.EmptyHeartPieces;
            var toReplenish = heartPieces < emptyHeartPieces
                ? heartPieces
                : emptyHeartPieces;
            heart.Replenish(toReplenish);

            heartPieces -= emptyHeartPieces;
            if (heartPieces <= 0) 
                break;
        }
    }

    public void Deplete(int heartPieces)
    {
        // watch out the foreach order
        foreach (var heart in m_hearts.AsEnumerable().Reverse())
        {
            var currentHeartPieces = heart.CurrentHeartPieces;
            var toDeplete = heartPieces < currentHeartPieces
                ? heartPieces
                : currentHeartPieces;
            heart.Deplete(toDeplete);

            heartPieces -= currentHeartPieces;
            if (heartPieces <= 0)
                break;
        }
    }
}
