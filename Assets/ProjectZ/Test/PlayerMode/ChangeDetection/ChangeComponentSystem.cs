using Unity.Entities;

namespace ProjectZ.Test.PlayerMode.ChangeDetection
{
    public class ChangeComponentSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
        }

        public void RW()
        {
            Entities.ForEach((ref ForChangeTestComponent c0) => { });
        }

        public void RO()
        {
            Entities.WithAllReadOnly<ForChangeTestComponent>().ForEach((ref ForChangeTestComponent c0) => { });
        }

        public void W()
        {
            Entities.ForEach((ref ForChangeTestComponent c0) => { c0.Value += 1; });
        }

        protected override void OnCreate()
        {
        }

        protected override void OnDestroy()
        {
        }
    }
}