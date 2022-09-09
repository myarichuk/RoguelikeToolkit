# Note: this repository will not be updated due to
* Heavy refactorings for all of the projects
* Set up unified template for all sub-projects
* Splitting of the monorepo to separate projects (easier automated NuGet deployments)

# The What
This is a loosely coupled collection of tool libs I wrote to myself for a roguelike I hopefully will finish in the future. 

## RoguelikeToolkit.Entities
This is a library to allow resolve entity templates for Entity Component System in a similar way [Ultimate Adom](https://www.ultimate-adom.com/index.php/2018/10/25/making-ultimate-adom-moddable-by-using-entity-component-systems/) is doing.
Supports entity template inheritance and creation of entity hierarchies.  
At the moment there is hardcoded support for the awesome [DefaultEcs](https://github.com/Doraku/DefaultEcs) but it can be converted to support other systems.

So, for the following entity templates:

**object.json**
```json
{
  "Id": "object",
  "Inherits": [],
  "Components": {
    "Health": 100.0 ,
    "Weight": 0.0 
  }
}
```

**bodypart.json**
```json
{
  "Id": "bodypart",
  "Inherits": [ "object" ],
  "Components": {
    "Dirt": 0.0
  }
}
```

**actor.json**
```json
{
  "Id": "Actor",
  "LeftArm": {
    "Inherits": [ "bodyPart" ],
    "Components": { "Weight": 10.0 }
  },
  "RightArm": {
    "Inherits": [ "bodyPart" ],
    "Components": { "Weight": 10.0 }
  },
  "Components": {
    "Attributes": {
      "Strength": 5,
      "Agility": 7
    }
  }
}
```

You can resovle the *actor* template like this:
```cs
var templateCollection = new EntityTemplateCollection("Templates");
var entityFactory = new EntityFactory(new World(), templateCollection);

var success = entityFactory.TryCreateEntity("actor", out var actorEntity);
```

This code will instantiate a [DefaultEcs](https://github.com/Doraku/DefaultEcs) entity with the specified components and child-parent relationships.
A component such as ``Attributes`` should be specified like this
```cs
[Component(Name = "Attributes")]
public class Attributes
{
  public int Strength { get; set; }
  public int Agility;
}
```
