using System.Collections.Generic;
using System.Linq;
using ProjectZ.UI.Heart;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectZ.UI
{
    public class App : MonoBehaviour
    {
        [SerializeField]
        private int modifyAmount = 1;

        [SerializeField]
        private List<Image> images;

        private HeartContainer m_heartContainer;
        private PlayerHp       player;

        private void Start()
        {
            // @Todo Do some search : List.Select?
            m_heartContainer = new HeartContainer(
                images.Select(image => new Heart.Heart(image)).ToList());
            player = new PlayerHp(20, 20);
            AddListener();
        }

        void AddListener()
        {
            player.Healed  += (sender, args) => m_heartContainer.Replenish(args.Amount);
            player.Damaged += (sender, args) => m_heartContainer.Deplete(args.Amount);
        }

        // todo 为什么只能减到1颗星？最后两个星一起减少
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                player.Heal(modifyAmount);
            }

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                player.Damage(modifyAmount);
                foreach (var VARIABLE in m_heartContainer.m_hearts)
                {
                    print(VARIABLE.CurrentHeartPieces);
                }
            }
        }
    }
}
