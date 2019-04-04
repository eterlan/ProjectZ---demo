public struct Timer
{
    public float elapsedTime;
    private float duration;
    private ByteBool running;
    private ByteBool started;
    
    public ByteBool Started
    {
        set { started = value; }
        get { return started; }
    }

    public float Duration
    {
        get { return duration; }
        set 
        {
            if ( value > 0 && !Running )
            {
                duration = value;                 
            }
        }
    }

    public ByteBool Finished
    {
        get { return Started && !Running; }
    }

    public ByteBool Running
    {
        get { return running; }
        set { running = value; }
    }

    


}