#include <cmath>
#include "LeeVector2d.h"

using std;

LeeVector2d::Set(float inx, float iny)
{
	x = inx;
	y = iny;
}

LeeVector2d::GetLength()
{
	float squared = (x*x + y*y);
	return sqrt(squared);
}

LeeVector2d LeeVector2d::Normalize()
{
	float length = GetLength();
	float xnorm = x / length;
	float ynorm = y / length;
	return new LeeVector2d(xnorm, ynorm);
}