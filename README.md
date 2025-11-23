# ShuffleBagLib

`ShuffleBagLib` is a small, thread-safe C# library that provides a **shuffle bag** (a.k.a. random bag) collection.

A **shuffle bag** lets you randomly draw items without immediate repeats: every item in the bag is returned exactly once per cycle, in a randomized order. When all items have been drawn, the bag is reshuffled and drawing continues.

This library is:

- Implemented as a generic `ShuffleBag<T>`
- Pure .NET (no UnityEngine or Unity-specific types)
- Thread-safe via internal locking
- Compatible for direct use in **Unity 2022.3+** (when built as .NET Standard 2.1)

> You can consume `ShuffleBagLib.dll` **directly** in Unity; no wrapper DLL is required.

## Features

- Random, non-repeating draws until the bag is exhausted
- Automatic reshuffle when all items have been drawn
- Generic: works with any type `T` (ints, enums, strings, custom classes, etc.)
- Thread-safe: `Add` and `NextItem` can be safely called from multiple threads (subject to usual Unity main-thread rules)

## Requirements

- **Library target**: `.NET Standard 2.1`
- **Unity**: 2022.3 LTS or newer
  - **Api Compatibility Level** set to: `.NET Standard 2.1`
    - `Edit > Project Settings > Player > Other Settings > Api Compatibility Level`

## Building the DLL

1. Open **Visual Studio 2022/2026**.
2. Create a project:
   - Project type: **Class Library**
   - Target framework: **.NET Standard 2.1**
3. Add the `ShuffleBag<T>` class to the project (namespace: `ShuffleBagLib`).
4. Build the project:
   - This produces `ShuffleBagLib.dll` in `bin/Release` (or `bin/Debug`).

## Installing in Unity

1. In your Unity project, create a folder (if it does not already exist):
```text
Assets/Plugins
```
2. Copy ShuffleBagLib.dll into:
```
Assets/Plugins/ShuffleBagLib.dll
```
3. In Unity, let it recompile.

    You should now see ShuffleBagLib available as a namespace in C# scripts.

## Basic Usage in Unity

### Example: Integer Shuffle Bag

## Basic Usage in Unity

### Example: Integer Shuffle Bag

```cs
using UnityEngine;
using ShuffleBagLib;
public class ShuffleBagExample : MonoBehaviour
{
    private ShuffleBag<int> _shuffleBag;

    private void Awake()
    {
        _shuffleBag = new ShuffleBag<int>();
        _shuffleBag.Add(69);
        _shuffleBag.Add(70);
        _shuffleBag.Add(71);
        _shuffleBag.Add(72);
    }

    public void DebugItem()
    {
        if (_shuffleBag == null)
        {
            Debug.LogError("Shuffle bag is not initialized.");
            return;
        }

        var bagItem = _shuffleBag.NextItem();
        Debug.Log($"bagItem = {bagItem}");
    }
}
```

> You can hook `DebugItem()` to a UI button, an input event, or call it from your game logic whenever you need the next value.

* * *

### Example: String Shuffle Bag (e.g., Enemy Types)

```cs
using UnityEngine;
using ShuffleBagLib;

public class EnemyTypeSelector : MonoBehaviour
{
    private ShuffleBag<string> _enemyTypes;

    private void Awake()
    {
        _enemyTypes = new ShuffleBag<string>();
        
        _enemyTypes.Add("Grunt");
        _enemyTypes.Add("Shooter");
        _enemyTypes.Add("Tank");
        _enemyTypes.Add("Support");
    }
    
    public string GetNextEnemyType()
    {
        if (_enemyTypes == null)
        {
            Debug.LogError("Enemy type shuffle bag is not initialized.");
            return string.Empty;
        }
    
        var enemyType = _enemyTypes.NextItem();
        Debug.Log($"Next enemy type: {enemyType}");
        return enemyType;
    }
}
```

* * *

## Controlling Random Weighting When Seeding

`ShuffleBag<T>` does not use explicit weights in its API.  
Instead, **weighting is controlled by how many times you add a given item** when seeding the bag.

Think of each call to `Add(item)` as inserting one “ticket” for that item into the bag.  
More tickets for an item = higher chance of that item appearing in a cycle.

### Example: Simple Weighted Ints

You want:

- Value `1` to be twice as likely as `2`
- Value `2` to be twice as likely as `3`

You can seed the bag like this:

```cs
_shuffleBag.Add(1);
_shuffleBag.Add(1); // 1 has 2 tickets

_shuffleBag.Add(2);
_shuffleBag.Add(2); // 2 has 2 tickets

_shuffleBag.Add(3); // 3 has 1 ticket
```

Now, per full cycle:

- `1` appears 2 times
- `2` appears 2 times
- `3` appears 1 time

Across many cycles, the relative frequency will approximate a 2:2:1 ratio.

### Example: Weighted Enemy Types (60/30/10)

You want:

- Grunt: 60%
- Shooter: 30%
- Tank: 10%

Use a simple integer ratio, for example 6:3:1:

```cs
private void Awake()
{
    _enemyTypes = new ShuffleBag<string>();
    
    // Grunt (weight 6)
    _enemyTypes.Add("Grunt");
    _enemyTypes.Add("Grunt");
    _enemyTypes.Add("Grunt");
    _enemyTypes.Add("Grunt");
    _enemyTypes.Add("Grunt");
    _enemyTypes.Add("Grunt");
    
    // Shooter (weight 3)
    _enemyTypes.Add("Shooter");
    _enemyTypes.Add("Shooter");
    _enemyTypes.Add("Shooter");
    
    // Tank (weight 1)
    _enemyTypes.Add("Tank");}
```

Each cycle will contain:

- 6 Grunts
- 3 Shooters
- 1 Tank

Because the shuffle bag returns each entry exactly once per cycle in random order, this gives you weighted randomness **without immediate repeats** (beyond what the weights themselves imply).

### Example Helper: Seeding With “AddWeighted”

You can also write your own small helper to seed by weight:

```cs
private void AddWeighted(ShuffleBag<string> bag, string value, int weight)
{
    for (var i = 0; i < weight; i++)
    {
        bag.Add(value);
    }
}

private void Awake()
{
    _enemyTypes = new ShuffleBag<string>();
    AddWeighted(_enemyTypes, "Grunt", 6);
    AddWeighted(_enemyTypes, "Shooter", 3);
    AddWeighted(_enemyTypes, "Tank", 1);
}
```

> Note: This helper is just a convenience around the existing `Add(T item)` API.  
> The DLL itself does not need to change to support weighting.

* * *

## Thread Safety Notes

`ShuffleBag<T>` is thread-safe:

- All access to internal state (`_items`, `_currentIndex`, `_random`) is synchronized via a private lock.
- You can safely call `Add` and `NextItem` from multiple threads.

However, keep in mind Unity’s constraints:

- Do not call Unity APIs (`Debug.Log`, `Instantiate`, modifying scene objects, etc.) from background threads.
- Use `ShuffleBag<T>` on background threads only for pure data decisions, and apply results on the main thread.

Example pattern:

- Worker thread decides “next enemy type” via `ShuffleBag<string>`.
- Main thread reads the result and spawns the enemy in the scene.

* * *

## API Overview

### Namespace

`using ShuffleBagLib;`

### Class

`public class ShuffleBag<T>{ public ShuffleBag(); public void Add(T item); public T NextItem();}`

#### ShuffleBag()

Creates an empty shuffle bag.

#### Add(T item)

Adds an item to the bag.  
Calling `Add` multiple times with the same value effectively increases its weight in the randomized sequence.

#### T NextItem()

Returns the next randomized item.

Behavior:

- All items (including duplicates used as weights) are returned exactly once per cycle.
- When all entries have been drawn, the internal order is reshuffled and a new cycle begins.
- Throws `InvalidOperationException` if the bag is empty.

* * *

## Common Pitfalls

- **Empty Bag**

    - Calling `NextItem()` on an empty bag throws `InvalidOperationException`.
    - Ensure you call `Add` at least once before `NextItem`.
- **Uninitialized Reference**

    - Make sure you initialize the shuffle bag (e.g., in `Awake` or `Start`) before any other method uses it.
    - If accessing from other scripts, ensure you use proper references or getters.
- **Unity API from Background Thread**

    - `ShuffleBag<T>` is thread-safe, but Unity APIs are not.
    - Use the bag from background threads only for pure logic; marshal results back to the main thread before touching anything in the scene.

* * *

## License

Use freely in your projects.  
Attribution is appreciated but not required.