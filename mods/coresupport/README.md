Author: ns  
https://nssm.me

Version: 1.1

# Core support functionality

Adds support for server commands, sent in chat starting with `/`.
The client side will try to parse commands and send them instead of messages.
As a fallback, the server side will do the same for messages
to support clients that do not have the mod.

Using commands looks like this:

`/Example 25 Evan "Hello World"`

You can get a list of all registered commands with `/Help`.

## Server commands

#### `/Help`
Display a list of commands available on the server.

#### `/Find <Target>` (admin only)
Teleport yourself to somebody else.

#### `/Fetch <Target>` (admin only)
Teleport someone else to yourself.

## Developers

### Registering commands

```csharp
registerServerCommand(
	name: string,
	desc: string,
	args: string,
	flags: string,
)
```

`args` is a newline-separated list of expected arguments. Each line should be a tab-separated list of fields, where the first field is the argument name, the second is the type (currently unused), and any others are argument flags. The only argument flag currently recognized is `optional`.

`flags` is a newline-separated list of extra options for the command. The only flag currently recognized is `adminonly`.

```csharp
registerServerCommand(
	"Example", // name
	"Does an example.", // desc
	// args
	"Speed" NL "Target\tplayername" NL "Secret\tbool\toptional",
	"adminonly" // flags
);
```

### Utility functions

#### `findClient(name: string) -> GameConnection`

Finds a client whose name matches or contains (case-insensitive) `name`.

#### `getClientPlayer(client: GameConnection) -> Player`

Returns the effective player (a real player, or their ghost) for a client.

## Changelog

### v1.1

* Added `/Find` command.
* Added `/Fetch` command.
* Added `getClientPlayer` utility function.
