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
    int arrayIndex = hashKey % (mBackingStore.size() + 1);
    mBackingStore.insert(mBackingStore.begin() + arrayIndex, value);
    mArrayCount++;
}

std::string Store::Get(std::string key)
{
    uint64_t hashKey = mHashing->GetHashKey(key);
    int arrayIndex = hashKey % mBackingStore.size() + 1;
    return mBackingStore[arrayIndex];
}