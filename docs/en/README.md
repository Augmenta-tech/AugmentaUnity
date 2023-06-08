---
description: Augmenta Unity library and examples
---

# Getting started with Unity

### Augmenta for Unity

Unity examples using the [Augmenta-Unity](https://github.com/theoriz/augmentaunity) library created by [Théoriz](http://www.theoriz.com/en/).

### Installation

#### Unity asset store

[https://assetstore.unity.com/packages/tools/integration/augmenta-for-unity-206658](https://assetstore.unity.com/packages/tools/integration/augmenta-for-unity-206658)

### Quickstart

To start developping your application you probably need Augmenta data.

> 👋 If you do not have an Augmenta node ready, you can use our [People Moving Simulator](http://localhost:5000/o/WMYiWQEgbBaNqcMYxuGj/s/ckwiO6YnYe34GTO1xUnh/).

* Open your Unity scene.
* Drop the Augmenta prefab (from Assets/Plugins/Augmenta/Prefabs) in it.
* Set the input port in the AugmentaManager script of the Augmenta prefab to your protocol port.
* Run the scene.
* You should see gizmos of your Augmenta scene and persons in the scene view. You can enable or disable debug objects with the Show Debug option of the AugmentaManager.



### Known Issues

There is an [issue](https://github.com/keijiro/KlakNDI/issues/130) with Klak NDI that may cause an error when importing.

To fix it, you can try switching between the different Api Compatibility Level in Project Settings/Player/Other Settings/Configuration.

You can also remove the Klak NDI package and the 11 - FusionNDI folder if you do not intend to use NDI.
