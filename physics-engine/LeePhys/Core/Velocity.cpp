#include "Velocity.h"

Velocity::Velocity(LeeVector2d* vector, std::function<float(float)> velocityFunction)
{
	mVector = vector;
	mVelocityFunction = velocityFunction;
}

Velocity::GetAverageSpeed(float time1, float time2)
{
	float velocityAtTime1 = mVelocityFunction(time1);
	float velocityAtTime2 = mVelocityFunction(time2);

	return (velocityAtTime2 - velocityAtTime1) / (time2 - time1);
}
