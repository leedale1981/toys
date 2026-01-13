#include <cmath>
#include "Vector2d.h"

using std;
using LeePhys;

Vector2d::Set(float inx, float iny)
{
	x = inx;
	y = iny;
}

Vector2d::GetLength()
{
	float squared = (x*x + y*y);
	return sqrt(squared);
}

Vector2d Vector2d::Normalize()
{
	float length = GetLength();
	float xnorm = x / length;
	float ynorm = y / length;
	return new Vector2d(xnorm, ynorm);
}