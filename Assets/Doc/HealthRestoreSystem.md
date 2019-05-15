```C#
public class HealthRestoreSystem : ComponentSystem
{
		// Filter = playerState && restoreHealthEventData;
		Entities.WithAll((ref PlayerState playerState, ref RestoreHealthEventData restoreHealthEventData)=>
		{
      if (!restoreHealthEventData.instant)
        {
            while (count < restoreHealthEventData.count)
            {
                // 生效间隔, 生效次数, 治疗量.
                coolDown -= deltaTime;
                if (coolDown < restoreHealthEventData.coolDown)
                {
                    playerState.Hp += restoreHealthEventData.Value;
                    count++;
                    coolDown = restoreHealthEventData.coolDown;
                }
                else
                {
                		playerState.Hp += restoreHealthEventData.Value;
                }
            }
        }
		});
}

public struct RestoreHealthEventData : IComponentData
{
		// 每跳hot的间隔时间
		public int CoolDown;
		// hot有几跳
		public int Count;
		// 每跳治疗量
		public int Value;
		// 一次性还是hot
		public bool instant;
}




// when press the item icon on UI
public class UIEventSender : MonoBehaviour
{
	private Entity item;
	private int cellLength = 5;
	public UIElement ui;
	// get Entity 
	// Mouse Position - leftTop corner of the first Item
	if (Input.GetMouseButtonDown(0))
	{
		// 屏幕坐标系
		var position = Input.mousePosition;
		var leftTop = new float2(ui.bound.center.x - ui.bound.extent.x, ui.bound.center.y + ui.bound.extent.y;
		var entityRelativePos = new float2(position.x - leftTop.x, leftTop.y - position.y);
		var xIndex = entityRelativePos.x / cellLength;
		var yIndex = entityRelativePos.y / cellLength;
		var indexOnUI = xIndex + yIndex * rowCount;
		var items = EntityManager.GetBuffer<Items>(player).AsNativeArray();
		for (int i = 0; i < items.Length(); i++)
		{
			if (items[i].IndexOnUI == indexOnUI)
			{
				item = items[i].Value;
			}
		}
		
	}
    public void OnUseItem()
    {
        var itemType = EntityManager.GetComponentData<ItemType>(item).Value;
        switch (itemType)
        {
            case ItemTypes.Med :
            {
                var medIndex = (int)EntityManager.GetComponentData<MedType>(item).Value;
                EntityManager.CreateEntity(new restoreHealthEventData
                                           {
                                              instant = MedValue[medIndex].Instant,
                                              Value = MedValue[medIndex].Value,
                                              Value = MedValue[medIndex].CoolDown,
                                              Value = MedValue[medIndex].Count,
                                           });
                break;
            }
            case ItemTypes.Food :
            {
                var medIndex = (int)EntityManager.GetComponentData<MedType>(item).Value;
                EntityManager.CreateEntity(new restoreHungryEventData
                                           (
                                              Value = MedValue[medIndex].Value,
                                           });
                break;
            }
        }
    }
}
```

