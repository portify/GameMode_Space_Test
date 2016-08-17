$IntroStartTime = 90000;
$IntroCamera = "17.4194 68.0933 92.5835 -0.830928 -0.308865 -0.462775 1.3534";

datablock AudioProfile(TimerCountdownSound)
{
	fileName = "Add-Ons/GameMode_Space_Test/sounds/ui_timercountdown.wav";
	description = AudioDefault3D;
};

datablock ParticleData(NanotrasenLogo)
{
	//textureName = "Add-Ons/GameMode_Space_Test/icons/NTLogo";
	textureName = "Add-Ons/GameMode_Space_Test/icons/nanotrasen";
};

function serverCmdReady(%client)
{
	if (!isObject(%miniGame = %client.miniGame))
		return;

	if (isObject(%round = %miniGame.spaceRound))
	{
		if (!%client.spaceReady)
		{
			%client.spaceReady = 1;
			%client.spaceJoinRound(%miniGame.pickLateJob());
		}

		return;
	}

	%client.spaceReady = !%client.spaceReady;
	%miniGame.updateReadyTimer();
	%client.spaceIntro();
}

function GameConnection::spaceIntro(%this)
{
	cancel(%this.spaceIntro);

	if (!isObject(%miniGame = %this.miniGame))
		return;

	for (%i = 0; %i < %miniGame.numMembers; %i++)
	{
		if (%miniGame.member[%i].spaceReady)
			%ready++;
	}

	%wait = isEventPending(%miniGame.startSpaceSchedule);
	%left = getTimeRemaining(%miniGame.startSpaceSchedule) / 1000;

	%readyText = "\c3" @ %ready @ " out of " @ %miniGame.numMembers @ " \c6players are ready.";
	%leftText = "\c6Round starting in \c3" @ getTimeString(mCeil(%left)) @ "\c6.";

	if (isObject(%miniGame.spaceRound))
	{
		if (%this.spaceReady)
			return;

		%text = "<just:center>\c6Use \c3/ready \c6to join the ongoing round!";
	}
	else if (%this.spaceReady)
	{
		%text = "<just:center>\c6Use \c3/ready \c6to cancel joining the round. " @ %readyText @ "\n" @ %leftText;
		serverPlay3D(TimerCountdownSound);
	}
	else
	{
		if (%ready)
		{
			%text = "<just:center>\c6Use \c3/ready \c6to join the upcoming round. " @ %readyText @ "\n" @ %leftText;
			serverPlay3D(TimerCountdownSound);
		}
		else
			%text = "<just:center>\c6Use \c3/ready \c6to start a new round!";
	}

	//%this.centerPrint("<bitmap:Add-Ons/GameMode_Space_Test/icons/NTLogo>", 3);
	%this.centerPrint("<bitmap:Add-Ons/GameMode_Space_Test/icons/nanotrasen>", 3);

	%this.bottomPrint(%text @ "\n", 3, 1);
	%this.spaceIntro = %this.schedule(1000, "spaceIntro");
}

function MiniGameSO::updateReadyTimer(%this)
{
	if (isObject(%this.spaceRound))
		return;

	%ready = 0;

	for (%i = 0; %i < %this.numMembers; %i++)
	{
		if (%this.member[%i].spaceReady)
			%ready++;
	}

	if (%ready == 0)
		cancel(%this.startSpaceSchedule);
	else if (%ready == %this.numMembers)
	{
		cancel(%this.startSpaceSchedule);
		%this.reset(0);

		messageAll('', "\c6The round timer has been skipped due to everybody being ready.");
	}
	else if (!isEventPending(%this.startSpaceSchedule))
		%this.startSpaceSchedule = %this.schedule($IntroStartTime, "reset", 0);
}