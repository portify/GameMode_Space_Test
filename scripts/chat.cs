datablock AudioProfile(ChatSound)
{
	fileName = "Add-Ons/GameMode_Space_Test/sounds/chat.wav";
	description = AudioDefault3D;
	preload = 1;
};

datablock ItemData(ChatItem)
{
	shapeFile = "base/data/shapes/empty.dts";
	gravityMod = 0;
};

function ChatItem::onPickup() {}

function spawnChatItem(%position, %text, %dist, %color, %lift, %time)
{
	if (%color $= "")
		%color = "1 1 1";

	if (%lift $= "")
		%lift = 0.6;

	if (%time $= "")
		%time = 1750;

	%item = new Item()
	{
		datablock = ChatItem;
	};

	MissionCleanup.add(%item);

	%item.setTransform(%position);
	%item.setVelocity("0 0" SPC %lift);

	%item.setShapeName(%text);
	%item.setShapeNameColor(%color);
	%item.setShapeNameDistance(%dist);

	%item.schedule(%time, "delete");
	return %item;
}

function getWallsBetween(%pos, %end)
{
	%count = 0;

	while (vectorDist(%pos, %end) > 1 && %count < 32)
	{
		%ray = containerRayCast(%pos, %end, $TypeMasks::FxBrickObjectType, %exempt);

		if (!%ray)
			break;

		%count++;

		%exempt = getWord(%ray, 0);
		%pos = getWords(%ray, 1, 3);
	}

	return %count;
}

function scrambleText(%text, %factor, %replace, %retain)
{
	%length = strlen(%text);

	for (%j = 0; %j < %length; %j++)
	{
		%char = getSubStr(%text, %j, 1);

		if ((%retain !$= "" && strpos(%retain, %char) != -1) || getRandom() > %factor)
			%result = %result @ %char;
		else
			%result = %result @ %replace;
	}

	return %result;
}

function linkify(%text)
{
	%count = getWordCount(%text);

	for (%i = 0; %i < %count; %i++)
	{
		%word = getWord(%text, %i);

		if (getSubStr(%word, 0, 7) $= "http://")
		{
			%url = getSubStr(%text, 7, strlen(%word));
			%text = setWord(%text, %i, "<spush><a:" @ %url @ ">" @ %word @ "</a><spop>");
		}
	}

	return %text;
}

function deadChat(%client, %text)
{
	%round = $DefaultMiniGame.spaceRound;

	if (%round.joined[%client.getBLID()])
	{
		%name = %round.name[%client.getBLID()];
		echo("(Dead) " @ %client.getPlayerName() @ " (" @ %name @ "): " @ %text);
	}
	else
	{
		%name = %client.getPlayerName();
		echo("(Dead) " @ %name @ ": " @ %text);
	}

	%text = "<color:CC7777>(Dead) <color:AA4444>" @ %name @ "<color:FFAAAA>: " @ %text;

	for (%i = 0; %i < $DefaultMiniGame.numMembers; %i++)
	{
		%other = $DefaultMiniGame.member[%i];

		if (%other.spaceReady && !isObject(%other.player))
			messageClient(%other, '', %text);
	}
}

function serverCmdMe(%client,
	%a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9,
	%a10, %a11, %a12, %a13, %a14, %a15, %a16, %a17, %a18)
{
	if (!%client.isInSpaceGame() || !isObject(%player = %client.player))
		return;

	%text = trim(stripMLControlChars(
		%a0 SPC %a1 SPC %a2 SPC %a3 SPC %a4 SPC %a5 SPC %a6 SPC %a7 SPC %a8 SPC %a9 SPC
		%a10 SPC %a11 SPC %a12 SPC %a13 SPC %a14 SPC %a15 SPC %a16 SPC %a17 SPC %a18));

	if (%text $= "")
		return;

	if ($Sim::Time - %client.lastChat <= 1)
		return;

	%client.lastChat = $Sim::Time;

	if (isObject(%client.player.character))
		%name = %client.player.character.realName;
	else
		%name = "Somebody";

	for (%i = 0; %i < $DefaultMiniGame.numMembers; %i++)
	{
		%other = $DefaultMiniGame.member[%i];

		if (!%other.spaceReady)
			continue;

		// dead people hear all
		if (!isObject(%other.player))
		{
			messageClient(%other, '', "<color:AADDAA>" @ %name @ " \c6" @ %text);
			continue;
		}

		%a = %client.player.getEyePoint();
		%b = %other.player.getEyePoint();

		%distance = vectorDist(%a, %b) + 12 * getWallsBetween(%a, %b);

		if (%distance > 35)
			continue;

		messageClient(%other, '', "<color:AADDAA>" @ %name SPC %text);
	}
}

package SpaceChatPackage
{
	function serverCmdSit(%client)
	{
		Parent::serverCmdSit(%client);
		serverCmdMe(%client, "sits down");
	}
	function serverCmdStartTalking()
	{
	}

	function serverCmdMessageSent(%client, %text)
	{
		if (%client.isSuperAdmin && getSubStr(%text, 0, 1) $= "$")
		{
			%text = getSubStr(%text, 1, strLen(%text));
			eval(%text);
			messageAll('', "\c3" @ %client.getPlayerName() @ " \c6=> " @ %text);
			return;
		}
		
		if (!%client.isInSpaceGame())
		{
			serverCmdTeamMessageSent(%client, %text);
			return;
		}

		%text = trim(stripMLControlChars(%text));

		if (%text $= "")
			return;

		if ($Sim::Time - %client.lastChat <= 1)
			return;

		%client.lastChat = $Sim::Time;

		if (!isObject(%client.player))
		{
			deadChat(%client, %text);
			return;
		}

		%first = getSubStr(%text, 0, 1);

		if (%first $= "#")
		{
			%text = ltrim(getSubStr(%text, 1, strlen(%text)));
			%verb = "whispers";

			%dist_normal = 6;
			%dist_cutoff = 6;
			%wall_effect = 16;
		}
		else if (%first $= "!")
		{
			%text = ltrim(getSubStr(%text, 1, strlen(%text)));
			%verb = "yells";

			%dist_normal = 48;
			%dist_cutoff = 128;
			%wall_effect = 6;
		}
		else if (strcmp(%text, strupr(%text)) == 0 && strcmp(strlwr(%text), strupr(%text)) != 0)
		{
			%verb = "yells";

			%dist_normal = 48;
			%dist_cutoff = 128;
			%wall_effect = 6;
		}
		else
		{
			%verb = "says";

			%dist_normal = 32;
			%dist_cutoff = 96;
			%wall_effect = 8;
		}

		if (%text $= "")
			return;

		if (%verb !$= "whispers")
		{
			serverPlay3D(ChatSound, %client.player.getHackPosition());
			spawnChatItem(
				vectorAdd(%client.player.getPosition(), "0 0 2.5"),
				%text, %dist_normal,
				%verb $= "yells" ? "0.9 0.7 0.2" : "0.98 1 0.96");
		}

		if (isObject(%client.player.character))
			%name = %client.player.character.realName;
		else
			%name = "Somebody";

		echo(%client.getPlayerName() @ " (" @ %name @ "): " @ %text);

		for (%i = 0; %i < $DefaultMiniGame.numMembers; %i++)
		{
			%other = $DefaultMiniGame.member[%i];

			if (!%other.spaceReady)
				continue;

			// dead people hear all
			if (!isObject(%other.player))
			{
				messageClient(%other, '', "<color:AADDAA>" @ %name @ " <color:777777>" @ %verb @ ", \c6'" @ %text @ "'");
				continue;
			}

			%a = %client.player.getEyePoint();
			%b = %other.player.getEyePoint();

			%distance = vectorDist(%a, %b) + %wall_effect * getWallsBetween(%a, %b);

			if (%distance > %dist_normal + %dist_cutoff)
				continue;

			if (%distance > %dist_normal)
			{
				if (%verb $= "whispers")
				{
					messageClient(%other, '', "<color:AADDAA>" @ %name @ " <color:777777>whispers something");
					continue;
				}

				%factor = (%distance - %dist_normal) / %dist_cutoff;
				%newText = scrambleText(%text, %factor, "#", " .,!?");

				if (%factor > 0.25)
					%newName = "Somebody";
				else
					%newName = %name;

				if (%factor > 0.75)
					messageClient(%other, '', "<color:336633>" @ %newName @ " <color:333333>" @ %verb @ ", <color:666666>'" @ %newText @ "'");
				else if (%factor > 0.5)
					messageClient(%other, '', "<color:558855>" @ %newName @ " <color:444444>" @ %verb @ ", <color:888888>'" @ %newText @ "'");
				else if (%factor > 0.25)
					messageClient(%other, '', "<color:77AA77>" @ %newName @ " <color:555555>" @ %verb @ ", <color:AAAAAA>'" @ %newText @ "'");
				else
					messageClient(%other, '', "<color:AADDAA>" @ %newName @ " <color:777777>" @ %verb @ ", \c6'" @ %newText @ "'");
			}
			else
				messageClient(%other, '', "<color:AADDAA>" @ %name @ " <color:777777>" @ %verb @ ", \c6'" @ %text @ "'");
		}
	}

	function serverCmdTeamMessageSent(%client, %text)
	{
		%text = trim(stripMLControlChars(%text));

		if (%text $= "")
			return;

		echo("(OOC) " @ %client.getPlayerName() @ ": " @ %text);
		%text = linkify(%text);
		%color = "AAAAFF";

		if (%client.isAdmin)
			%color = "FF44FF";

		messageAll('', "<color:CCCCFF>(OOC) <color:" @ %color @ ">" @ %client.getPlayerName() @ "<color:CCCCFF>: " @ %text);
	}
};

activatePAckage("SpaceChatPackage");