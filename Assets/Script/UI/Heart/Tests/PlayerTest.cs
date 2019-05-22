using System;
using NUnit.Framework;

namespace Tests
{
    public class PlayerTest
    {
        public class TestPlayerHealthProperty
        {
            [Test]
            public void _0_Player_Health_Default_To_50()
            {
                var target = new PlayerHp(50,100);
                Assert.AreEqual(50,target.Health);
            }

            [Test]
            public void _1_Health_With_Maximum_100()
            {
                var target = new PlayerHp(50,100);
                target.SetHealth = 150;
                Assert.AreEqual(100,target.Health);
            }
        }
        public class TestReplenishMethod
        {
            [Test]
            public void _0_Health_With_50_To_50()
            {
                var target = new PlayerHp(50,100);
                target.Heal(0);
                Assert.AreEqual(50,target.Health);
            }

            [Test]
            public void _1_Health_With_50_To_100()
            {
                var target = new PlayerHp(50,100);
                target.Heal(50);
                Assert.AreEqual(100,target.Health);
            }

            [Test]
            public void _2_Health_Not_Exceed_Maximum()
            {
                var target = new PlayerHp(50,100);
                target.Heal(100);
                Assert.AreEqual(100,target.Health);
            }
        }
        public class TestDepleteMethod
        {
            [Test]
            public void _0_Health_With_50_To_0()
            {
                var target = new PlayerHp(50,100);
                target.Damage(50);
                Assert.AreEqual(0, target.Health);
            }

            [Test]
            public void _1_Health_Not_Exceed_Minimum()
            {
                var target = new PlayerHp(50,100);
                target.Damage(100);
                Assert.AreEqual(0,target.Health);
            }
        }
        
        public class TestHealedEvent
        {
            [Test]
            public void _0_Is_Healed_Event_Raised()
            {
                var amount = -1;
                var player = new PlayerHp(1,2);
                player.Healed += (sender,args)=>amount = args.Amount;
                player.Heal(0);
                
                Assert.AreEqual(0,amount);
            }

            [Test]
            public void _1_Heal_Not_Exceed_Maximum()
            {
                int amount = 0;
                var player = new PlayerHp(1,2);
                player.Healed += (sender,args)=>amount = args.Amount;
                player.Heal(2);
                
                Assert.AreEqual(1,amount);
            }
        }
        
        public class TestDamagedEvent
        {
            [Test]
            public void _0_Is_Damaged_Event_Raised()
            {
                int amount = -1;
                var player = new PlayerHp(1,2);
                player.Damaged += (sender, args) => amount = args.Amount;
                player.Damage(0);

                Assert.AreEqual(0, amount);
            }

            [Test]
            public void _1_Damage_Event_Not_Exceed_Minimum()
            {
                var amount = 2;
                var player = new PlayerHp(1,2);
                player.Damaged += (sender, args) => amount = args.Amount;
                player.Damage(2);

                Assert.AreEqual(1,amount);
            }
        }
    }
}