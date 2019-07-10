# IsoSorting
Isometric sorting system for Unity using ECS. 
You can sort by depth or by order in layer.

44000 sprites are sorted in ~12ms in parallel.

# Flow
![alt text](https://zupimages.net/up/19/28/dpxr.png)

#Sorting by depth or by order by layer
Sorting by default is based on order in layer.
If you want use depth sorting, add ```SORTBYDEPTH``` to your compilation symbol

#IsoTool
You can convert dynamically gameobject from IsoTool to IsoSorting via the script ConvertIsoObjectManager.
For that, Add to compilation symbol ```ISOTOOL``` and add ConvertIsoObjectManager to a gameobject in the scene and play.
