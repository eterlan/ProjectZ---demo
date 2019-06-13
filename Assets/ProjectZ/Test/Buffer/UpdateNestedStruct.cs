using Unity.Entities;

namespace ProjectZ.Test.Buffer
{
    public class UpdateNestedStruct : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((ref T t) =>
            {
                t.point.X += 1;
                t.forTest += 1;            
            });
        }
    }
    /// <summary>
    /// How to Iterate Buffer? Can buffer contain more than one variables?
    /// </summary>
    public class UpdateBufferWithMultipleElement : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach ((DynamicBuffer<U> u)=>
            {
                var element = new U { Var1 = 1,Var2 = 2};
                u.Add(element);
            });
        }
    }

    /// <summary>
    /// How to modify existing element?
    /// </summary>
    public class ModifyExistingElement : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((DynamicBuffer<U> u) =>
            {
                var element = new U {Var2 = 1};
                u.Add(element);
                var temp = u[0];
                temp.Var2 += 1;
                u[0]      =  temp;
            
                // THIS DOESN'T WORK
                // u[0].Var1 = 2;
            });
        }
    }
}