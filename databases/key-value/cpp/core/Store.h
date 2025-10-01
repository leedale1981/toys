#include <cstdint>
#include <string>
#include "Hashing.h"

class Store
{
private:
    static const int mBucketSize = 1024;
    int mArrayCount = 0;
    std::string mBackingStore[mBucketSize];
    Hashing *mHashing;

    void LoadDataFromDisk();

public:
    Store();
    void Put(std::string key, std::string value);
    std::string Get(std::string key);
    bool Flush();
};