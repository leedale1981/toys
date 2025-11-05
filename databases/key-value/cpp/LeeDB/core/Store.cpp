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
    Flush();
    mArrayCount++;
}

string Store::Get(string key)
{
    uint64_t hashKey = mHashing->GetHashKey(key);
    int arrayIndex = hashKey % mBucketSize;
    return mBackingStore[arrayIndex];
}

void Store::Flush()
{
    ofstream outStream(mFileName, ios::binary);

    if (!outStream)
    {
        throw runtime_error("Could not open data file!");
    }

    outStream.write(reinterpret_cast<const char *>(&mBucketSize), sizeof(mBucketSize));

    for (uint64_t index = 0; index < mBucketSize; ++index)
    {
        const string value = mBackingStore[index];
        const uint64_t len = static_cast<uint64_t>(value.length()); // bytes, no terminator
        outStream.write(reinterpret_cast<const char *>(&len), sizeof(len));

        if (len)
        {
            outStream.write(value.data(), len);
        }
    }

    if (!outStream.good())
    {
        throw runtime_error("write error");
    }
}

void Store::LoadData()
{
    ifstream inStream(mFileName, ios::binary);

    if (!inStream)
    {
        return;
    }

    uint64_t fileBucketSize = 0;
    inStream.read(reinterpret_cast<char *>(&fileBucketSize), sizeof(fileBucketSize));

    if (!inStream)
    {
        throw std::runtime_error("Failed to read bucket size");
    }

    if (fileBucketSize != mBucketSize)
    {
        throw std::runtime_error("Bucket size in file does not match compiled size");
    }

    for (uint64_t index = 0; index < mBucketSize; ++index)
    {
        uint64_t len = 0;
        inStream.read(reinterpret_cast<char *>(&len), sizeof(len));

        if (!inStream)
        {
            throw std::runtime_error("Unexpected end of file while reading string length");
        }

        std::string value;

        if (len)
        {
            value.resize(static_cast<size_t>(len));
            inStream.read(value.data(), static_cast<std::streamsize>(len));

            if (!inStream)
            {
                throw std::runtime_error("Unexpected end of file while reading string data");
            }
        }

        mBackingStore[index] = std::move(value);
    }

    if (!inStream.good() && !inStream.eof())
    {
        throw std::runtime_error("Read error");
    }
}