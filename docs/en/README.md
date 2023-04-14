---
description: Augmenta Unity library and examples
---

# Getting started with Unity

### Augmenta for Unity

Unity examples using the [Augmenta-Unity](https://github.com/theoriz/augmentaunity) library created by [Théoriz](http://www.theoriz.com/en/).

### Installation

#### Unity asset store

[https://assetstore.unity.com/packages/tools/integration/augmenta-for-unity-206658](https://assetstore.unity.com/packages/tools/integration/augmenta-for-unity-206658)

#### Git User

* Create a new [Unity](https://unity3d.com/fr) project and git it.
* `$git submodule update --init --recursive` to pull everything.

#### Non Git User

* Create a new [Unity](https://unity3d.com/fr) project.
* Download zip and unzip this project in `*ProjectFolder*`.
* Download zip and unzip [Augmenta Unity](https://github.com/Theoriz/AugmentaUnity) in `*ProjectFolder*/Assets/Plugins/Augmenta/`.
* Download zip and unzip [Shared-Texture-Unity](https://github.com/Theoriz/Shared-Texture-Unity) in `*ProjectFolder*/Assets/Plugins/SharedTextureUnity/`.

### How to Use

#### Setup

To start developping your application you probably need Augmenta data.

> 👋 If you do not have an Augmenta node ready, you can use our [Augmenta simulator](../../doc/broken-reference/).

* Open your Unity scene.
* Drop the Augmenta prefab (from Assets/Plugins/Augmenta/Prefabs) in it.
* Set the input port in the AugmentaManager script of the Augmenta prefab to your protocol port.
* Run the scene.
* You should see gizmos of your Augmenta scene and persons in the scene view. You can enable or disable debug objects with the Show Debug option of the AugmentaManager.

#### Using Custom Object Prefabs

To instantiate your own prefab on each Augmenta object, add your prefab to the Custom Object Prefab parameter of the Augmenta Manager.

You can change this prefab at runtime by calling the function `ChangeCustomObjectPrefab(GameObject newPrefab)` of the Augmenta Manager.

**Using Custom Behaviours**

You can implement custom spawn and destroy behaviours for your custom Augmenta objects by implementing the IAugmentaObjectBehaviour interface in a script of your object. If you do, its Spawn function will be called when the object is instantiated, and its Destroy function will be called when the object should be destroyed (i.e. when the corresponding AugmentaObject is destroyed).

Note that if you implement the IAugmentaObjectBehaviour interface, the AugmentaObject will _NOT_ destroy your object when it destroys itself, instead it will call the Destroy function of the interface. You should handle the destruction of the custom object yourself in the Destroy() function of the interface.

An example use of the custom behaviours is shown in scene 10 - AugmentaObjectBehaviour.

#### Using Several Augmenta Streams

You can receive different Augmenta streams in the same Unity application as long as they are not on the same OSC port. You need to add an Augmenta prefab (i.e. AugmentaManager) for each incoming stream, then set each AugmentaManager ID and input port to listen to each protocol.

### Example Scenes

#### 0 - Minimalist

The simplest example using only the AugmentaManager to parse incoming Augmenta data and expose them to Unity.

<figure><img src="https://camo.githubusercontent.com/9c7ccdfabc555f5bb124ccbac73672232e539547c8c6ddfa9a82133070fbd438/68747470733a2f2f6d656469612e67697068792e636f6d2f6d656469612f59314d6a4152414638634d75324f6558466e2f67697068792e676966" alt=""><figcaption></figcaption></figure>

#### 1 - AugmentaSceneToSpout

In this example, an Augmenta Scene Camera is used to always render exactly the Augmenta Scene. The resulting texture is sent via Spout to be used in an external software.

<figure><img src="https://camo.githubusercontent.com/7eb1cb63f361c1e73628f6e0234685f4539ffd19928a9551d5017cf1357800f1/68747470733a2f2f6d656469612e67697068792e636f6d2f6d656469612f6947396d336b505475354e775a67707135542f67697068792e676966" alt=""><figcaption></figcaption></figure>

#### 2 - AugmentaMPSCounter

This example analyzes the incoming Augmenta messages rate to compute an estimation of the number of messages received per second.

<figure><img src="https://camo.githubusercontent.com/cbdbf5d80fe83ec6059f031494cd9503ac8672e9dfd8aac6dc95149267fc5135/68747470733a2f2f6d656469612e67697068792e636f6d2f6d656469612f596c66346356585077367545774f6935646d2f67697068792e676966" alt=""><figcaption></figcaption></figure>

#### 3 - AugmentaVideoOutput

This example shows how to use an Augmenta Video Output along with an Augmenta Video Output Camera to always render exactly the video texture area sent by [Fusion](https://augmenta-tech.com/download/#fusion). The resulting texture is sent via Spout and shown on a debug quad in the scene.

In this workflow, the field of view of the camera is computed to always match exactly the Video Output area.

<figure><img src="https://camo.githubusercontent.com/ccf8a6a15f6ee88938a9d9a542407116f385d8f4dda826f70c703a40f19c264e/68747470733a2f2f6d656469612e67697068792e636f6d2f6d656469612f6c53367a434f77394670393956346c62384f2f67697068792e676966" alt=""><figcaption></figcaption></figure>

#### 4 - AugmentaVideoOutputFromExternalCamera

This example shows how to use an Augmenta Video Output along with any camera in the scene to render the intersection of the video texture area sent by [Fusion](https://augmenta-tech.com/download/#fusion) and the camera's field of view. The resulting texture is sent via Spout and shown on a debug quad in the scene.

In this workflow, the field of view of the camera is fixed and color padding is added to the output video texture to match the desired texture resolution.

<figure><img src="https://camo.githubusercontent.com/0aadba22e88fcaea06b2179e047bc48d4b27ba766085be3038f8f0b768fe868a/68747470733a2f2f6d656469612e67697068792e636f6d2f6d656469612f656b3463336c44496a4955627158354f42662f67697068792e676966" alt=""><figcaption></figcaption></figure>

#### 5 - SeveralAugmentaScenes

In this example, two different Augmenta streams are received on two different ports and placed in the scene to simulate the usecase of an interactive floor and an interactive wall used together in the same scene.

<figure><img src="https://camo.githubusercontent.com/4f67780b0ad7eab3d5dd60a9a57c37c7a07d53ed5006061aa71466fa5d4ffb0f/68747470733a2f2f6d656469612e67697068792e636f6d2f6d656469612f686f795a77385a4d354b564c475437736d362f67697068792e676966" alt=""><figcaption></figcaption></figure>

#### 6 - AugmentaToGameObject

In this example, a custom object prefab is used to make a simple scene with squirrels react to Augmenta persons.

<figure><img src="https://camo.githubusercontent.com/666fbfac0ff83c32e2e94920d38c66785febc5cdd0e9e02ca172f0ff7c3df8bf/68747470733a2f2f6d656469612e67697068792e636f6d2f6d656469612f506c793154434b763873744c49474e6543742f67697068792e676966" alt=""><figcaption></figcaption></figure>

#### 7 - AugmentaToShader

In this example, the Augmenta person data is send to a ripple shader in order to have the shader creates ripples under the Augmenta persons.

<figure><img src="https://camo.githubusercontent.com/bb7b7252810d8caaa72da9d2ef8266d4c3a48518c1cc23dc96832dfaf959085e/68747470733a2f2f6d656469612e67697068792e636f6d2f6d656469612f694b47786f3177353933474b4b565245586b2f67697068792e676966" alt=""><figcaption></figcaption></figure>

#### 8 - AugmentaToVFXGraph

In this example, the Augmenta person data is send to a VFXGraph in order to make the sand particle react to the oldest 3 persons in the scene.

<figure><img src="https://camo.githubusercontent.com/dd56905d85db4df2908cf1ee2f8fb701f6675a428d0f7948748555b2d2b5706c/68747470733a2f2f6d656469612e67697068792e636f6d2f6d656469612f6b633731466d556745496737656c525464372f67697068792e676966" alt=""><figcaption></figcaption></figure>

#### 9 - FusionSpout

In this example, the FusionSpout prefab is used to display a Spout coming from Augmenta Fusion on a quad fitted to an AugmentaVideoOutput.

<figure><img src="https://camo.githubusercontent.com/a2e5b529581492e2c1ab41faa4c5e9b74efaac94de33c1ac6f3fc686804b5904/68747470733a2f2f6d656469612e67697068792e636f6d2f6d656469612f326536576b76676332383442786839345a592f67697068792e676966" alt=""><figcaption></figcaption></figure>

#### 10 - AugmentaObjectBehaviour

In this example, the IAugmentaObjectBehaviour interface is used in the custom object prefab to fade in and out a sphere rotating around each AugmentaObject.

<figure><img src="https://camo.githubusercontent.com/0bf4710a191a2c432d50e7a4e2f92655478f08d54728673798dd9eff7e8e0a33/68747470733a2f2f6d656469612e67697068792e636f6d2f6d656469612f7a354a59753437354d4b70513059466d56432f67697068792e676966" alt=""><figcaption></figcaption></figure>

#### 11 - FusionNDI

In this example, the FusionNDI prefab is used to display an NDI coming from Augmenta Fusion on a quad fitted to an AugmentaVideoOutput.

<figure><img src="https://camo.githubusercontent.com/a2e5b529581492e2c1ab41faa4c5e9b74efaac94de33c1ac6f3fc686804b5904/68747470733a2f2f6d656469612e67697068792e636f6d2f6d656469612f326536576b76676332383442786839345a592f67697068792e676966" alt=""><figcaption></figcaption></figure>

#### 12 - ImmersiveSpace

In this example, an existing immersive space 3D model is imported and the Augmenta areas are mapped to the floor and walls of the space. The Fusion file for this space can be found in the Fusion folder of this scene.

<figure><img src="https://camo.githubusercontent.com/4ba018701062a9e9404e62ff0641f51709dc9dddd198651bd0b4476ef43b5be7/68747470733a2f2f6d656469612e67697068792e636f6d2f6d656469612f32704b48387a766a4471796653436b6273582f67697068792e676966" alt=""><figcaption></figcaption></figure>

### Known Issues

There is an [issue](https://github.com/keijiro/KlakNDI/issues/130) with Klak NDI that may cause an error when importing.

To fix it, you can try switching between the different Api Compatibility Level in Project Settings/Player/Other Settings/Configuration.

You can also remove the Klak NDI package and the 11 - FusionNDI folder if you do not intend to use NDI.

### Last Tested Unity Version

Unity 2021.2.17f1
