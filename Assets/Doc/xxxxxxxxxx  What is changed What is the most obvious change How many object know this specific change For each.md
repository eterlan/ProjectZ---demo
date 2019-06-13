```C#
// What is changed? What is the most obvious change?
// How many object know this specific change?
// For each abstract layer, what's the change? What is accept as input, and output?
// Is the input from a same place as the output?
// Is these change happened among an unique type of object?
// When objects communicate with each other, what's the "brige"? What's the 
// 
// For each change, the data as input or output shouldn't know about each other. For the sake of extensible, which means, I could switch the input or output source to another member, by easily change the variable to the method of "Boss" - Manager class. The benefit of this philosophy, known as singleton, is to aviod dig into the "old" logic again, when introduce a new source, by keep the origin implementation unchanged.

// Every single design philosophy is to make sure least repeatness, both for code repeatness and times of reread.

// Too much rules is just limitation to your creativity. Be brave to fall, keep going with confidence, also keep some design philosophy in mind and think about better implementation, only when you feel like doing some refraction, it's the way to be creative.

@TODO 

- Q : What's the operation I need in the UI_InventoryPanel?
    - A: 
	1.Left Click -> Change to another cell/Swap with another item.
    2.Left Click -> Show item/equipment info
    3.Right Click -> Equip
        - 1.Depends on the status you click on an icon.
        2. Depends on the MousePosition you click.
        3. None.
        	- 1.OnClicked{
        // already select item, click empty cell or another itemImage.
        		if (!m_itemOnClicked exist)
                {
                    if (gameObject.hasComponent<item>)
                        // notImeplement
                    else if (gameObejct.hasComponent<cell>){
                        var cell = gameObject.GetComponent<Cell>();
                		MoveItem(m_itemOnClicked, cell);
                }
        // select item.
                else
                    ShowItemInfo();
                    
			- MoveItem(item, cell)
            {
             	var cellIndex = cell.index;
                if (CanStoreImage(index1d, imageSize))
                	item.ChangeOccupyCells(item);
                item.ClearCells(item);//Panel.Cells[item.cellIndex].SetEmpty // Not Implemented : 
				var size = item.size;
                var insertIndex = item.cellIndex[0];
           		if(canStoreImage(insertIndex, size))
                {
                    item.cellIndex = 
                }
            }
            
           - ChangeOccupyCells(Item item, int firstCellIndex){
               item.cells.Clear();
               item.cells.Insert(firstCellIndex);
           }
           - 
                    
                   
            
    
(- Instantiate Item with prefab, and set the position according to the index of cells it occupy.)
- When add a new item, the item should be able to add to the right place(in the grid).
    - Instantiate a prefab. Then set it's position according to the index of cells it occupy.
    var InstantiatePosition = 
- When add a new item, the item should be able to find the cloest place to occupy from TopRight corner.
    - I know which place I can occupy
    For(int i = 0; i < cells.Length();i++){
    	var cell = cell[i];
    	If(cells.IsOccupied)
        {
            If(cells[i+width].IsOccupied)
            {
                continue;
            }
        }
	}
- I can know which Cell is occupied.
- ForEach(ItemCell){
    If(ItemCell.IsOccupied)
}


```

