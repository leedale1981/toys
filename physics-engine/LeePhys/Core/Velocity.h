#include "Vector2d.h"
#include <functional>

using namespace LeePhys;

class Velocity
{
public:
	Velocity(Vector2d* vector, std::function<float(float)> velocityFunction);
	float GetAverageSpeed(float time1, float time2);
private:
	Vector2d* mVector;
	std::function<float(float)> mVelocityFunction
};