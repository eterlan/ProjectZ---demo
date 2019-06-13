
using System;

namespace ProjectZ.UI.Heart
{
    public class PlayerHp
    {
        private int m_health;
        private int m_maximumHealth;
        private int m_minimumHealth = 0;
    
        // Why use event keyword?
        public event EventHandler<HealedEventArgs>  Healed;
        public event EventHandler<DamagedEventArgs> Damaged;
        public int                                  Health => m_health;

        public int SetHealth
        {
            set
            {
                if (value <= m_maximumHealth)
                {
                    m_health = value >= m_minimumHealth ? value : m_minimumHealth;
                }
                else
                    m_health = m_maximumHealth;
            }
        }

        /// <summary>
        /// Don't set anything in the constructor, for the sake of maintainability
        /// </summary>
        public PlayerHp(int currentHp, int maximumHp = 100)
        {
            m_health        = currentHp;
            m_maximumHealth = maximumHp;
        }

        public void Heal(int amount)
        {
            var newHealth = Math.Min(m_health + amount, m_maximumHealth);
            if (Healed != null)
            {
                var changeAmount = newHealth - Health;
                Healed(this, new HealedEventArgs(changeAmount));
            }
            m_health = newHealth;
        }

        public void Damage(int amount)
        {
            var newHealth = Math.Max(m_health - amount, m_minimumHealth);
        
            if (Damaged != null)
            {
                var changedAmount = Health - newHealth;
                Damaged(this, new DamagedEventArgs(changedAmount));
            }
            m_health = newHealth;
        }
    
        public class HealedEventArgs : EventArgs
        {
            public int Amount { get; private set; }
            public HealedEventArgs(int amount)
            {
                Amount = amount;
            }
        }

        public class DamagedEventArgs : EventArgs 
        {
            public int Amount { get; private set; }

            public DamagedEventArgs(int amount)
            {
                Amount = amount;
            }
        }
    }
}