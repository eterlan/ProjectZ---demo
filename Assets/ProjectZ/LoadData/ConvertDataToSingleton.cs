using System;
using System.Collections.Generic;
using ProjectZ.Component;
using ProjectZ.Component.Setting;
using Unity.Entities;

namespace ProjectZ.LoadData
{
    /// <summary>
    /// From JsonReader to DataSingleton
    /// </summary>
    [DisableAutoCreation]
    public class ConvertDataToSingleton : ComponentSystem
    {
        // Maybe I should do it in Monobehaviour? For the sake of it run only once. 
        protected override void OnUpdate()
        {
            EntityManager.CreateEntity(typeof(AIDataSingleton));
            LoadFactorInfoData();
            LoadBehaviourData();
        }

        private void LoadFactorInfoData()
        {
            // GetJsonReader Data
            var factorInfos = JsonReader.LoadData<FactorInfo>();

            // Assign value to DataSingleton.
            
            //@TODO 在哪里初始化比较好呢？Singleton 里面还是这里？放在应用层会经常忘记填一行诶。。
            // 放在了内部。

            for (int i = 0; i < factorInfos.Length; i++)
            {
                AIDataSingleton.Factors.FactorsMax[i]  = factorInfos[i].Max;
                AIDataSingleton.Factors.FactorsMin[i]  = factorInfos[i].Min;
                AIDataSingleton.Factors.FactorsType[i] = JsonHelper.ParseEnum<FactorType>(factorInfos[i].Name);
            }
        }

        private void LoadBehaviourData()
        {
            var needLvLength = Setting.NeedLvCount;
            var needLvInfo = new List<List<BehaviourType>>(needLvLength);
            for (var i = 0; i < needLvInfo.Capacity; i++)
            {
                needLvInfo.Add(new List<BehaviourType>());
            }
            
            // Get BehaviourInfo Data
            var behavioursInfo = JsonReader.LoadData<BehaviourInfo>();
            var behaviourLength = behavioursInfo.Length;

            for (var i = 0; i < behaviourLength; i++)
            {
                AIDataSingleton.Behaviours.BehavioursType[i] =
                    JsonHelper.ParseEnum<BehaviourType>(behavioursInfo[i].Name);
                
                // Fill behaviour -> factorInfo 
                var behaveFactorsCount = behavioursInfo[i].FactorsInfo.Length;
                AIDataSingleton.Behaviours.FactorsInfo.Modes[i] = new FactorMode[behaveFactorsCount];
                AIDataSingleton.Behaviours.FactorsInfo.Types[i] = new FactorType[behaveFactorsCount];
                AIDataSingleton.Behaviours.FactorsInfo.Weights[i] = new float[behaveFactorsCount];
                for (var j = 0; j < behaveFactorsCount; j++)
                {
                    var modeType = JsonHelper.ParseEnum<FactorMode>(behavioursInfo[i].FactorsInfo[j].FactorMode);
                    AIDataSingleton.Behaviours.FactorsInfo.Modes[i][j] = modeType;
                    var factorType = JsonHelper.ParseEnum<FactorType>(behavioursInfo[i].FactorsInfo[j].FactorName);
                    AIDataSingleton.Behaviours.FactorsInfo.Types[i][j] = factorType;
                    var weight = behavioursInfo[i].FactorsInfo[j].FactorWeight;
                    AIDataSingleton.Behaviours.FactorsInfo.Weights[i][j] = weight;
                }
                
                // Fill NeedLvInfo 
                var needLv = behavioursInfo[i].NeedLevel;
                needLvInfo[needLv].Add(AIDataSingleton.Behaviours.BehavioursType[i]);
            }
            // 将全部行为按类别分好后，添加至static NeedLvInfo 中
            for (var i = 0; i < needLvLength; i++)
            {
                AIDataSingleton.NeedLevels.BehavioursType[i] = needLvInfo[i].ToArray();
            }
        }
    }
}
