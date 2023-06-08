# Using Augmenta data in C\#

### Namespace

The Augmenta scripts are all in the namespace Augmenta, so you will need the following at the top of your custom script.

```
using Augmenta;
```

### Accessing the Augmenta scene and the Augmenta objects

You can access the Augmenta Scene and Augmenta Objects through the Augmenta Manager.\
For example the following code returns the Augmenta object with id 5 if it exists :

```
// Get the AugmentaObject with id 5
if(myAugmentaManager.augmentaObjects.ContainsKey(5)){
    AugmentaObject retrievedObject = myAugmentaManager.augmentaObjects[5];
} 
```

Both the AugmentaScene and AugmentaObject classes expose the raw Augmenta data of the scene and objects respectively. You can use them directly, but we recommend using the Unity space data from the Augmenta objects as it is more intuitive to use.\


### Using AugmentaObject data in Unity space (recommended)

The table below summarizes the available AugmentaObject properties exposed in Unity space, automatically converted from the Augmenta data.\


<table><thead><tr><th width="208">Property</th><th>Description</th></tr></thead><tbody><tr><td>worldPosition2D</td><td>Object center position on the Augmenta Scene plane, ignoring its height.</td></tr><tr><td>worldPosition3D</td><td>Object center position above the Augmenta Scene plane, taking its height into account.</td></tr><tr><td>worldScale</td><td>Object size in meters.</td></tr><tr><td>worldVelocity2D</td><td>Smoothed object velocity on the Augmenta Scene plane in meters per second.</td></tr><tr><td>worldVelocity3D</td><td>Smoothed object velocity in meters per second.</td></tr></tbody></table>



### Converting data from Augmenta to Unity (not recommended)

\
When using the Augmenta data directly, keep in mind that Augmenta and Unity does not use the same coordinate system.

Augmenta uses a 2D coordinate system where X is right and Y is down.\
Unity uses a 3D left-handed coordinate system where X is right, Y is up, and Z is forward.\


To convert from the Augmenta coordinates to Unity coordinates, the Augmenta scene is assumed to be a horizontal floor. This means the following axis conversion :&#x20;

* X in Augmenta remains X in Unity
* Y in Augmenta becomes -Z in Unity
* Z (height) in Augmenta becomes Y in Unity. This is only used for the objects height as the Augmenta scene is always assumed to be at height zero.
