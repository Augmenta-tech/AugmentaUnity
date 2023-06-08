# Using custom behaviours

You can implement custom spawn and destroy behaviours for your custom Augmenta objects by implementing the IAugmentaObjectBehaviour interface in a script of your object. If you do, its Spawn function will be called when the object is instantiated, and its Destroy function will be called when the object should be destroyed (i.e. when the corresponding AugmentaObject is destroyed).

Note that if you implement the IAugmentaObjectBehaviour interface, the AugmentaObject will _NOT_ destroy your object when it destroys itself, instead it will call the Destroy function of the interface. You should handle the destruction of the custom object yourself in the Destroy() function of the interface.

An example use of the custom behaviours is shown in scene 10 - AugmentaObjectBehaviour.
