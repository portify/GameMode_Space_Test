exec("./support/hit_region.cs");
exec("./support/types.cs");
exec("./support/json.cs");

exec("./scripts/hats.cs");
exec("./scripts/names.cs");
exec("./scripts/intro.cs");
exec("./scripts/jobs.cs");
exec("./scripts/round.cs");
exec("./scripts/chat.cs");
exec("./scripts/character.cs");
exec("./scripts/player.cs");
exec("./scripts/blood.cs");
exec("./scripts/damage.cs");
exec("./scripts/footsteps.cs");
exec("./scripts/doors.cs");
exec("./scripts/item_support.cs");
exec("./scripts/events.cs");
exec("./scripts/access_flags.cs");
exec("./scripts/items/id.cs");
exec("./scripts/items/flashlight.cs");

function serverCmdAlive(%client)
{
	if (!%client.isAdmin)
		return;

	%round = $DefaultMiniGame.spaceRound;

	if (!isObject(%round))
	{
		messageClient(%client, '', "\c6No round is running.");
		return;
	}

	%count = %round.playerSet.getCount();
	messageClient(%client, '', "\c6Players left alive:");

	for (%i = 0; %i < %count; %i++)
	{
		%player = %round.playerSet.getObject(%i);
		messageClient(%client, '', "  \c3" @ %player.character.realName SPC "(" @ %player.clientName @ " - " @ %player.clientBLID @ ")" @ (%player.client ? " - On the server" : " - Not on the server"));
	}
}

function serverCmdWho(%client)
{
	if (!%client.isAdmin)
		return;

	%round = $DefaultMiniGame.spaceRound;

	if (!isObject(%round))
	{
		messageClient(%client, '', "\c6No round is running.");
		return;
	}

	%count = %round.playerSet.getCount();
	messageClient(%client, '', "\c6Players left alive:");

	for (%i = 0; %i < %count; %i++)
	{
		%player = %round.playerSet.getObject(%i);
		messageClient(%client, '', "  \c3" @ %player.character.realName SPC "(" @ %player.clientName @ " - " @ %player.clientBLID @ ")" @ (%player.client ? " - On the server" : " - Not on the server"));
	}
}