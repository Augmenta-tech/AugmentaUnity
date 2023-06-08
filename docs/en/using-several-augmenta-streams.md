# Using several Augmenta streams

You can receive different Augmenta streams in the same Unity application as long as they are not on the same OSC port :&#x20;

* Add an Augmenta prefab (i.e. AugmentaManager) for each incoming stream in your scene.
* Set a different ID on each AugmentaManager. For example "Floor" and "Wall".
* Set the input port of each AugmentaManager to the corresponding Augmenta stream port.
