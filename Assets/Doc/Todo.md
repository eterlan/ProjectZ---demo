### Todo

-   一句话形容四个Inventory的关系以及作用
    -   InventorySlot
        -   表示小格，包含Item和图片等鼠标交互产生的行为，属于最上层应用
    -   InventoryPanel
        -   
    -   InventoryGrid
    -   Inventory
        -   提供物品栏的创建与检测合法性函数

-   怎么换位的？

### Note

-   ^  ==  !=
-   多个参数的函数几乎只有API，也就是说小号书很多都只做一件事
-   判断是否在界内只用上下左右四个点判断即可
-   写的时候试试自然语言组织逻辑，也就是说先写出来希望最后是如何使用API的，再去构思API该怎么写。
-   可以把需要的几个东西用struct组织起来
-   除非不同层抽象，不然不应该把行为全部拆成不用的函数，这样反而不好。
-   底层函数，为了便于理解与服用，宁愿多用参数也不应使用过于复杂的数据结构？( not sure )
-   Toggle可以写成 bool x ^= true
-   方法