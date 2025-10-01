#include <cstdint>
#include <string>
#include "Store.h"

Store::Store()
{
    mHashing = new Hashing();
    LoadDataFromDisk();
}

void Store::Put(std::string key, std::string value)
{
    uint64_t hashKey = mHashing->GetHashKey(key);
    int arrayIndex = hashKey % mBucketSize;
    mBackingStore[arrayIndex] = value;
    mArrayCount++;
}

std::string Store::Get(std::string key)
{
    uint64_t hashKey = mHashing->GetHashKey(key);
    int arrayIndex = hashKey % mBucketSize;
    return mBackingStore[arrayIndex];
}

bool Store::Flush()
{
    return true;
}

void Store::LoadDataFromDisk()
{
}