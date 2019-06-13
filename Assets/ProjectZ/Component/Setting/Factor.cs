namespace ProjectZ.Component.Setting
{
    /// <summary>
    /// 影响判断的必需品储量
    /// </summary>
    public enum StockFactor
    {
        Food,
        Water,
        Medicine,
    }
    /// <summary>
    /// 人物所处的状态
    /// </summary>
    public enum StateFactor
    {
        Fighting,
        Hp,
        Mp,
        Hungriness,
        Thirsty,
        Tired,
        Sickness,
        Safety,
        Boredom,
        Solitude,
        被无视,
        找不到生存的意义,
    }
    /// <summary>
    /// How this Factor effect Behaviour? 
    /// </summary>
    public enum FactorMode
    {
        Direct,
        Inverse,
        Custom, // How?
    }
}