//using System.Collections.Generic;
//using Unity.Collections;
//using Unity.Entities;
//using Unity.Jobs;
//using ProjectZ.Component.Setting;
//
//namespace ProjectZ.AI
//{
//    public class BehaviourSystem : JobComponentSystem
//    {
//        private          bool                   m_isInitialize;
//        private readonly List<BehaviourSetting> m_uniqueType = new List<BehaviourSetting>(5);
//
//        protected override JobHandle OnUpdate(JobHandle inputDependency)
//        {
//            EntityManager.GetAllUniqueSharedComponentData(m_uniqueType);
//            var settings = m_uniqueType[1];
//
//            var processFactorJob = new ExecuteBehaviour
//            {
//                Settings = settings
//            };
//            var executeBehaviourJobHandle = processFactorJob.Schedule(this,inputDependency);
//            m_uniqueType.Clear();
//            inputDependency = executeBehaviourJobHandle;
//            return inputDependency;
//        }
//
//        private struct ExecuteBehaviour : IJobForEachWithEntity<HumanState, HumanStock, CurrentBehaviour, Navigation>
//        {
//            [ReadOnly]
//            public BehaviourSetting Settings;
//
//            private void CalculateBehaviourEffect(ref int gainFactor, ref int payFactor, int gainValue, int payValue)
//            {
//                gainFactor += gainValue;
//                payFactor  -= payValue;
//            }
//
//            private void PrepareValueForBehaviour(BehaviourType behaviourType, out int gainValue, out int payValue,
//                out int coolDownTime)
//            {
//                switch (behaviourType)
//                {
//                    case BehaviourType.Eat:
//                        gainValue    = Settings.eatGain;
//                        payValue     = Settings.eatCost;
//                        coolDownTime = Settings.eatCoolDownInMinute;
//                        break;
//
//                    case BehaviourType.Drink:
//                        gainValue    = Settings.drinkGain;
//                        payValue     = Settings.drinkCost;
//                        coolDownTime = Settings.drinkCoolDownInMinute;
//                        break;
//
//                    // Do nothing.
//                    default:
//                        gainValue    = Settings.eatGain;
//                        payValue     = Settings.eatCost;
//                        coolDownTime = Settings.eatCoolDownInMinute;
//                        break;
//                }
//            }
//
//            public void Execute(Entity entity, int index, 
//                ref HumanState state,
//                ref HumanStock stock, 
//                [ReadOnly]ref CurrentBehaviour currentBehaviour,
//                [ReadOnly]ref Navigation navigation)
//            {
//                PrepareValueForBehaviour(currentBehaviour.BehaviourType, out var gainValue,
//                    out var payValue, out var coolDownTime);
//            
//                if (!navigation.Arrived)
//                    currentBehaviour.ExecuteTimer = 0;
//                else
//                {
//                    if (currentBehaviour.ExecuteTimer > coolDownTime)
//                    {
//                        switch (currentBehaviour.BehaviourType)
//                        {
//                            case BehaviourType.Eat:
//                                CalculateBehaviourEffect(ref state.Hungry, ref stock.Food, gainValue, payValue);
//                                break;
//                            case BehaviourType.Drink:
//                                CalculateBehaviourEffect(ref state.Thirsty, ref stock.Water, gainValue,
//                                    payValue);
//                                break;
//                        }
//                    }
//                }
//            }
//        }
//    }
//}