class LeeVector2d
{
private:
	float x;
	float y;
public:
	explicit LeeVector2d(float inx, float iny) :x(inx), y(iny)
	{
	}

	void Set(float inx, float iny);
	float GetLength();
	LeeVector2d* Normalize();
};