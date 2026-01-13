namespace LeePhys
{
	class Vector2d
	{
	private:
		float x;
		float y;
	public:
		explicit Vector2d(float inx, float iny) :x(inx), y(iny)
		{
		}

		void Set(float inx, float iny);
		float GetLength();
		Vector2d* Normalize();
	};
}