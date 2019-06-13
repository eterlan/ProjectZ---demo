using System;
using UnityEngine.UI;

namespace ProjectZ.UI.Heart
{
    public class Heart
    {
        private readonly Image m_image;
        private const    float PerHeartPieceFill   = .25f;
        public static    int   HeartPiecesPerHeart = 4;
        private          int   CurrentFilledHeartPieces => (int) (m_image.fillAmount * HeartPiecesPerHeart);

        public Heart(Image image)
        {
            m_image = image;
        }

        public int CurrentHeartPieces => CurrentFilledHeartPieces;

        public int EmptyHeartPieces => HeartPiecesPerHeart - CurrentFilledHeartPieces;

        public void Replenish(int numberOfHeartPieces)
        {
            if (numberOfHeartPieces < 0) 
                throw new ArgumentOutOfRangeException(nameof(numberOfHeartPieces));
            m_image.fillAmount += numberOfHeartPieces * PerHeartPieceFill;
        }

        public void Deplete(int numberOfHeartPieces)
        {
            if (numberOfHeartPieces < 0) 
                throw new ArgumentOutOfRangeException(nameof(numberOfHeartPieces));
            m_image.fillAmount -= numberOfHeartPieces * PerHeartPieceFill;
        }
    }
}