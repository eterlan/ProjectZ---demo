namespace ProjectZ.Component.Setting
{
    /// <summary>
    /// Keep Alive 
    /// </summary>
    public enum Lv1Behaviour
    {
        Attack,
        Escape,
    }
    /// <summary>
    /// Physiological need 
    /// </summary>
    public enum Lv2Behaviour
    {
        Eat,
        Dring,
        Sleep,
        Heating,
        Heal,
        Trade,
        Steal,
    }
    /// <summary>
    /// Safety need
    /// </summary>
    public enum Lv3Behaviour
    {
        Check,
        Suspect,
        Bypass,
        Respect,
        讨好,
    }
    /// <summary>
    /// Entertain need
    /// </summary>
    public enum Lv4Behaviour
    {
        Play,
        Sing,
        Wander,
        Chat
    }
    /// <summary>
    /// Recognition
    /// </summary>
    public enum Lv5Behaviour
    {
        Chat,
        Love,
        BeRespect,
    }
    /// <summary>
    /// 
    /// </summary>
    public enum Lv6Behaviour
    {
        Help,
        Build,
        Work,
    }
    /// <summary>
    /// 根据不同的目标物，进行不同的寻路
    /// </summary>
    public enum TargetType
    {
        Around,
        House,
        Tree,
        River,
    }
}