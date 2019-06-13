namespace ProjectZ.Component.Setting
{
    public static class Setting
    {
        public const int NeedLvCount = 6;
        // Max State/Stock Factor
        public const int MaxWater      = 10;
        public const int MaxFood       = 10;
        public const int MaxHungry     = 100;
        public const int MaxThirsty    = 100;
        public const int MaxSleepiness = 100;
        public const int MaxStamina    = 100;

        public const int DrinkGainThirsty = -10;
        public const int DrinkCostWater   = 1;
        public const int EatGainHungry    = -10;

        public const int EatCostFood = 1;

        // Time
        public const int DefaultElapsedFactor    = 15;
        public const int DefaultTimeElapsedSpeed = 30;

        // CoolDown
        public const int EatCoolDownInMinute      = 1;
        public const int DrinkCoolDownInMinute    = 1;
        public const int GetWaterCoolDownInMinute = 1;
        public const int HuntCoolDownInMinute     = 1;
//public static Dictionary<>
    }
}