function GameConnection::spaceTakeControl(%this, %player)
{
	commandToClient(%this, 'ClearCenterPrint');
	commandToClient(%this, 'ClearBottomPrint');

	if (%player.isCorpse)
	{
		%player.originalClient = %this;

		%this.setControlObject(%this.camera);
		%this.camera.setControlObject(%this.camera);
		%this.camera.setMode("Corpse", %player);
	}
	else
	{
		%this.player = %player;
		%player.client = %this;

		%player.clientName = %this.getPlayerName();
		%player.clientBLID = %this.getBLID();

		%this.setControlObject(%player);

		//%player.invInit();
	}
}

function GameConnection::spaceJoinRound(%this, %job)
{
	messageClient(%this, '', "\c6You are the " @ %job @ ".");
	messageClient(%this, '', "\c6As the " @ %job @ " you answer directly to NOBODY. Special circumstances may change this.");
	messageClient(%this, '', "\c6To speak on your departments radio, bug Port to add radios.");

	commandToClient(%this, 'ClearCenterPrint');
	commandToClient(%this, 'ClearBottomPrint');

	cancel(%this.spaceIntro);

	SpaceRound.joined[%blid = %this.getBLID()] = 1;

	if (isObject(%this.player))
		%this.player.delete();

	%spawnPoint = %this.getSpawnPoint(); // replace with job-specific

	%player = new Player()
	{
		datablock = PlayerSpaceArmor;
		client = %this;
		spawnTime = getSimTime();
		currWeaponSlot = -1;
		currTool = -1;
		canDismount = 1;

		clientName = %this.getPlayerName();
		clientBLID = %this.getBLID();

		spaceRound = SpaceRound;
	};

	if (!isObject(%player))
	{
		error("ERROR: FAILED TO CREATE PLAYER!");
		return;
	}

	MissionCleanup.add(%player);

	%player.setTransform(%spawnPoint);
	%player.setEnergyLevel(%player.getDataBlock().maxEnergy);

	commandToClient(%this, 'ShowEnergyBar', 0);
	commandToClient(%this, 'PlayGui_CreateToolHud', PlayerSpaceArmor.maxTools);

	SpaceRound.playerSet.add(%player);
	SpaceRound.player[%blid] = %player;

	%player.generateCharacter(%job);
	%player.initDamage();

	SpaceRound.name[%blid] = %player.character.realName;
	SpaceRound.characterSet.add(%player.character);

	%player.character.clientName = %this.getPlayerName();

	if (isObject(%this.camera))
		%this.camera.unMountImage(0);

	%this.player = %player;
	%this.setControlObject(%player);

	messageClient(%this, 'MsgYourSpawn');
	%this.hasSpawnedOnce = 1;
}

function GameConnection::spaceGhost(%this)
{
	commandToClient(%this, 'ClearCenterPrint');
	commandToClient(%this, 'ClearBottomPrint');

	%this.setControlObject(%this.camera);
	// put %camera somewhere instead
	%this.camera.setControlObject(%this.camera);
	%this.camera.setFlyMode();
}

function SpaceRound::onAdd(%this)
{
	%this.playerSet = new SimSet();
	%this.corpseSet = new SimSet();
	%this.itemSet = new SimSet();
	%this.garbageSet = new SimSet();
	%this.characterSet = new SimSet();

	%this.startTime = $Sim::Time;
	%this.killCount = 0;
	%this.ended = 0;

	%this.roleCount = 23;
	%this.rolePointer = 0;

	%this.role0  = "Captain";
	%this.role1  = "Head of Personnel";
	%this.role2  = "Head of Security";
	%this.role3  = "Chief Engineer";
	%this.role4  = "Detective";
	%this.role5  = "Chief Medical Officer";
	%this.role6  = "Research Director";
	%this.role7  = "Warden";
	%this.role8  = "Station Engineer";
	%this.role9  = "Medical Doctor";
	%this.role10 = "Security Officer";
	%this.role11 = "Scientist";
	%this.role12 = "Chemist";
	%this.role13 = "Medical Doctor";
	%this.role14 = "Roboticist";
	%this.role15 = "Security Officer";
	%this.role16 = "Atmospheric Technician";
	%this.role17 = "Chef";
	%this.role18 = "Botanist";
	%this.role19 = "Station Engineer";
	%this.role20 = "Botanist";
	%this.role21 = "Station Engineer";
	%this.role22 = "Security Officer";
}

function SpaceRound::onRemove(%this)
{
	%this.playerSet.deleteAll();
	%this.corpseSet.deleteAll();
	%this.itemSet.deleteAll();
	%this.garbageSet.deleteAll();
	%this.characterSet.deleteAll();

	%this.playerSet.delete();
	%this.corpseSet.delete();
	%this.itemSet.delete();
	%this.garbageSet.delete();
	%this.characterSet.delete();
}

function SpaceRound::pickJob(%this)
{
	if (%this.rolePointer >= %this.roleCount)
		return "Assistant";

	%job = %this.role[%this.rolePointer];
	%this.rolePointer++;

	return %job;
}

function SpaceRound::onKill(%this, %killer, %victim)
{
	%victim.killedBy = %killer;
	%victim.killedTime = $Sim::Time - %this.startTime;

	if (%killer.killCount $= "")
		%killer.killCount = 0;

	%this.killKiller[%this.killCount] = %killer;
	%this.killVictim[%this.killCount] = %victim;
	%this.killTime[%this.killCount] = $Sim::Time - %this.startTime;

	%this.killCount++;

	%killer.killVictim[%killer.killCount] = %victim;
	%Killer.killTime[%killer.killCount] = $Sim::Time - %this.startTime;
	%killer.killCount++;
}

function SpaceRound::addKill(%this, %killer, %victim, %type)
{
	// %this.killKiller[%this.killCount] = %killer;
	// %this.killVictim[%this.killCount] = %victim;
	// %this.killType[%this.killCount] = %type;

	// %this.killCount++;
}

function SpaceRound::end(%this, %message)
{
	if (isEventPending(%this.endSchedule))
		return;

	messageAll('', "<color:444444>==================");
	messageAll('', "<color:FFFFFF>The round is over!");

	if (%message !$= "")
		messageAll('', "<font:lucida console:14>  " @ %message);

	messageAll('', "<color:444444>------------------");
	messageAll('', "<color:FFFFFF>Cast and crew:");

	%count = %this.characterSet.getCount();

	for (%i = 0; %i < %count; %i++)
	{
		%char = %this.characterSet.getObject(%i);
		%line = "  <color:" @ (%char.isTraitor ? "FF6666" : "66FF66") @ ">" @ %char.realName @ " (" @ %char.clientName @ ") \c6as the " @ %char.job;

		if (%char.killedBy)
			%line = %line @ "\c6, <color:FF6666>killed by " @ %char.killedBy.realName;

		messageAll('', %line);
	}

	messageAll('', "<color:444444>------------------");
	messageAll('', "<color:FFFFFF>Kill log:");

	for (%i = 0; %i < %this.killCount; %i++)
	{
		%time = getTimeString(mCeil(%this.killTime[%i]));

		%killer = %this.killKiller[%i];
		%victim = %this.killVictim[%i];

		%killerText = "<color:" @ (%killer.isTraitor ? "FF6666" : "66FF66") @ ">" @ %killer.realName @ " (" @ %killer.clientName @ ")";
		%victimText = "<color:" @ (%victim.isTraitor ? "FF6666" : "66FF66") @ ">" @ %victim.realName @ " (" @ %victim.clientName @ ")";

		%line = "  \c7" @ %time @ ": " @ %killerText @ " \c6killed \c3" @ %victimText;
		messageAll('', %line);
	}

	%this.endSchedule = %this.miniGame.schedule(10000, "endSpace");
}

function SpaceRound::checkEnd(%this)
{
	if (isEventPending(%this.endSchedule))
		return;

	%living = %this.playerSet.getCount();

	for (%i = 0; %i < %living; %i++)
	{
		if (%this.playerSet.getObject(%i).character.isTraitor)
			%traitor = 1;
		else
			%normal = 1;
	}

	if (%traitor && !%normal)
		%this.end("<color:FFEE33>The traitor succeeded with their objectives: Kill everyone.");
	else if (!%traitor && %normal)
		%this.end("<color:44FF33>The traitor is dead!");
	else if (!%traitor && !%normal)
		%this.end("\c6Everyone is dead.");

	// if (%living < 1)
	// 	%this.end("Everybody is dead. Merasmus is proud.");
	// else if (%living < 2)
	// 	%this.end(%this.playerSet.getObject(0).clientName @ " is a cunt.");
}

function MiniGameSO::endSpace(%this)
{
	if (isObject(%this.spaceRound))
		%this.spaceRound.delete();

	for (%i = 0; %i < %this.numMembers; %i++)
	{
		%this.member[%i].setControlObject(%this.member[%i].camera);
		%this.member[%i].camera.setControlObject(%this.member[%i].dummyCamera);
		%this.member[%i].camera.setFlyMode();
		%this.member[%i].camera.setTransform($IntroCamera);
		%this.member[%i].spaceReady = 0;
	}

	for (%i = 0; %i < %this.numMembers; %i++)
		%this.member[%i].spaceIntro();
}

function GameConnection::isInSpaceGame(%this)
{
	return isObject(%this.miniGame.spaceRound) && %this.spaceReady;
}

function generateCodePhrase()
{
	%words = getRandom(2, 4);

	for (%i = 0; %i < %words; %i++)
	{
		if (%i)
			%phrase = %phrase @ ", ";

		if (getRandom() < 0.05)
			%phrase = %phrase @ getRandomName(getRandom(1) ? "first_male" : "first_female") SPC getRandomName("last");
		else if (getRandom() < 0.05)
			%phrase = %phrase @ getRandomName("jobs");
		else
		{
			%rand = getRandom(2);

			switch (%rand)
			{
				case 0: %phrase = %phrase @ getRandomName("nouns");
				case 1: %phrase = %phrase @ getRandomName("adjectives");
				case 2: %phrase = %phrase @ getRandomName("verbs");
			}
		}
	}

	return %phrase;
}

package SpaceGame
{
	function MiniGameSO::addMember(%this, %client)
	{
		if (%this.owner != 0)
			return Parent::addMember(%this, %client);

		%blid = %client.getBLID();

		for (%i = 0; %i < %this.numMembers; %i++)
		{
			if (%this.member[%i].getBLID() == %blid)
			{
				%this.removeMember(%this.member[%i]);
				%i--;
			}
		}

		Parent::addMember(%this, %client);
		%round = %this.spaceRound;

		//if (isObject(%round))
		if (isObject(SpaceRound) && SpaceRound.joined[%blid])
		{
			if (isObject(%round.player[%blid]))
				%client.spaceTakeControl(%round.player[%blid]);
			else
				%client.spaceGhost();
		}
		else
		{
			// just start a new round immediately for now?
			//%this.reset(0);
			%client.setControlObject(%client.camera);
			%client.camera.setControlObject(%client.dummyCamera);
			%client.camera.setFlyMode();
			%client.camera.setTransform($IntroCamera);

			%client.spaceReady = 0;
			%client.spaceIntro();
		}
	}

	function Armor::onRemove(%this, %obj)
	{
		Parent::onRemove(%this, %obj);
	}

	function MiniGameSO::removeMember(%this, %client)
	{
		if (%this.owner != 0)
			return Parent::removeMember(%this, %client);

		if (isObject(%client.player))
		{
			%client.player.client = "";
			%client.player = "";
		}

		Parent::removeMember(%this, %client);
		%client.instantRespawn();

		if (isObject(%round))
			%round.checkEnd();
	}

	function MiniGameSO::reset(%this, %client)
	{
		if (%this.owner != 0)
			return Parent::reset(%this, %client);

		if (getSimTime() - %this.lastResetTime <= 5000)
			return;

		if (isObject(%this.spaceRound))
			%this.spaceRound.delete();

		Parent::reset(%this, %client);

		%round = %this.spaceRound = new ScriptObject(SpaceRound)
		{
			miniGame = %this;
		};

		%this.divideJobs();

		// pick a traitor! ;)
		%numMembers = 0;

		for (%i = 0; %i < %this.numMembers; %i++)
		{
			if (isObject(%this.member[%i].player.character))
			{
				%member[%numMembers] = %this.member[%i];
				%numMembers++;
			}
		}

		//%traitors = getMax(1, mFloor(%numMembers / 3.5));
		%traitors = getMax(1, mFloor(%numMembers / 3));

		%phrase1 = generateCodePhrase();
		%phrase2 = generateCodePhrase();

		for (%i = 0; %i < %traitors && %numMembers; %i++)
		{
			%choice = %member[%index = getRandom(%numMembers--)];

			for (%j = %index; %j <= %numMembers; %j++)
				%member[%j] = %member[%j + 1];

			%player = %choice.player;
			%player.character.isTraitor = 1;

			%player.setTool(2, TraitorPDAItem);
			%player.getToolProps(2).text = "\c6Your objective is to kill everyone but other traitors.\n\c6Dispose of this PDA so it won't be found.\n\c6Code phrase: \c3" @ %phrase1 @ "\n\c6Code response: \c3" @ %phrase2 @ "\n";

			// complimentary gun
			%player.setTool(3, PistolItem);

			messageClient(%choice, '', "<font:palatino linotype:48>\c6You are a Traitor. Check your Traitor PDA for further details.");
		}

		for (%i = 0; %i < %this.numMembers; %i++)
		{
			%choice = %this.member[%i];

			if (!isObject(%choice.player.character) || !%this.client.isAdmin || %choice.player.character.isTraitor)
				continue;

			%player = %choice.player;
			%player.character.isTraitor = 1;

			%player.setTool(2, TraitorPDAItem);
			%player.getToolProps(2).text = "\c6Your objective is to kill everyone but other traitors.\n\c6Dispose of this PDA so it won't be found.\n\c6Code phrase: \c3" @ %phrase1 @ "\n\c6Code response: \c3" @ %phrase2 @ "\n";

			// complimentary gun
			%player.setTool(3, PistolItem);

			messageClient(%choice, '', "<font:palatino linotype:48>\c6You are a Traitor. Check your Traitor PDA for further details.");
		}
	}

	function GameConnection::spawnPlayer(%this)
	{
		if (!isObject(%this.miniGame) || %this.miniGame.owner != 0)
			return Parent::spawnPlayer(%this);
	}
};

activatePackage("SpaceGame");