$SkinColorCount = 3;
$SkinColor0 = "0.392 0.196 0";
$SkinColor1 = "1 0.878 0.611";
$SkinColor2 = "0.956 0.878 0.784";

function Player::generateCharacter(%this, %job)
{
	if (%this.client.name $= "Port")
		%this.ambiguousGender = 1;

	if (isObject(%this.character))
		%this.character.delete();

	if (isObject(%this.clothing))
		%this.clothing.delete();

	%char = new ScriptObject()
	{
		class = "SSCharacter";
	};

	%this.character = %char;
	%this.clothing = new ScriptGroup();

	%char.gender = getRandom(0, 1);
	%char.age = getRandom(20, 50);
	//%char.job = getRandomName("jobs");
	%char.job = %job;

	if (0)
		%char.realName = getRandomName("clown");
	else
		%char.realName = getRandomName("first_" @ (%char.gender ? "male" : "female")) SPC getRandomName("last");

	%char.skinColor = $SkinColor[getRandom($SkinColorCount - 1)] SPC 1;
	%char.faceName = "smiley";

	%this.clearTools();
	%this.setTool(0, IDItem);
	%this.setTool(1, FlashlightItem);

	//%this.invHand1 = FlashlightItem.getID();
	%id = %this.getToolProps(0);

	%id.name = %char.realName;
	%id.gender = %char.gender;
	%id.age = %char.age;
	%id.job = %job;
	%id.accessFlagCount = $JobAccessCount[%job];

	for (%i = 0; %i < $JobAccessCount[%job]; %i++)
	{
		%flag = $JobAccessID[%job, %i];

		if (%flag $= "all")
			%id.accessFlag[%i] = -1;
		else
			%id.accessFlag[%i] = $AccessFlagID[%flag];
	}

	for (%i = 0; %i < $JobClothing[%job].length; %i++)
		%this.clothing.add($JobClothing[%job].value[%i].copy());

	//switch$ (%char.job)
	switch$ ("")
	{
	case "Captain":
		%this.addAccessFlag(-1);

		%this.clothing.add(new ScriptObject()
		{
			type = "Node";
			desc = "a Captain's hat";
			node = "copHat";
			color = "0 0.5 0.25 1";
		});

		// %this.clothing.add(new ScriptObject()
		// {
		// 	type = "Node";
		// 	desc = "A Rank epaulets";
		// 	node = "epauletsRankA";
		// 	color = "0.9 0.9 0 1";
		// });

		%this.clothing.add(new ScriptObject()
		{
			type = "Suit";
			desc = "a Captain's green suit";
			decal = "Mod-Pilot";
			color = "0 0.5 0.25 1";
			padNode = "epauletsRankA";
			padColor = "0.9 0.9 0 1";
		});

		%this.clothing.add(new ScriptObject()
		{
			type = "Shoes";
			desc = "generic grey boots";
			color = "0.2 0.2 0.2 1";
		});

	case "Head of Personnel":
		%this.addAccessFlag($AccessFlagID["Command"]);

		%this.clothing.add(new ScriptObject()
		{
			type = "Node";
			desc = "a blue cap";
			node = "copHat";
			color = "0 0.141 0.333 1";
		});

		%this.clothing.add(new ScriptObject()
		{
			type = "Suit";
			desc = "a blue airforce suit";
			decal = "Mod-Pilot";
			color = "0 0.141 0.333 1";
		});

		%this.clothing.add(new ScriptObject()
		{
			type = "Shoes";
			desc = "generic grey boots";
			color = "0.2 0.2 0.2 1";
		});

	case "Head of Security":
		%this.addAccessFlag($AccessFlagID["Command"]);
		%this.addAccessFlag($AccessFlagID["Security"]);

		%this.clothing.add(new ScriptObject()
		{
			type = "Hat";
			desc = "sunglasses";
			hatName = "Sunglasses";
		});

		%this.clothing.add(new ScriptObject()
		{
			type = "Node";
			desc = "a red cap";
			node = "copHat";
			color = "0.388 0 0.117 1";
		});

		%this.clothing.add(new ScriptObject()
		{
			type = "Node";
			desc = "B Rank epaulets";
			node = "epauletsRankB";
			color = "0.9 0.9 0 1";
		});

		%this.clothing.add(new ScriptObject()
		{
			type = "Suit";
			desc = "a red police jumpsuit";
			decal = "Mod-Police";
			color = "0.388 0 0.117 1";
		});

		%this.clothing.add(new ScriptObject()
		{
			type = "Shoes";
			desc = "generic grey boots";
			color = "0.2 0.2 0.2 1";
		});

	case "Warden":
		%this.addAccessFlag($AccessFlagID["Security"]);

		%this.clothing.add(new ScriptObject()
		{
			type = "Node";
			desc = "D Rank epaulets";
			node = "epauletsRankD";
			color = "0.9 0.9 0 1";
		});

		%this.clothing.add(new ScriptObject()
		{
			type = "Suit";
			desc = "a red police jumpsuit";
			decal = "Mod-Police";
			color = "0.588 0 0 1";
		});

		%this.clothing.add(new ScriptObject()
		{
			type = "Shoes";
			desc = "generic grey boots";
			color = "0.2 0.2 0.2 1";
		});

	case "Security Officer":
		%this.addAccessFlag($AccessFlagID["Security"]);

		%this.clothing.add(new ScriptObject()
		{
			type = "Hat";
			desc = "a cop hat";
			hatName = "Cop";
		});

		%this.clothing.add(new ScriptObject()
		{
			type = "Suit";
			desc = "a blue police suit";
			decal = "Mod-Police";
			color = "0 0.336 0.5 1";
		});

		%this.clothing.add(new ScriptObject()
		{
			type = "Shoes";
			desc = "generic grey boots";
			color = "0.2 0.2 0.2 1";
		});

	case "Detective":
		%this.clothing.add(new ScriptObject()
		{
			type = "Hat";
			desc = "a classic Deerstalker detective hat";
			hatName = "Detective";
		});

		%this.clothing.add(new ScriptObject()
		{
			type = "Suit";
			desc = "a tan suit";
			decal = "Mod-Suit";
			color = "0.794 0.607 0.471 1";
		});

		%this.clothing.add(new ScriptObject()
		{
			type = "Shoes";
			desc = "generic grey boots";
			color = "0.2 0.2 0.2 1";
		});

		%this.clothing.add(new ScriptObject()
		{
			type = "Gloves";
			desc = "black gloves";
			color = "0.2 0.2 0.2 1";
		});

	case "Chief Medical Officer":
		%this.clothing.add(new ScriptObject()
		{
			type = "Node";
			desc = "blue shoulderpads";
			node = "ShoulderPads";
			color = "0.105 0.458 0.768 1";
		});

		%this.clothing.add(new ScriptObject()
		{
			type = "Suit";
			desc = "a white labcoat";
			decal = "DrKleiner";
			color = "0.9 0.9 0.9 1";
		});

		%this.clothing.add(new ScriptObject()
		{
			type = "Shoes";
			desc = "generic grey boots";
			color = "0.2 0.2 0.2 1";
		});

	case "Medical Doctor":
		%this.clothing.add(new ScriptObject()
		{
			type = "Hat";
			desc = "a stethoscope";
			hatName = "Doctor";
		});

		%this.clothing.add(new ScriptObject()
		{
			type = "Suit";
			desc = "a white labcoat";
			decal = "DrKleiner";
			color = "0.9 0.9 0.9 1";
		});

		%this.clothing.add(new ScriptObject()
		{
			type = "Shoes";
			desc = "generic grey boots";
			color = "0.2 0.2 0.2 1";
		});

	case "Chemist":
		%this.clothing.add(new ScriptObject()
		{
			type = "Suit";
			desc = "a white labcoat with tan sleeves";
			decal = "DrKleiner";
			color = "0.9 0.9 0.9 1";
			sleeveColor = "0.901 0.341 0.078 1";
		});

		%this.clothing.add(new ScriptObject()
		{
			type = "Shoes";
			desc = "generic grey boots";
			color = "0.2 0.2 0.2 1";
		});

	case "Research Director":
		%this.clothing.add(new ScriptObject()
		{
			type = "Suit";
			desc = "a tan labcoat";
			decal = "DrKleiner";
			color = "0.901 0.341 0.078 1";
		});

		%this.clothing.add(new ScriptObject()
		{
			type = "Shoes";
			desc = "generic grey boots";
			color = "0.2 0.2 0.2 1";
		});

	case "Scientist":
		%this.clothing.add(new ScriptObject()
		{
			type = "Suit";
			desc = "a purple labcoat";
			decal = "DrKleiner";
			color = "0.892 0.845 0.976 1";
			sleeveColor = "0.644 0.327 0.612 1";
		});

		%this.clothing.add(new ScriptObject()
		{
			type = "Shoes";
			desc = "generic grey boots";
			color = "0.2 0.2 0.2 1";
		});

	case "Roboticist":
		%this.clothing.add(new ScriptObject()
		{
			type = "Suit";
			desc = "a grey labcoat";
			decal = "DrKleiner";
			color = "0.5 0.5 0.5 1";
			sleeveColor = "0.2 0.2 0.2 1";
		});

		%this.clothing.add(new ScriptObject()
		{
			type = "Shoes";
			desc = "generic grey boots";
			color = "0.2 0.2 0.2 1";
		});

	case "Chief Engineer":
		%this.clothing.add(new ScriptObject()
		{
			type = "Hat";
			desc = "a hardhat";
			hatName = "Hardhat";
		});

		%this.clothing.add(new ScriptObject()
		{
			type = "Node";
			desc = "orange shoulderpads";
			node = "ShoulderPads";
			color = "0.901 0.341 0.078 1";
		});

		%this.clothing.add(new ScriptObject()
		{
			type = "Suit";
			desc = "an orange jumpsuit";
			decal = "worm_engineer";
			color = "0.901 0.341 0.078 1";
		});

		%this.clothing.add(new ScriptObject()
		{
			type = "Shoes";
			desc = "generic grey boots";
			color = "0.2 0.2 0.2 1";
		});

	case "Station Engineer":
		%this.clothing.add(new ScriptObject()
		{
			type = "Hat";
			desc = "a hardhat";
			hatName = "Hardhat";
		});

		%this.clothing.add(new ScriptObject()
		{
			type = "Suit";
			desc = "an orange jumpsuit";
			decal = "worm_engineer";
			color = "0.901 0.341 0.078 1";
		});

		%this.clothing.add(new ScriptObject()
		{
			type = "Shoes";
			desc = "generic grey boots";
			color = "0.2 0.2 0.2 1";
		});

	case "Atmospheric Technician":
		%this.clothing.add(new ScriptObject()
		{
			type = "Hat";
			desc = "a headset";
			hatName = "Headset";
		});

		%this.clothing.add(new ScriptObject()
		{
			type = "Suit";
			desc = "an orange jumpsuit";
			decal = "worm_engineer";
			color = "0.901 0.341 0.078 1";
			sleeveColor = "0.9 0.9 0 1";
		});

		%this.clothing.add(new ScriptObject()
		{
			type = "Shoes";
			desc = "generic grey boots";
			color = "0.2 0.2 0.2 1";
		});

	case "Chef":
		%this.clothing.add(new ScriptObject()
		{
			type = "Suit";
			desc = "a chef's suit";
			decal = "Chef";
			color = "0.9 0.9 0.9 1";
		});

		%this.clothing.add(new ScriptObject()
		{
			type = "Shoes";
			desc = "generic grey boots";
			color = "0.2 0.2 0.2 1";
		});

	case "Botanist":
		%this.clothing.add(new ScriptObject()
		{
			type = "Hat";
			desc = "a Ragtime hat";
			hatName = "Ragtime";
		});

		%this.clothing.add(new ScriptObject()
		{
			type = "Suit";
			desc = "a green jumpsuit";
			decal = "worm_engineer";
			color = "0 0.5 0.25 1";
		});

		%this.clothing.add(new ScriptObject()
		{
			type = "Shoes";
			desc = "generic grey boots";
			color = "0.2 0.2 0.2 1";
		});

	default:
	// 	echo("err" SPC %char.job);
	// 	%this.clothing.add(new ScriptObject()
	// 	{
	// 		type = "";
	// 		desc = "invalid clothing for job: " @ %char.job;
	// 	});
	}

	%this.applyCharacter();
}

function Player::applyCharacter(%this)
{
	%char = %this.character;

	if (!isObject(%char))
		return;

	%this.hideNode("ALL");
	%this.setHeadUp(0);

	if (!%this.isCorpse)
	{
		%this.setShapeName(%char.realName, 8564862);
		%this.setShapeNameColor("1 1 1");
		%this.setShapeNameDistance(12);
	}

	%this.unHideNode("headSkin");
	%this.unHideNode(%char.gender ? "chest" : "femChest");
	%this.unHideNode("lhand");
	%this.unHideNode("rhand");
	%this.unHideNode("larm");
	%this.unHideNode("rarm");
	%this.unHideNode("lshoe");
	%this.unHideNode("rshoe");
	%this.unHideNode("pants");

	%headColor = %char.skinColor;
	%chestColor = %char.skinColor;
	%lhandColor = %char.skinColor;
	%rhandColor = %char.skinColor;
	%larmColor = %char.skinColor;
	%rarmColor = %char.skinColor;
	%lshoeColor = %char.skinColor;
	%rshoeColor = %char.skinColor;
	%pantsColor = %char.skinColor;
	%hatName = "";
	%decalName = "";

	%this.setFaceName(%char.faceName);
	%count = %this.clothing.getCount();

	for (%i = 0; %i < %count; %i++)
	{
		%item = %this.clothing.getObject(%i);

		switch$ (%item.type)
		{
		case "Node":
			%this.unHideNode(%item.node);
			%this.setNodeColor(%item.node, %item.color);
		case "Hat":
			%hatName = %item.hatName;
		case "Suit":
			%decalName = %item.decal;
			%chestColor = %item.color;
			%pantsColor = %item.color;
			%larmColor = %item.sleeveColor $= "" ? %item.color : %item.sleeveColor;
			%rarmColor = %item.sleeveColor $= "" ? %item.color : %item.sleeveColor;

			if (%item.padNode !$= "" && %item.padColor !$= "")
			{
				%this.unHideNode(%item.padNode);
				%this.setNodeColor(%item.padNode, %item.padColor);
			}
		//case "Shoes":
		case "Shoe":
			%lshoeColor = %item.color;
			%rshoeColor = %item.color;
		//case "Gloves":
		case "Glove":
			%lshoeColor = %item.color;
			%rshoeColor = %item.color;
		case "Mask":
			%headColor = %item.color;
		}
	}

	%this.setNodeColor("headSkin", %headColor);
	%this.setNodeColor(%char.gender ? "chest" : "femChest", %chestColor);
	%this.setNodeColor("lhand", %lhandColor);
	%this.setNodeColor("rhand", %rhandColor);
	%this.setNodeColor("larm", %larmColor);
	%this.setNodeColor("rarm", %rarmColor);
	%this.setNodeColor("lshoe", %lshoeColor);
	%this.setNodeColor("rshoe", %rshoeColor);
	%this.setNodeColor("pants", %pantsColor);

	%this.mountHat(%hatName);
	%this.setDecalName(%decalName);
}