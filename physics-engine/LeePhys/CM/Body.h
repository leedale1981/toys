#include "..\Core\Vector2d.h"

class Body
{
private:
	Vector2d mPosition;
	Vector2d mVelocity;
	Vector2d mForce;
	float mass;
	float radius;

public:
	Body(Vector2d position, Vector2d mass);

};