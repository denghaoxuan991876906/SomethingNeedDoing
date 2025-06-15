## Something Need Doing

This plugin is an expansion of the native macro system in FFXIV. Unlimited slots, unlimited length, more commands, more modifiers. It also has a lua engine along with many modules for automating much of the game.

Repo:
```
https://puni.sh/api/repository/croizat
```

### Macros and Scripting

Scripts are currently only available in lua (hopefully more engines to come soon). For access to in-game functions, navigate to the `Help` tab in the main window, then the `Lua` tab to see all available functions. They are automatically generated so that will always contain the full list, along with descriptions and examples.

Both native macros and scripts can be run at any time, via the ui for via command, but they can also be set to run on certain [Trigger Events](https://github.com/Jaksuhn/SomethingNeedDoing/blob/05b8780130cb62835222318f7cef0ced04d4f8a0/SomethingNeedDoing/Core/Enums.cs#L57-L70). Scripts specifically can also have function-level trigger events, by naming the functions after the event.

```lua
function loop()
    for i = 1, 5 do
        yield("/e loop counter: ".. i)
        yield("/wait " .. 1)
    end
end

function OnTerritoryChange() -- This will be automatically called only while the script is running!
    yield("/e changed territory!")
end

loop()
```

### Github Integration

All macros support linking to a specific file on github. Simply add a link to the file in the metadata and it will be automatically downloaded and checked for updates on start up.

### Dependencies and conflicts

Also in the metadata is the ability for macros to depend on other macros, be they remote or local. You can also set plugin dependencies as well as plugin conflicts, and dependencies will be checked on macro start, and conflicting plugins will be disabled during run time.
