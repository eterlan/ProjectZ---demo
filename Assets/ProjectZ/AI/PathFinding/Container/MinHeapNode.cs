namespace ProjectZ.AI.PathFinding.Container
{
    public struct MinHeapNode
    {
        public MinHeapNode(int index, float expectedCost)
        {
            Index        = index;
            ExpectedCost = expectedCost;
            Next         = -1;
        }

        public int   Index        { get; } // TODO to position
        public float ExpectedCost { get; }
        public int   Next         { get; set; }
    }
}