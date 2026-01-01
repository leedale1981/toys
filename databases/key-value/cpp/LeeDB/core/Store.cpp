#include <cstdint>
#include <string>
#include <fstream>
#include "Store.h"

using namespace std;

Store::Store()
{
    mHashing = new Hashing();
    LoadData();
}

void Store::Put(string key, string value)
{
    uint64_t hashKey = mHashing->GetHashKey(key);
    int arrayIndex = hashKey % mBucketSize;
    mBackingStore[arrayIndex] = value;
    mArrayCount++;
    Flush();
}

void Store::Delete(string key)
{
    uint64_t hashKey = mHashing->GetHashKey(key);
    int arrayIndex = hashKey % mBucketSize;
    mBackingStore[arrayIndex] = "";
    Flush();
}

string Store::Get(string key)
{
    uint64_t hashKey = mHashing->GetHashKey(key);
    int arrayIndex = hashKey % mBucketSize;
    return mBackingStore[arrayIndex];
}

void Store::Flush()
{
    FILE* file;
    fopen_s(&file, mFileName.c_str(), "wb");

    if (file != NULL)
    {
        fwrite(mBackingStore, sizeof(mBackingStore[0]), sizeof(mBackingStore) / sizeof(mBackingStore[0]), file);
        fclose(file);
    }
}

void Store::LoadData()
{
    FILE* file; 
    fopen_s(&file, mFileName.c_str(), "rb");

    if (file)
    {
        fread(mBackingStore, sizeof(mBackingStore[0]), sizeof(mBackingStore) / sizeof(mBackingStore[0]), file);
        fclose(file);
    }
}