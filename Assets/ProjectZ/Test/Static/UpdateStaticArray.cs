using Unity.Entities;

namespace ProjectZ.Test.Static
{
    public class UpdateStaticArray : ComponentSystem
    {
        protected override void OnUpdate()
        {
            S.Values[0] += 3;
            Entities.ForEach((ref S s) =>
            {
                //S.Values[0] += 3;
            });
        }

        protected override void OnCreate()
        {
        }

        protected override void OnDestroy()
        {
        }
    }
}
