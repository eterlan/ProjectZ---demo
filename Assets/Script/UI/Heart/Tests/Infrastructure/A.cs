namespace Tests.Infrastructure
{
    public static class A
    {
        public static HeartBuilder Heart()
        {
            return new HeartBuilder();
        }

        public static HeartContainerBuilder HeartContainerBuilder()
        {
            return new HeartContainerBuilder();
        }
    }
}