function loadNameList(%listName, %fileName, %append)
{
	if (!%append)
		deleteVariables("$NameList" @ %listName @ "*");

	$NameListCount[%listName] = 0;

	%fp = new FileObject();
	%fp.openForRead(%fileName);

	while (!%fp.isEOF())
	{
		%line = %fp.readLine();

		if (%line $= "")
			continue;

		if (getSubStr(ltrim(%line), 0, 1) $= "#")
			continue;

		$NameList[$NameListCount[%listName], %listName] = %line;
		$NameListCount[%listName]++;
	}

	%fp.close();
	%fp.delete();
}

function getRandomName(%nameList)
{
	return $NameList[getRandom(0, $NameListCount[%nameList] - 1), %nameList];
}

function generateCharacter()
{
	%char = new ScriptObject()
	{
		class = "SSCharacter";
	};

	%char.gender = getRandom(0, 1);
	%char.age = getRandom(20, 50);
	%char.job = getRandomName("jobs");

	%char.lastName = getRandomName("last");
	%char.firstName = getRandomName("first_" @ (%gender ? "male" : "female"));

	return %char;
}

function SSCharacter::getFloatName(%this)
{
	return %this.job SPC "-" SPC %this.firstName SPC %this.lastName;
}

loadNameList("jobs", "Add-Ons/GameMode_Space_Test/data/names/jobs2.txt");
//loadNameList("items", "Add-Ons/GameMode_Space_Test/data/names/items.txt");

loadNameList("last", "Add-Ons/GameMode_Space_Test/data/names/last.txt");
loadNameList("first_female", "Add-Ons/GameMode_Space_Test/data/names/first_female.txt");
loadNameList("first_male", "Add-Ons/GameMode_Space_Test/data/names/first_male.txt");
loadNameList("last", "Add-Ons/GameMode_Space_Test/data/names/last.txt");
loadNameList("clown", "Add-Ons/GameMode_Space_Test/data/names/clown.txt");

loadNameList("verbs", "Add-Ons/GameMode_Space_Test/data/names/verbs.txt");
loadNameList("adjectives", "Add-Ons/GameMode_Space_Test/data/names/adjectives.txt");
loadNameList("nouns", "Add-Ons/GameMode_Space_Test/data/names/nouns.txt");