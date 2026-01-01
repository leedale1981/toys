#include <cstdint>
#include <string>
#include "Hashing.h"

class Store
{
private:
    static const int mBucketSize = 1024;
    const std::string mFileName = "leedb_data.bin";
    int mArrayCount = 0;
    std::string mBackingStore[mBucketSize];
    Hashing *mHashing;

    void LoadData();

public:
    Store();
    void Put(std::string key, std::string value);
    std::string Get(std::string key);
    void Delete(std::string key);
    void Flush();
};