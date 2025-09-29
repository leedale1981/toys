#include <cstdint>
#include <string>
#include "Store.h"

Store::Store()
{
    mHashing = new Hashing();
}

void Store::Put(std::string key, std::string value)
{
    uint64_t hashKey = mHashing->GetHashKey(key);
    int arrayIndex = hashKey % mBackingStore->length();
    mBackingStore[arrayIndex] = value;
    mArrayCount++;
}

std::string Store::Get(std::string key)
{
    uint64_t hashKey = mHashing->GetHashKey(key);
    int arrayIndex = hashKey % mBackingStore->length();
    return mBackingStore[arrayIndex];
}