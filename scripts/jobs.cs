function reloadJobs()
{
	// first, clear all data
	for (%i = 0; %i < $JobCount; %i++)
	{
		%name = $JobName[%i];
		$JobName[%i] = "";
		$JobHead[%name] = "";

		%division = $JobDivision[%name];
		$JobDivision[%name] = "";

		if ($JobDivisionListCount[%division] !$= "")
		{
			for (%j = 0; %j < $JobDivisionListCount[%division]; %j++)
				$JobDivisionListName[%division, %j] = "";

			$JobDivisionListCount[%division] = "";
		}

		for (%j = 0; %j < $JobAccessCount[%name]; %j++)
			$JobAccessID[%name, %j] = "";

		$JobAccessCount[%name] = "";

		$JobClothing[%name].delete();
		$JobClothing[%name] = "";
	}

	$JobCount = 0;

	echo("Loading job data...");
	%data = JSON::parseFile("Add-Ons/GameMode_Space_Test/data/jobs.json");

	if (!isObject(%data))
	{
		error("ERROR: Failed to load job data!");
		$JobCount = 0;
		return;
	}

	for (%i = 0; %i < %data.__keys.length; %i++)
	{
		%name = %data.__keys.value[%i];
		%job = %data.get(%name);

		$JobName[$JobCount] = %name;
		$JobCount++;

		$JobHead[%name] = %job.head;
		$JobDivision[%name] = %division = %job.division;

		if ($JobDivisionListCount[%division] $= "")
			$JobDivisionListCount[%division] = 0;

		$JobDivisionListName[%division, $JobDivisionListCount[%division]] = %name;
		$JobDivisionListCount[%division]++;

		$JobAccessCount[%name] = %job.access.length;

		for (%j = 0; %j < %job.access.length; %j++)
			$JobAccessID[%name, %j] = %job.access.value[%j];

		$JobClothing[%name] = ref(%job.clothing);
	}

	echo("Done loading job data.");
	%data.delete();
}

function JobPicker(%miniGame)
{
	%obj = new ScriptObject()
	{
		class = "JobPicker";
		numMembers = 0;
	};

	for (%i = 0; %i < %miniGame.numMembers; %i++)
	{
		%member = %miniGame.member[%i];

		if (%member.spaceReady)
		{
			%obj.member[%obj.numMembers] = %member;
			%obj.numMembers++;
		}
	}

	%obj.numJobs = 0;

	for (%i = 0; %i < $JobCount; %i++)
	{
		if ($JobName[%i] $= "Assistant")
			continue;

		%obj.job[%i] = $JobName[%i];
		%obj.numJobs++;
	}

	return %obj;
}

function JobPicker::assign(%this, %job)
{
	if (!%this.numMembers)
		return 0;

	if (%job !$= "Assistant")
	{
		for (%i = 0; %i < %this.numJobs; 0)
		{
			if (%found)
			{
				%this.job[%i] = %this.job[%i + 1];
				%i++;
			}
			else if (%this.job[%i] $= %job)
				%found = 1;
			else
				%i++;
		}

		if (%found)
			%this.job[%this.numJobs--] = "";
		else
			return 0;
	}

	%memberIndex = getRandom(%this.numMembers--);
	%member = %this.member[%memberIndex];

	for (%i = %memberIndex; %i <= %this.numMembers; %i++)
		%this.member[%i] = %this.member[%i + 1];

	%member.spaceJoinRound(%job);
	return 1;
}

function JobPicker::getRandomHead(%this)
{
	%count = 0;

	for (%i = 0; %i < %this.numJobs; %i++)
	{
		if ($JobHead[%this.job[%i]])
		{
			%job[%count] = %this.job[%i];
			%count++;
		}
	}

	return %job[getRandom(%count - 1)];
}

function MiniGameSO::divideJobs(%this)
{
	%picker = JobPicker(%this);
	%i = -1;

	while (%picker.numMembers && %picker.numJobs)
	{
		if (%i++ % 3 == 0)
		{
			%head = %picker.getRandomHead();

			if (%head !$= "")
			{
				%picker.assign(%head);
				continue;
			}
		}

		%picker.assign(%picker.job[getRandom(%picker.numJobs - 1)]);
	}

	while (%picker.numMembers)
		%picker.assign("Assistant");

	// this is really bad.
	%length = %picker.numJobs - 1;

	for (%i = 0; %i <= %length; %i++)
	{
		%other = getRandom(%i, %picker.numJobs);
		%temp = %picker.job[%i];

		%picker.job[%i] = %picker.job[%other];
		%picker.job[%other] = %temp;
	}

	for (%i = 0; %i < %picker.numJobs; %i++)
		%this.availableJob[%i] = %picker.job[%i];

	%this.availableCount = %picker.numJobs;
	%picker.delete();
}

function MiniGameSO::pickLateJob(%this)
{
	if (%this.availableCount)
	{
		%job = %this.availableJob0;
		%this.availableCount--;

		for (%i = 0; %i <= %this.availableCount; %i++)
			%this.availableJob[%i] = %this.availableJob[%i + 1];

		return %job;
	}

	return "Assistant";
}

if ($JobCount $= "")
	reloadJobs();