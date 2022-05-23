# IsoSorting
Isometric sorting system for Unity using ECS. 
You can sort by depth or by order in layer.

EDIT : using an old version of Entities package and has a bug during some sorting cituation, feel free to commit a fix

44000 sprites are sorted in ~12ms in parallel.

# How to add to your project
- Open manifest.json at %RepertoryPath%/Packages
- Add ```"com.lennethproduction.isosorting": "https://github.com/Sylmerria/IsoSorting.git"``` in dependencies section

# Flow
![alt text](https://zupimages.net/up/19/28/dpxr.png)

# Sorting by depth or by order by layer
Sorting by default is based on order in layer.
If you want use depth sorting, add ```SORTBYDEPTH``` to your compilation symbol

# IsoTool
You can convert dynamically gameobject from IsoTool to IsoSorting via the script ConvertIsoObjectManager.
For that, Add to compilation symbol ```ISOTOOL``` and add ConvertIsoObjectManager to a gameobject in the scene and play.

# Help
You can talk and have help on this discord server : https://discord.gg/rFjXE9N
