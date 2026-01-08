#include "LeeVector2d.h"
#include <functional>

class Velocity
{
public:
	Velocity(LeeVector2d* vector, std::function<float(float)> velocityFunction);
	float GetAverageSpeed(float time1, float time2);
private:
	LeeVector2d* mVector;
	std::function<float(float)> mVelocityFunction
};