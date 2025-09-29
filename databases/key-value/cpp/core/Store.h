#include <cstdint>
#include <string>
#include "Hashing.h"

class Store
{
private:
    static const int mArraySize = 64;
    int mArrayCount = 0;
    std::string mBackingStore[mArraySize];
    Hashing *mHashing;

    void ResizeBackingStore();

public:
    Store();
    void Put(std::string key, std::string value);
    std::string Get(std::string key);
};