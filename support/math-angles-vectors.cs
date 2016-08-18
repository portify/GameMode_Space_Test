function median(%a, %b, %c) {
	if ((%a >= %b && %a <= %b) || (%a <= %b && %a >= %b)) return %a;
	if ((%b >= %a && %b <= %c) || (%b <= %a && %b >= %c)) return %b;
	if ((%c >= %a && %c <= %b) || (%c <= %a && %c >= %b)) return %c;
}

function min(%a, %b) {
	return %a < %b ? %a : %b;
}

function min3(%a, %b, %c)
{
	return %a < %b ? (%a < %c ? %a : %c) : (%b < %c ? %b : %c);
}

function max(%a, %b) {
	return %a > %b ? %a : %b;
}

function max3(%a, %b, %c)
{
	return %a > %b ? (%a > %c ? %a : %c) : (%b > %c ? %b : %c);
}

function vectorMax(%vectorA, %vectorB)
{
	return getMax(getWord(%vectorA, 0), getWord(%vectorB, 0)) SPC getMax(getWord(%vectorA, 1), getWord(%vectorB, 1)) SPC getMax(getWord(%vectorA, 2), getWord(%vectorB, 2));
}
function vectorMin(%vectorA, %vectorB)
{
	return getMin(getWord(%vectorA, 0), getWord(%vectorB, 0)) SPC getMin(getWord(%vectorA, 1), getWord(%vectorB, 1)) SPC getMin(getWord(%vectorA, 2), getWord(%vectorB, 2));
}

function vectorSpread(%vector, %spread) {
	%x = (getRandom() - 0.5) * 10 * 3.1415926 * %spread;
	%y = (getRandom() - 0.5) * 10 * 3.1415926 * %spread;
	%z = (getRandom() - 0.5) * 10 * 3.1415926 * %spread;

	%mat = matrixCreateFromEuler(%x SPC %y SPC %z);
	return vectorNormalize(matrixMulVector(%mat, %vector));
}

function spreadVector(%vector, %spread)
{
	%scalars = randomScalar() SPC randomScalar() SPC randomScalar();
	%scalars = vectorScale(%scalars, mDegToRad(%spread));

	return matrixMulVector(matrixCreateFromEuler(%scalars), %vector);
}

function randomScalar()
{
	return getRandom() * 2 - 1;
}

function eulerToAxis(%euler)
{
	%euler = VectorScale(%euler,$pi / 180);
	%matrix = MatrixCreateFromEuler(%euler);
	return getWords(%matrix,3,6);
}

function axisToEuler(%axis)
{
	%angleOver2 = getWord(%axis,3) * 0.5;
	%angleOver2 = -%angleOver2;
	%sinThetaOver2 = mSin(%angleOver2);
	%cosThetaOver2 = mCos(%angleOver2);
	%q0 = %cosThetaOver2;
	%q1 = getWord(%axis,0) * %sinThetaOver2;
	%q2 = getWord(%axis,1) * %sinThetaOver2;
	%q3 = getWord(%axis,2) * %sinThetaOver2;
	%q0q0 = %q0 * %q0;
	%q1q2 = %q1 * %q2;
	%q0q3 = %q0 * %q3;
	%q1q3 = %q1 * %q3;
	%q0q2 = %q0 * %q2;
	%q2q2 = %q2 * %q2;
	%q2q3 = %q2 * %q3;
	%q0q1 = %q0 * %q1;
	%q3q3 = %q3 * %q3;
	%m13 = 2.0 * (%q1q3 - %q0q2);
	%m21 = 2.0 * (%q1q2 - %q0q3);
	%m22 = 2.0 * %q0q0 - 1.0 + 2.0 * %q2q2;
	%m23 = 2.0 * (%q2q3 + %q0q1);
	%m33 = 2.0 * %q0q0 - 1.0 + 2.0 * %q3q3;
	return mRadToDeg(mAsin(%m23)) SPC mRadToDeg(mAtan(-%m13, %m33)) SPC mRadToDeg(mAtan(-%m21, %m22));
}

//Thanks to Wrapperup for this one, though iirc it only works around z rotation
function RotatePointAroundPivot(%point, %pivot, %zrot)
{
	%dist = vectorDist(%point, %pivot);
	
	%norm = vectorNormalize(vectorSub(%point, %pivot));
	
	%xB = getWord(%norm, 0);
	%yB = getWord(%norm, 1);
	
	%angle = mRadToDeg(mATan(%xB,%yB));
	
	%newAngle = %angle + %zrot;
	
	%pos = mSin(mDegToRad(%newAngle)) SPC mCos(mDegToRad(%newAngle)) SPC 0;
	
	%pos = vectorScale(%pos, %dist);
	%pos = vectorAdd(%pos, %pivot);
	
	return %pos;
}