using UnityEngine;
using UnityEngine.UI;

namespace Utility
{
    public class App : MonoBehaviour
    {
        [SerializeField] private Image image;
        private                  Heart m_heart;
        private void Start()
        {
            m_heart = new Heart(image);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                m_heart.Replenish(1);
            }
        
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                m_heart.Deplete(1);
            }
        }
    }
}
